using EzCondo.BackEndServer.Contracts.SubscriptionInvoices;
using EzCondo.BackEndServer.Data;
using EzCondo.BackEndServer.Data.Entities.SchemaPayment;
using EzCondo.BackEndServer.Data.Entities.SchemaPlatform;
using EzCondo.BackEndServer.Data.Enums;
using EzCondo.BackEndServer.Infrastructure.Authentication;
using EzCondo.BackEndServer.Infrastructure.Payments.PayOS;
using EzCondo.BackEndServer.Infrastructure.Payments.PayOS.Models;
using EzCondo.BackEndServer.Infrastructure.Payments.PayOS.Options;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Npgsql;
using System.Security.Claims;

namespace EzCondo.BackEndServer.Services.SubscriptionInvoices
{
	public sealed class TenantSubscriptionPaymentLinkService(
		ApplicationDbContext dbcontext,
		IHttpContextAccessor httpContextAccessor,
		IPayOSGateway payOSGateway,
		IOptions<PayOSIntegrationOptions> options,
		TimeProvider timeProvider,
		ILogger<TenantSubscriptionPaymentLinkService> logger)
		: ITenantSubscriptionPaymentLinkService
	{
		private const string OpenAttemptUniqueIndex = 
			"ux_subscription_payment_attempts_open_invoice";

		private readonly PayOSIntegrationOptions _options = options.Value;

		public async Task<CreateSubscriptionPaymentLinkResult> CreateAsync(
			Guid invoiceId, 
			CancellationToken cancellationToken = default)
		{
			if(!TryGetCurrentIdentity(
				out var userId,
				out var tenantId))
			{
				return Failed(
					CreateSubscriptionPaymentLinkFailure.InvalidIdentity);
			}

			if(!await IsTenantAdminAsync(
				userId,
				tenantId,
				cancellationToken))
			{
				return Failed(
					CreateSubscriptionPaymentLinkFailure.TenantAdminRequired);
			}

			var now = timeProvider.GetUtcNow();

			PaymentRequest paymentRequest;
			SubscriptionPaymentAttempt paymentAttempt;
			PayableInvoice invoice;

			await using (
				var transaction = await dbcontext.Database
				.BeginTransactionAsync(cancellationToken))
			{
				await AcquireInvoiceLockAsync(
					invoiceId,
					cancellationToken);

				var loadedInvoice = await dbcontext.SubscriptionInvoices
					.AsNoTracking()
					.Where(
						x =>
							x.Id == invoiceId &&
							x.TenantId == tenantId)
					.Select(x => new PayableInvoice(
						x.Id,
						x.InvoiceNo,
						x.TenantId,
						x.SubscriptionId,
						x.Status,
						x.TotalAmount,
						x.PaidAmount,
						x.Currency,
						x.IsDeleted))
					.SingleOrDefaultAsync(cancellationToken);

				if (loadedInvoice is null)
				{
					await transaction.CommitAsync(cancellationToken);

					return Failed(
						CreateSubscriptionPaymentLinkFailure.InvoiceNotFound);
				}

				invoice = loadedInvoice;

				var remainingAmount = invoice.TotalAmount - invoice.PaidAmount;

				if(invoice.IsDeleted ||
					invoice.Status != SubscriptionInvoiceStatus.Issued ||
					remainingAmount <= 0 ||
					!string.Equals(
						invoice.Currency.Trim(),
						"VND",
						StringComparison.OrdinalIgnoreCase) ||
					!TryConvertPayOSAmount(
						remainingAmount,
						out var payOSAmount))
				{
					await transaction.CommitAsync(cancellationToken);

					return Failed(
						CreateSubscriptionPaymentLinkFailure.InvoiceNotPayable);
				}

				var renewalReady = await dbcontext.SubscriptionRenewalRequests
					.AsNoTracking()
					.AnyAsync(
						x =>
							x.TenantId == tenantId &&
							x.SubscriptionId == invoice.SubscriptionId &&
							x.SubscriptionInvoiceId == invoice.Id &&
							x.Status == SubscriptionRenewalRequestStatus.InvoiceIssued,
						cancellationToken);

				if (!renewalReady)
				{
					await transaction.CommitAsync(cancellationToken);

					return Failed(
						CreateSubscriptionPaymentLinkFailure.RenewalRequestNotReady);
				}

				var accountCode = NormalizeAccountCode(
					_options.ProviderAccountCode);

				var providerAccount =
					await dbcontext.ProviderAccounts
					.AsNoTracking()
					.SingleOrDefaultAsync(
						x =>
							x.TenantId == null &&
							x.OwnerScope == ProviderOwnerScope.Platform &&
							x.Code == accountCode &&
							x.Provider == PayOSProviderConstants.ProviderCode &&
							x.PaymentCredentialsRef ==
								PayOSProviderConstants.PaymentCredentialsReference &&
							x.SupportsCollection &&
							x.IsActive,
						cancellationToken);

				if(providerAccount is null)
				{
					await transaction.CommitAsync(cancellationToken);

					return Failed(
						CreateSubscriptionPaymentLinkFailure.ProviderAccountUnavailable);
				}

				var openAttempt = await dbcontext.SubscriptionPaymentAttempts
					.Include(x => x.PaymentRequests)
					.SingleOrDefaultAsync(
						x =>
							x.SubscriptionInvoiceId == invoice.Id &&
							(
								x.Status == SubscriptionPaymentAttemptStatus.Pending ||
								x.Status == SubscriptionPaymentAttemptStatus.Partially_paid
							),
						cancellationToken);

				if(openAttempt is not null)
				{
					var existingRequest = openAttempt.PaymentRequests;

					if(CanReuse(
						existingRequest,
						remainingAmount,
						now))
					{
						await transaction.CommitAsync(cancellationToken);

						return CreateSubscriptionPaymentLinkResult.Success(
							ToResponse(
								invoice,
								existingRequest,
								openAttempt,
								reused: true));
					}

					if(IsOpen(existingRequest.Status) &&
						!HasCompleteLink(existingRequest) &&
						existingRequest.CreatedAt > (now - GetCreationGracePeriod()))
					{
						await transaction.CommitAsync(cancellationToken);

						return Failed(
							CreateSubscriptionPaymentLinkFailure.PaymentLinkCreationInProgress);
					}

					var recoveryResult = await RecoverStaleAttemptAsync(
						openAttempt,
						now,
						cancellationToken);

					if(recoveryResult != RecoveryResult.Closed)
					{
						await transaction.CommitAsync(cancellationToken);

						return recoveryResult switch
						{
							RecoveryResult.ReconciliationPending =>
								Failed(
									CreateSubscriptionPaymentLinkFailure
										.PaymentReconciliationPending),

							_ =>
								Failed(
									CreateSubscriptionPaymentLinkFailure
										.PaymentProviderUnavailable)
						};
					}
				}

				var orderCode = await dbcontext.Database
					.SqlQueryRaw<long>(
						"""
						SELECT nextval(
							'payment.payment_order_code_seq'
						) AS "Value"
						""")
					.SingleAsync(cancellationToken);

				var expiresAt = now.AddMinutes(
					_options.PaymentLinkMinutes);

				paymentRequest = new PaymentRequest
				{
					Id = Guid.NewGuid(),
					ProviderAccountId = providerAccount.Id,
					OrderCode = orderCode,
					ProviderPaymentLinkId = null,
					Status = PaymentRequestStatus.Pending,
					Amount = remainingAmount,
					AmountReceived = 0,
					Currency = "VND",
					CheckoutUrl = null,
					QrCode = null,
					ReturnUrl = _options.ReturnUrl,
					CancelUrl = _options.CancelUrl,
					ExpiresAt = expiresAt,
					CreatedAt = now
				};

				paymentAttempt = new SubscriptionPaymentAttempt
				{
					Id = Guid.NewGuid(),
					SubscriptionInvoiceId = invoice.Id,
					GatewayAccountId = providerAccount.Id,
					GatewayPaymentRequestId = paymentRequest.Id,
					RequestedByUserId = userId,
					Status = SubscriptionPaymentAttemptStatus.Pending,
					CreatedAt = now
				};

				dbcontext.PaymentRequests.Add(paymentRequest);
				dbcontext.SubscriptionPaymentAttempts.Add(paymentAttempt);

				try
				{
					await dbcontext.SaveChangesAsync(cancellationToken);

					await transaction.CommitAsync(cancellationToken);
				}
				catch (DbUpdateException exception)
					when (IsOpenAttemptUniqueViolation(exception))
				{
					await transaction.RollbackAsync(cancellationToken);

					dbcontext.ChangeTracker.Clear();

					return Failed(
						CreateSubscriptionPaymentLinkFailure
							.PaymentLinkCreationInProgress);
				}

				_ = payOSAmount;
			}

			dbcontext.ChangeTracker.Clear();

			PayOSPaymentLinkResult providerResult;

			try
			{
				providerResult =
					await payOSGateway.CreatePaymentLinkAsync(
						new CreatePayOSPaymentLinkCommand(
							paymentRequest.OrderCode,
							decimal.ToInt64(paymentRequest.Amount),
							CreateDescription(paymentRequest.OrderCode),
							paymentRequest.ExpiresAt!.Value),
						cancellationToken);
			}
			catch(PayOSGatewayException exception)
			{
				logger.LogError(
					exception,
					"Không thể tạo PayOS link cho SubsciptionInvoiceId: " +
						"{SubscriptionInvoiceId}; OrderCode: {OrderCode}",
					invoice.Id,
					paymentRequest.OrderCode);

				await MarkCreationFailedAsync(
					paymentRequest.Id,
					paymentAttempt.Id,
					cancellationToken);

				return Failed(
					CreateSubscriptionPaymentLinkFailure
						.PaymentProviderUnavailable);
			}

			var completedAt = timeProvider.GetUtcNow();

			var affectedRows = await dbcontext.PaymentRequests
				.Where(
					x =>
						x.Id == paymentRequest.Id &&
						x.ProviderAccountId == paymentRequest.ProviderAccountId &&
						x.Status == PaymentRequestStatus.Pending &&
						x.ProviderPaymentLinkId == null)
				.ExecuteUpdateAsync(
					setters => setters
						.SetProperty(
							x => x.ProviderPaymentLinkId,
							providerResult.PaymentLinkId)
						.SetProperty(
							x => x.CheckoutUrl,
							providerResult.CheckoutUrl)
						.SetProperty(
							x => x.QrCode,
							providerResult.QrCode)
						.SetProperty(
							x => x.ExpiresAt,
							providerResult.ExpiresAt)
						.SetProperty(
							x => x.UpdatedAt,
							completedAt),
					cancellationToken);

			if(affectedRows != 1)
			{
				logger.LogError(
					"PayOS đã tạo link nhưng không thể hoàn tất PaymentRequest. " +
						"PaymentRequestid: {PaymentRequestId}; " +
						"OrderCode: {OrderCode}",
					paymentRequest.Id,
					paymentRequest.OrderCode);

				return Failed(
					CreateSubscriptionPaymentLinkFailure
						.PaymentLinkCreationInProgress);
			}

			paymentRequest.ProviderPaymentLinkId = providerResult.PaymentLinkId;

			paymentRequest.CheckoutUrl = providerResult.CheckoutUrl;

			paymentRequest.QrCode = providerResult.QrCode;

			paymentRequest.ExpiresAt = providerResult.ExpiresAt;

			return CreateSubscriptionPaymentLinkResult.Success(
				ToResponse(
					invoice,
					paymentRequest,
					paymentAttempt,
					reused: false));
		}

		private async Task<RecoveryResult> RecoverStaleAttemptAsync(
			SubscriptionPaymentAttempt attempt,
			DateTimeOffset now,
			CancellationToken cancellationToken)
		{
			var request = attempt.PaymentRequests;

			PayOSPaymentLinkStatusResult? providerLink;

			try
			{
				providerLink = await payOSGateway.GetPaymentLinkStatusAsync(
					request.OrderCode,
					cancellationToken);
			}
			catch(PayOSGatewayException exception)
			{
				logger.LogWarning(
					exception,
					"Không thể khôi phục trạng thái PayOS. " +
						"OrderCode: {OrderCode}",
					request.OrderCode);

				return RecoveryResult.ProviderUnavailable;
			}

			if(providerLink is null)
			{
				request.Status = PaymentRequestStatus.Failed;

				attempt.Status = SubscriptionPaymentAttemptStatus.Failed;

				await dbcontext.SaveChangesAsync(cancellationToken);

				return RecoveryResult.Closed;
			}

			if(!string.IsNullOrWhiteSpace(request.ProviderPaymentLinkId) &&
				!string.Equals(
					request.ProviderPaymentLinkId,
					providerLink.PaymentLinkId,
					StringComparison.Ordinal))
			{
				logger.LogError(
					"PaymentLinkId trong database không khớp PayOS. " +
						"OrderCode: {OrderCode}",
					request.OrderCode);

				return RecoveryResult.ReconciliationPending;
			}

			request.ProviderPaymentLinkId ??= providerLink.PaymentLinkId;

			var providerStatus = providerLink.ProviderStatus.Trim().ToLowerInvariant();

			if(providerLink.AmountPaid > 0 ||
				providerStatus is
					"paid" or
					"underpaid" or
					"processing")
			{
				await dbcontext.SaveChangesAsync(cancellationToken);

				return RecoveryResult.ReconciliationPending;
			}

			if(providerStatus == "pending")
			{
				try
				{
					var cancelled = await payOSGateway.CancelPaymentLinkAsync(
						request.OrderCode,
						"EzCondo recovery",
						cancellationToken);

					request.ProviderPaymentLinkId ??= cancelled.PaymentLinkId;

					request.Status = PaymentRequestStatus.Cancelled;

					request.CancelledAt = now;
					request.CancellationReason = "stale_local_reservation";

					attempt.Status = SubscriptionPaymentAttemptStatus.Cancelled;

					await dbcontext.SaveChangesAsync(cancellationToken);

					return RecoveryResult.Closed;
				}
				catch(PayOSGatewayException exception)
				{
					logger.LogWarning(
						exception,
						"Không thể hủy PayOS link cũ. " +
							"OrderCode: {OrderCode}",
						request.OrderCode);

					return RecoveryResult.ProviderUnavailable;
				}
			}

			request.Status = providerStatus switch
			{
				"cancelled" => PaymentRequestStatus.Cancelled,
				"expired" => PaymentRequestStatus.Expired,
				_ => PaymentRequestStatus.Failed
			};

			attempt.Status =
				request.Status == PaymentRequestStatus.Failed
					? SubscriptionPaymentAttemptStatus.Failed
					: SubscriptionPaymentAttemptStatus.Cancelled;

			if(request.Status == PaymentRequestStatus.Cancelled)
			{
				request.CancelledAt ??= now;
				request.CancellationReason ??= "provider_link_cancelled";
			}

			await dbcontext.SaveChangesAsync(cancellationToken);

			return RecoveryResult.Closed;
		}

		private static string CreateDescription(long orderCode)
		{
			var suffix = orderCode % 10_000_000;

			return $"EZ{suffix:D7}";
		}

		private async Task MarkCreationFailedAsync(
			Guid paymentRequestId, 
			Guid paymentAttemptId, 
			CancellationToken cancellationToken)
		{
			var now = timeProvider.GetUtcNow();

			await dbcontext.PaymentRequests
				.Where(
					x =>
						x.Id == paymentRequestId &&
						x.Status == PaymentRequestStatus.Pending &&
						x.ProviderPaymentLinkId == null)
				.ExecuteUpdateAsync(
					setters => setters
						.SetProperty(
							x => x.Status,
							PaymentRequestStatus.Failed)
						.SetProperty(
							x => x.UpdatedAt,
							now),
					cancellationToken);

			await dbcontext.SubscriptionPaymentAttempts
				.Where(
					x =>
						x.Id == paymentAttemptId &&
						x.Status == SubscriptionPaymentAttemptStatus.Pending)
				.ExecuteUpdateAsync(
					setters => setters
						.SetProperty(
							x => x.Status,
							SubscriptionPaymentAttemptStatus.Failed),
					cancellationToken);
		}

		private async Task AcquireInvoiceLockAsync(
			Guid invoiceId,
			CancellationToken cancellationToken)
		{
			if(dbcontext.Database.ProviderName !=
					"Npgsql.EntityFrameworkCore.PostgreSQL")
			{
				return;
			}

			var lockKey = invoiceId.ToString();

			await dbcontext.Database.ExecuteSqlInterpolatedAsync(
				$"""
				SELECT pg_advisory_xact_lock(
					hashtextextended({lockKey}, 0))
				""",
				cancellationToken);
				
		}

		private bool TryGetCurrentIdentity(
			out Guid userId,
			out Guid tenantId)
		{
			userId = Guid.Empty;
			tenantId = Guid.Empty;

			var principal = httpContextAccessor.HttpContext?.User;

			var userIdValue = principal?.FindFirstValue(AppClaimTypes.Subject);

			var tenantIdValue = principal?.FindFirstValue(AppClaimTypes.TenantId);

			return Guid.TryParse(userIdValue, out userId) &&
				Guid.TryParse(tenantIdValue, out tenantId);
		}

		private Task<bool> IsTenantAdminAsync(
			Guid userId,
			Guid tenantId,
			CancellationToken cancellationToken)
		{
			return dbcontext.UserRoles
				.AsNoTracking()
				.AnyAsync(
					x =>
						x.UserId == userId &&
						x.Scope == RoleScopeEnum.Tenant &&
						x.TenantId == tenantId &&
						x.RevokedAt == null &&

						x.Role.Scope == RoleScopeEnum.Tenant &&
						x.Role.TenantId == tenantId &&
						x.Role.Code == DbInitializer.TenantAdminRoleCode &&
						!x.Role.IsDeleted &&

						x.User.TenantId == tenantId &&
						!x.User.IsDeleted &&
						x.User.Status == UserStatusEnum.Active &&
						x.User.EmailConfirmed,
					cancellationToken);
		}

		private bool IsOpenAttemptUniqueViolation(DbUpdateException exception)
		{
			return exception.InnerException is PostgresException
			{
				SqlState: PostgresErrorCodes.UniqueViolation,
				ConstraintName: OpenAttemptUniqueIndex
			};
		}

		private TimeSpan GetCreationGracePeriod()
		{
			var seconds = checked(
				_options.TimeoutSeconds * (_options.MaxRetries + 1) + 30);

			return TimeSpan.FromSeconds(seconds);
		}

		private static bool HasCompleteLink(PaymentRequest request)
		{
			return !string.IsNullOrWhiteSpace(request.ProviderPaymentLinkId) &&
				!string.IsNullOrWhiteSpace(request.CheckoutUrl) &&
				!string.IsNullOrWhiteSpace(request.QrCode) &&
				request.ExpiresAt is not null;
		}

		private static SubscriptionPaymentLinkResponse ToResponse(
			PayableInvoice invoice, 
			PaymentRequest request,
			SubscriptionPaymentAttempt attempt, 
			bool reused)
		{
			return new SubscriptionPaymentLinkResponse(
				invoice.Id,
				invoice.InvoiceNo,
				request.Id,
				attempt.Id,
				request.OrderCode,
				request.ProviderPaymentLinkId!,
				request.Amount - request.AmountReceived,
				request.Currency.Trim(),
				request.Status.ToString().ToLowerInvariant(),
				request.CheckoutUrl!,
				request.QrCode!,
				request.ExpiresAt!.Value,
				reused);
		}

		private static bool TryConvertPayOSAmount(
			decimal value, 
			out long amount)
		{
			amount = 0;

			if(value <= 0 ||
				value != decimal.Truncate(value) ||
				value > long.MaxValue)
			{
				return false;
			}

			amount = decimal.ToInt64(value);

			return true;
		}

		private static CreateSubscriptionPaymentLinkResult Failed(
			CreateSubscriptionPaymentLinkFailure failure)
		{
			return CreateSubscriptionPaymentLinkResult.Failed(failure);
		}

		private static bool CanReuse(
			PaymentRequest request,
			decimal remainingAmount,
			DateTimeOffset now)
		{
			var requestRemaining =
				request.Amount - request.AmountReceived;

			return IsOpen(request.Status) &&
				HasCompleteLink(request) &&
				request.ExpiresAt > now &&
				requestRemaining == remainingAmount;
		}

		private static bool IsOpen(PaymentRequestStatus status)
		{
			return status is
				PaymentRequestStatus.Pending or
				PaymentRequestStatus.Underpaid;
		}

		private static string NormalizeAccountCode(string value)
		{
			return value.Trim().ToUpperInvariant();
		}

		private sealed record PayableInvoice(
			Guid Id,
			string InvoiceNo,
			Guid TenantId,
			Guid SubscriptionId,
			SubscriptionInvoiceStatus Status,
			decimal TotalAmount,
			decimal PaidAmount,
			string Currency,
			bool IsDeleted);

		private enum RecoveryResult
		{
			Closed,
			ReconciliationPending,
			ProviderUnavailable
		}
	}
}
