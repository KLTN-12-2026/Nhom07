using EzCondo.BackEndServer.Data;
using EzCondo.BackEndServer.Data.Entities.SchemaPayment;
using EzCondo.BackEndServer.Data.Enums;
using EzCondo.BackEndServer.Infrastructure.Payments.PayOS;
using EzCondo.BackEndServer.Infrastructure.Payments.PayOS.Models;
using Microsoft.EntityFrameworkCore;

namespace EzCondo.BackEndServer.Services.Payments.PayOS
{
	public sealed class PayOSWebhookProcessingService(
		ApplicationDbContext dbcontext,
		IPayOSGateway payOSGateway,
		TimeProvider timeProvider,
		ILogger<PayOSWebhookProcessingService> logger)
		: IPayOSWebhookProcessingService
	{
		public async Task<PayOSWebhookProcessingResult> ProcessAsync(
			Guid webhookKey, 
			string rawPayload, 
			CancellationToken cancellationToken = default)
		{
			if(webhookKey == Guid.Empty)
			{
				return new(
					PayOSWebhookProcessingStatus.ProviderAccountNotFound,
					"Webhookey không hợp lệ.");
			}

			if (string.IsNullOrWhiteSpace(rawPayload))
			{
				return new(
					PayOSWebhookProcessingStatus.InvalidPayload,
					"Payload webhook không được để trống.");
			}

			var providerAccount = await dbcontext.ProviderAccounts
				.SingleOrDefaultAsync(
					x =>
						x.WebhookKey == webhookKey &&
						x.Provider == "payos" &&
						x.OwnerScope == ProviderOwnerScope.Platform &&
						x.TenantId == null &&
						x.SupportsCollection &&
						x.IsActive,
					cancellationToken);

			if(providerAccount is null)
			{
				return new(
					PayOSWebhookProcessingStatus.ProviderAccountNotFound,
					"Không tìm thấy kênh PayOS Platform.");
			}

			PayOSVerifiedWebhookData webhookData;

			try
			{
				webhookData = await payOSGateway.VerifyWebhookAsync(
					rawPayload,
					cancellationToken);
			}
			catch(PayOSWebhookVerificationException exception)
			{
				logger.LogWarning(
					exception,
					"Webhook PayOS có chữ ký không hợp lệ.");

				await SaveInvalidWebhookAsync(
					providerAccount.Id,
					rawPayload,
					exception.Message,
					cancellationToken);

				return new(
					PayOSWebhookProcessingStatus.InvalidSignature,
					exception.Message);
			}

			if(webhookData.Amount <= 0 ||
				string.IsNullOrWhiteSpace(webhookData.Reference) ||
				string.IsNullOrWhiteSpace(webhookData.PaymentLinkId) ||
				!string.Equals(
					webhookData.Currency?.Trim(),
					"VND",
					StringComparison.OrdinalIgnoreCase))
			{
				return await SaveFailedVerifiedWebhookAsync(
					providerAccount.Id,
					rawPayload,
					webhookData,
					"PayOS giao dịch PayOS không hợp lệ.",
					cancellationToken);
			}

			var eventKey = PayOSWebhookEventKeyFactory.Create(webhookData);//

			var now = timeProvider.GetUtcNow();

			await using var transaction = await dbcontext.Database
				.BeginTransactionAsync(cancellationToken);

			await AcquireOrderLockAsync(
				providerAccount.Id,
				webhookData.OrderCode,
				cancellationToken);

			var webhookEvent = await dbcontext.PaymentWebhookEvents
				.SingleOrDefaultAsync(
					x =>
						x.ProviderAccountId == providerAccount.Id &&
						x.EventKey == eventKey,
					cancellationToken);

			if(webhookEvent?.ProcessingStatus == ProcessingStatusEnum.Succeeded)
			{
				await transaction.CommitAsync(cancellationToken);

				return new(
					PayOSWebhookProcessingStatus.Duplicate,
					"Webhook đã được xử lý trước đó.");
			}

			if(webhookEvent is null)
			{
				webhookEvent = new PaymentWebhookEvent
				{
					Id = Guid.NewGuid(),
					ProviderAccountId = providerAccount.Id,
					EventKey = eventKey,
					OrderCode = webhookData.OrderCode,
					ProviderPaymentLinkId = webhookData.PaymentLinkId,
					ProviderReference = webhookData.Reference,
					SignatureValid = true,
					RawPayload = rawPayload,
					ProcessingStatus = ProcessingStatusEnum.Processing,
					ProcessingAttempts = 1,
					LastAttemptAt = now,
					ReceivedAt = now
				};

				dbcontext.PaymentWebhookEvents.Add(webhookEvent);
			}
			else
			{
				webhookEvent.ProcessingStatus = ProcessingStatusEnum.Processing;
				webhookEvent.ProcessingAttempts++;
				webhookEvent.LastAttemptAt = now;
				webhookEvent.ProcessingError = null;
			}

			await dbcontext.SaveChangesAsync(cancellationToken);

			var paymentRequest = await dbcontext.PaymentRequests
				.SingleOrDefaultAsync(
					x =>
						x.ProviderAccountId == providerAccount.Id &&
						x.OrderCode == webhookData.OrderCode,
					cancellationToken);

			if(paymentRequest is null)
			{
				webhookEvent.ProcessingStatus = 
					ProcessingStatusEnum.Failed;

				webhookEvent.ProcessingError = 
					"Không tìm thấy payment request theo OrderCode.";

				await dbcontext.SaveChangesAsync(cancellationToken);

				await transaction.CommitAsync(cancellationToken);

				return new(
					PayOSWebhookProcessingStatus.Unresolved,
					"Không tìm thấy payment request.");
			}

			webhookEvent.PaymentRequestId = paymentRequest.Id;

			if(!string.Equals(
				paymentRequest.ProviderPaymentLinkId,
				webhookData.PaymentLinkId,
				StringComparison.Ordinal))
			{
				webhookEvent.ProcessingStatus = 
					ProcessingStatusEnum.Failed;

				webhookEvent.ProcessingError =
					"PaymentLinkId từ PayOS không khớp Database.";

				await dbcontext.SaveChangesAsync(cancellationToken);

				await transaction.CommitAsync(cancellationToken);

				return new(
					PayOSWebhookProcessingStatus.Unresolved,
					"PaymentLinkId không khớp.");
			}

			var providerTransactionId = webhookData.Reference.Trim();

			var existingTransaction = await dbcontext.PaymentTransactions
				.SingleOrDefaultAsync(
					x =>
						x.ProviderAccountId == providerAccount.Id &&
						x.ProviderTransactionId == providerTransactionId,
					cancellationToken);

			if(existingTransaction?.Status == PaymentTransactionStatus.Verified)
			{
				webhookEvent.ProcessingStatus = ProcessingStatusEnum.Succeeded;

				webhookEvent.ProcessedAt = now;

				webhookEvent.ProcessingError = null;

				await dbcontext.SaveChangesAsync(cancellationToken);

				await transaction.CommitAsync(cancellationToken);

				return new(
					PayOSWebhookProcessingStatus.Duplicate,
					"Giao dịch đã được ghi nhận trước đó.");
			}

			var paymentAttempt = await dbcontext.SubscriptionPaymentAttempts
				.Include(x => x.SubscriptionInvoice)
				.SingleOrDefaultAsync(
					x =>
						x.GatewayAccountId == providerAccount.Id &&
						x.GatewayPaymentRequestId == paymentRequest.Id,
					cancellationToken);

			if(paymentAttempt is null)
			{
				webhookEvent.ProcessingStatus = ProcessingStatusEnum.Failed;

				webhookEvent.ProcessingError = "Không tìm thấy subscription payment attempt.";

				await dbcontext.SaveChangesAsync(cancellationToken);

				await transaction.CommitAsync(cancellationToken);

				return new(
					PayOSWebhookProcessingStatus.Unresolved,
					"Không tìm thấy payment attempt.");
			}

			var invoice = paymentAttempt.SubscriptionInvoice;

			var webhookAmount = decimal.Parse(
				webhookData.Amount.ToString(),
				System.Globalization.CultureInfo.InvariantCulture);

			var invoiceRemaining = Math.Max(
				invoice.TotalAmount - invoice.PaidAmount,
				0);

			var paymentRequestRemaining = Math.Max(
				paymentRequest.Amount - paymentRequest.AmountReceived,
				0);

			var amountApplied = Math.Min(
				webhookAmount,
				Math.Min(
					invoiceRemaining,
					paymentRequestRemaining));

			var occurredAt = ParseOccurredAt(
				webhookData.TransactionDateTime,
				now);

			var paymentTransaction = existingTransaction ??
				new PaymentTransaction
				{
					Id = Guid.NewGuid(),
					ProviderAccountId = providerAccount.Id,
					PaymentRequestId = paymentRequest.Id,
					SourceWebhookEventId = webhookEvent.Id,
					ProviderReference = webhookData.Reference,
					ProviderTransactionId = providerTransactionId,
					Amount = webhookAmount,
					OccurredAt = occurredAt,
					PayerMasked = "{}",
					ProviderPayload = rawPayload,
					CreatedAt = now,
					Currency = "VND",
					Status = PaymentTransactionStatus.Verified
				};

			if(existingTransaction is null)
			{
				dbcontext.PaymentTransactions.Add(
					paymentTransaction);
			}
			else
			{
				existingTransaction.PaymentRequestId = paymentRequest.Id;
				existingTransaction.SourceWebhookEventId = webhookEvent.Id;
				existingTransaction.Status = PaymentTransactionStatus.Verified;
			}

			if(amountApplied > 0)
			{
				var allocation = new SubscriptionPaymentAllocation
				{
					Id = Guid.NewGuid(),
					PaymentAttemptId = paymentAttempt.Id,
					GatewayAccountId = providerAccount.Id,
					GatewayTransactionId = paymentTransaction.Id,
					AmountApplied = amountApplied,
					CreatedAt = now
				};

				dbcontext.SubscriptionPaymentAllocations.Add(allocation);

				paymentRequest.AmountReceived =
					Math.Min(
						paymentRequest.Amount,
						paymentRequest.AmountReceived + amountApplied);

				invoice.PaidAmount =
					Math.Min(
						invoice.TotalAmount,
						invoice.PaidAmount + amountApplied);
			}

			paymentRequest.Status = paymentRequest.AmountReceived >= paymentRequest.Amount
				? PaymentRequestStatus.Paid
				: PaymentRequestStatus.Underpaid;

			if(invoice.PaidAmount >= invoice.TotalAmount)
			{
				invoice.Status = SubscriptionInvoiceStatus.Paid;

				paymentAttempt.Status = SubscriptionPaymentAttemptStatus.Paid;
			}
			else if(amountApplied > 0)
			{
				paymentAttempt.Status = 
					SubscriptionPaymentAttemptStatus.Partially_paid;
			}

			var renewalRequest = await dbcontext.SubscriptionRenewalRequests
				.SingleOrDefaultAsync(
					x =>
						x.SubscriptionInvoiceId == invoice.Id,
					cancellationToken);

			if(invoice.Status == SubscriptionInvoiceStatus.Paid &&
				renewalRequest?.Status == SubscriptionRenewalRequestStatus.InvoiceIssued)
			{
				renewalRequest.Status = 
					SubscriptionRenewalRequestStatus.PaidAwaitingActivation;
			}

			webhookEvent.ProcessingStatus = ProcessingStatusEnum.Succeeded;

			webhookEvent.ProcessedAt = now;

			webhookEvent.ProcessingError = null;

			await dbcontext.SaveChangesAsync(cancellationToken);

			await transaction.CommitAsync(cancellationToken);

			return new(
				PayOSWebhookProcessingStatus.Succeeded,
				amountApplied < webhookAmount
					? "Đã ghi nhận giao dịch; có phần tiền vượt quá số tiền còn thiếu."
					: "Đã ghi nhận thanh toán.");
		}

		private static DateTimeOffset ParseOccurredAt(
			string value, 
			DateTimeOffset fallback)
		{
			if(!DateTime.TryParse(
				value,
				System.Globalization.CultureInfo.InvariantCulture,
				System.Globalization.DateTimeStyles.None,
				out var localTime))
			{
				return fallback;
			}

			var vietnamTimeZone = TimeZoneInfo.FindSystemTimeZoneById(
				"SE Asia Standard Time");

			var utcTime = TimeZoneInfo.ConvertTimeToUtc(
				DateTime.SpecifyKind(
					localTime,
					DateTimeKind.Unspecified),
				vietnamTimeZone);

			return new DateTimeOffset(
				utcTime,
				TimeSpan.Zero);
		}

		private async Task SaveInvalidWebhookAsync(
			Guid providerAccountId, 
			string rawPayload, 
			string error, 
			CancellationToken cancellationToken)
		{
			var now = timeProvider.GetUtcNow();

			dbcontext.PaymentWebhookEvents.Add(
				new PaymentWebhookEvent
				{
					Id = Guid.NewGuid(),
					ProviderAccountId = providerAccountId,
					SignatureValid = false,
					RawPayload = rawPayload,
					ProcessingStatus = ProcessingStatusEnum.Failed,
					ProcessingAttempts = 1,
					LastAttemptAt = now,
					ReceivedAt = now,
					ProcessingError = error
				});

			await dbcontext.SaveChangesAsync(cancellationToken);
		}

		private async Task AcquireOrderLockAsync(
			Guid providerAccountId, 
			long orderCode, 
			CancellationToken cancellationToken)
		{
			if(dbcontext.Database.ProviderName !=
				"Npgsql.EntityFrameworkCore.PostgreSQL")
			{
				return;
			}

			var lockKey = $"payos:{providerAccountId:N}:{orderCode}";

			await dbcontext.Database
				.ExecuteSqlInterpolatedAsync(
				$"""
				SELECT pg_advisory_xact_lock(
					hashtextextended({lockKey}, 0))
				""",
				cancellationToken);
		}

		private async Task<PayOSWebhookProcessingResult> SaveFailedVerifiedWebhookAsync(
			Guid providerAccountId,
			string rawPayload,
			PayOSVerifiedWebhookData data,
			string error,
			CancellationToken cancellationToken)
		{
			var now = timeProvider.GetUtcNow();

			var eventKey =
				PayOSWebhookEventKeyFactory.Create(data);

			await using var transaction =
				await dbcontext.Database.BeginTransactionAsync(
					cancellationToken);

			await AcquireOrderLockAsync(
				providerAccountId,
				data.OrderCode,
				cancellationToken);

			var existingEvent =
				await dbcontext.PaymentWebhookEvents
					.SingleOrDefaultAsync(
						x =>
							x.ProviderAccountId == providerAccountId &&
							x.EventKey == eventKey,
						cancellationToken);

			if (existingEvent?.ProcessingStatus ==
				ProcessingStatusEnum.Succeeded)
			{
				await transaction.CommitAsync(cancellationToken);

				return new(
					PayOSWebhookProcessingStatus.Duplicate,
					"Webhook đã được xử lý trước đó.");
			}

			if (existingEvent is null)
			{
				dbcontext.PaymentWebhookEvents.Add(
					new PaymentWebhookEvent
					{
						Id = Guid.NewGuid(),
						ProviderAccountId = providerAccountId,
						EventKey = eventKey,
						OrderCode = data.OrderCode,
						ProviderPaymentLinkId = data.PaymentLinkId,
						ProviderReference = data.Reference,
						SignatureValid = true,
						RawPayload = rawPayload,
						ProcessingStatus = ProcessingStatusEnum.Failed,
						ProcessingAttempts = 1,
						LastAttemptAt = now,
						ReceivedAt = now,
						ProcessingError = error
					});
			}
			else
			{
				existingEvent.ProcessingStatus =
					ProcessingStatusEnum.Failed;

				existingEvent.ProcessingAttempts++;

				existingEvent.LastAttemptAt = now;
				existingEvent.ProcessingError = error;
				existingEvent.RawPayload = rawPayload;
				existingEvent.SignatureValid = true;
			}

			await dbcontext.SaveChangesAsync(cancellationToken);

			await transaction.CommitAsync(cancellationToken);

			return new(
				PayOSWebhookProcessingStatus.Unresolved,
				error);
		}
	}
}
