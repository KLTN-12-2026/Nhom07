using EzCondo.BackEndServer.Contracts.SubscriptionInvoices;
using EzCondo.BackEndServer.Data;
using EzCondo.BackEndServer.Data.Entities.SchemaPlatform;
using EzCondo.BackEndServer.Data.Enums;
using EzCondo.BackEndServer.Infrastructure.Authentication;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using System.Security.Claims;

namespace EzCondo.BackEndServer.Services.SubscriptionInvoices
{
	public sealed class SubscriptionInvoiceService(
		ApplicationDbContext dbcontext,
		IHttpContextAccessor httpContextAccessor,
		TimeProvider timeProvider)
		: ISubscriptionInvoiceService
	{
		private static readonly TimeSpan VietNamUtcOffset = TimeSpan.FromHours(7);

		private const string ActivePeriodUniqueIndex = "ux_subscription_invoices_active_period";

		public async Task<CreateSubscriptionInvoiceDraftResult> CreateDraftFromRenewalAsync(
			Guid renewalRequestId, 
			CancellationToken cancellationToken = default)
		{
			if(renewalRequestId == Guid.Empty)
			{
				return CreateSubscriptionInvoiceDraftResult.Failed(
					CreateSubscriptionInvoiceDraftFailure
						.RenewalRequestNotFound);
			}

			if(!TryGetCurrentUserId(out var platformAdminId))
			{
				return CreateSubscriptionInvoiceDraftResult.Failed(
					CreateSubscriptionInvoiceDraftFailure
						.InvalidIdentity);
			}

			var isPlatformAdmin = await IsPlatformAdminAsync(
				platformAdminId,
				cancellationToken);

			if (!isPlatformAdmin)
			{
				return CreateSubscriptionInvoiceDraftResult.Failed(
					CreateSubscriptionInvoiceDraftFailure
						.PlatformAdminRequired);
			}

			var renewal = await dbcontext.SubscriptionRenewalRequests
				.AsNoTracking()
				.Where(x => x.Id == renewalRequestId)
				.Select(x => new RenewalInvoiceSource(
					x.Id,
					x.TenantId,
					x.Tenant.Code,
					x.Tenant.Slug,
					x.Tenant.LegalName,
					x.Tenant.Status,

					x.SubscriptionId,
					x.Subscription.Status,
					x.Subscription.EndsAt,

					x.Status,
					x.CurrentEndsAtSnapshot,
					x.SelectedPlanId,
					x.PlanNameSnapshot,
					x.BillingCycleMonthsSnapshot,
					x.PriceSnapshot,
					x.CurrencySnapshot,
					x.BillingPeriodFrom,
					x.BillingPeriodTo,
					x.SubscriptionInvoiceId))
				.SingleOrDefaultAsync(cancellationToken);

			if(renewal is null)
			{
				return CreateSubscriptionInvoiceDraftResult.Failed(
					CreateSubscriptionInvoiceDraftFailure
						.RenewalRequestNotFound);
			}

			if(renewal.RenewalStatus !=
					SubscriptionRenewalRequestStatus.AwaitingPlatformReview ||
				renewal.SubscriptionInvoiceId.HasValue)
			{
				return CreateSubscriptionInvoiceDraftResult.Failed(
					CreateSubscriptionInvoiceDraftFailure
						.RenewalRequestAlreadyReviewed);
			}

			var expectedTenantStatus = renewal.SubscriptionStatus switch
			{
				SubscriptionStatusEnum.Trial => TenantStatusEnum.Trial,
				SubscriptionStatusEnum.Active => TenantStatusEnum.Active,

				_ => (TenantStatusEnum?)null
			};

			if(renewal.SubscriptionEndsAt != renewal.CurrentEndsAtSnapshot ||
				!expectedTenantStatus.HasValue ||
				renewal.TenantStatus != expectedTenantStatus.Value)
			{
				return CreateSubscriptionInvoiceDraftResult.Failed(
					CreateSubscriptionInvoiceDraftFailure
						.RenewalRequestStale);
			}

			if (!renewal.SelectedPlanId.HasValue ||
				string.IsNullOrWhiteSpace(renewal.PlanNameSnapshot) ||
				!renewal.BillingCycleMonthsSnapshot.HasValue ||
				renewal.BillingCycleMonthsSnapshot.Value is not (6 or 12 or 24) ||
				!renewal.PriceSnapshot.HasValue ||
				renewal.PriceSnapshot.Value <= 0 ||
				string.IsNullOrWhiteSpace(renewal.CurrencySnapshot) ||
				renewal.CurrencySnapshot.Trim().Length != 3 ||
				!renewal.BillingPeriodFrom.HasValue ||
				!renewal.BillingPeriodTo.HasValue ||
				renewal.BillingPeriodFrom.Value != renewal.CurrentEndsAtSnapshot ||
				renewal.BillingPeriodTo.Value !=
					renewal.BillingPeriodFrom.Value.AddMonths(renewal.BillingCycleMonthsSnapshot.Value))
			{
				return CreateSubscriptionInvoiceDraftResult.Failed(
					CreateSubscriptionInvoiceDraftFailure
						.RenewalSnapshotInvalid);
			}

			var selectedPlanId = renewal.SelectedPlanId.Value;

			var planName = renewal.PlanNameSnapshot!;

			var totalAmount = renewal.PriceSnapshot.Value;

			var currency = renewal.CurrencySnapshot!.Trim();

			var billingPeriodFrom = renewal.BillingPeriodFrom.Value;

			var billingPeriodTo = renewal.BillingPeriodTo.Value;

			var invoiceAlreadyExists = await dbcontext.SubscriptionInvoices
				.AsNoTracking()
				.AnyAsync(
					x =>
						!x.IsDeleted &&
						x.TenantId == renewal.TenantId &&
						x.SubscriptionId == renewal.SubscriptionId &&
						x.BillingPeriodFrom == billingPeriodFrom &&
						x.BillingPeriodTo == billingPeriodTo,
				cancellationToken);

			if (invoiceAlreadyExists)
			{
				return CreateSubscriptionInvoiceDraftResult.Failed(
					CreateSubscriptionInvoiceDraftFailure
						.InvoiceAlreadyExistsForPeriod);
			}

			var now = timeProvider.GetUtcNow();

			await using var transaction = await dbcontext.Database
				.BeginTransactionAsync(cancellationToken);

			var sequenceValue = await dbcontext.Database
				.SqlQueryRaw<long>(
					"""
					SELECT nextval(
						'platform.subscription_invoice_no_seq'
						) AS "Value"
					""")
				.SingleOrDefaultAsync(cancellationToken);

			var invoiceNo = CreateInvoiceNumber(
				sequenceValue,
				now);


			var invoice = new SubscriptionInvoice
			{
				Id = Guid.NewGuid(),
				TenantId = renewal.TenantId,
				SubscriptionId = renewal.SubscriptionId,
				InvoiceNo = invoiceNo,
				BillingPeriodFrom = billingPeriodFrom,
				BillingPeriodTo = billingPeriodTo,
				DueAt = CalculateDueAt(billingPeriodFrom),
				Status = SubscriptionInvoiceStatus.Draft,
				TotalAmount = totalAmount,
				PaidAmount = 0,
				Currency = currency,
				CreatedByAdminId = platformAdminId,
				CreatedAt = now
			};

			dbcontext.SubscriptionInvoices.Add(invoice);

			try
			{
				await dbcontext.SaveChangesAsync(cancellationToken);

				var affectedRows = await dbcontext.SubscriptionRenewalRequests
					.Where(
						x =>
							x.Id == renewalRequestId &&
							x.Status == SubscriptionRenewalRequestStatus.AwaitingPlatformReview &&
							x.SubscriptionInvoiceId == null &&
							x.PlatformReviewedByAdminId == null &&
							x.PlatformReviewedAt == null)
					.ExecuteUpdateAsync(
						setters => setters
							.SetProperty(
								x => x.PlatformReviewedByAdminId,
								(Guid?)platformAdminId)
							.SetProperty(
								x => x.PlatformReviewedAt,
								(DateTimeOffset?)now)
							.SetProperty(
								x => x.SubscriptionInvoiceId,
								(Guid?)invoice.Id)
							.SetProperty(
								x => x.Status,
								SubscriptionRenewalRequestStatus.InvoiceDraftCreated)
							.SetProperty(
								x => x.UpdatedAt,
								(DateTimeOffset?)now),
						cancellationToken);

				if(affectedRows == 0)
				{
					await transaction.RollbackAsync(cancellationToken);

					dbcontext.Entry(invoice).State = EntityState.Detached;

					return CreateSubscriptionInvoiceDraftResult.Failed(
						CreateSubscriptionInvoiceDraftFailure
							.RenewalRequestAlreadyReviewed);
				}

				await transaction.CommitAsync(cancellationToken);
			}
			catch(DbUpdateException exception)
				when (IsActivePeriodUniqueViolation(exception))
			{
				await transaction.RollbackAsync(cancellationToken);

				dbcontext.Entry(invoice).State = EntityState.Detached;

				return CreateSubscriptionInvoiceDraftResult.Failed(
					CreateSubscriptionInvoiceDraftFailure
						.InvoiceAlreadyExistsForPeriod);
			}

			var response = new SubscriptionInvoiceResponse(
				invoice.Id,
				renewal.TenantId,
				renewal.TenantCode,
				renewal.TenantSlug,
				renewal.TenantLegalName,
				renewal.SubscriptionId,
				selectedPlanId,
				planName,
				invoice.InvoiceNo,
				billingPeriodFrom,
				billingPeriodTo,
				invoice.DueAt,
				invoice.Status,
				invoice.TotalAmount,
				invoice.PaidAmount,
				invoice.Currency,
				invoice.CreatedByAdminId,
				invoice.CreatedAt);

			return CreateSubscriptionInvoiceDraftResult.Success(response);
		}

		public async Task<IssueSubscriptionInvoiceResult> IssueAsync(
			Guid invoiceId, 
			CancellationToken cancellationToken = default)
		{
			if(invoiceId == Guid.Empty)
			{
				return IssueSubscriptionInvoiceResult.Failed(
					IssueSubscriptionInvoiceFailure.InvoiceNotFound);
			}

			if(!TryGetCurrentUserId(out var platformAdminId))
			{
				return IssueSubscriptionInvoiceResult.Failed(
					IssueSubscriptionInvoiceFailure.InvalidIdentity);
			}

			var isPlatformAdmin = await IsPlatformAdminAsync(
				platformAdminId,
				cancellationToken);

			if (!isPlatformAdmin)
			{
				return IssueSubscriptionInvoiceResult.Failed(
					IssueSubscriptionInvoiceFailure.PlatformAdminRequired);
			}

			var invoice = await dbcontext.SubscriptionInvoices
				.AsNoTracking()
				.Where(
					x =>
						x.Id == invoiceId &&
						!x.IsDeleted)
				.Select(
					x => new IssueInvoiceSource(
						x.Id,
						x.InvoiceNo,

						x.TenantId,
						x.Tenant.Code,
						x.Tenant.Slug,
						x.Tenant.LegalName,
						x.Tenant.Status,

						x.SubscriptionId,
						x.Subscription.Status,
						x.Subscription.EndsAt,

						x.Status,
						x.BillingPeriodFrom,
						x.BillingPeriodTo,
						x.DueAt,

						x.TotalAmount,
						x.PaidAmount,
						x.Currency,

						x.IssuedByAdminId,
						x.IssuedAt))
				.SingleOrDefaultAsync(cancellationToken);

			if(invoice is null)
			{
				return IssueSubscriptionInvoiceResult.Failed(
					IssueSubscriptionInvoiceFailure.InvoiceNotFound);
			}

			if(invoice.InvoiceStatus != SubscriptionInvoiceStatus.Draft ||
				invoice.IssuedByAdminId.HasValue ||
				invoice.IssuedAt.HasValue)
			{
				return IssueSubscriptionInvoiceResult.Failed(
					IssueSubscriptionInvoiceFailure.InvoiceNotDraft);
			}

			var renewal = await dbcontext.SubscriptionRenewalRequests
				.AsNoTracking()
				.Where(x => x.SubscriptionInvoiceId == invoiceId)
				.Select(
					x => new IssueRenewalSource(
						x.Id,
						x.TenantId,
						x.SubscriptionId,
						x.Status,
						x.CurrentEndsAtSnapshot,

						x.SelectedPlanId,
						x.PlanNameSnapshot,
						x.PriceSnapshot,
						x.CurrencySnapshot,
						x.BillingPeriodFrom,
						x.BillingPeriodTo,

						x.SubscriptionInvoiceId))
				.SingleOrDefaultAsync(cancellationToken);

			if(renewal is null)
			{
				return IssueSubscriptionInvoiceResult.Failed(
					IssueSubscriptionInvoiceFailure.RenewalRequestNotFound);
			}

			if (renewal.RenewalStatus != SubscriptionRenewalRequestStatus.InvoiceDraftCreated ||
				renewal.SubscriptionInvoiceId != invoice.InvoiceId)
			{
				return IssueSubscriptionInvoiceResult.Failed(
					IssueSubscriptionInvoiceFailure.RenewalRequestNotReady);
			}

			var expectedTenantStatus = invoice.SubscriptionStatus switch
			{
				SubscriptionStatusEnum.Trial => TenantStatusEnum.Trial,
				SubscriptionStatusEnum.Active => TenantStatusEnum.Active,

				_ => (TenantStatusEnum?)null
			};

			if(!expectedTenantStatus.HasValue ||
				invoice.TenantStatus != expectedTenantStatus.Value ||
				invoice.SubscriptionEndsAt != renewal.CurrentEndsAtSnapshot)
			{
				return IssueSubscriptionInvoiceResult.Failed(
					IssueSubscriptionInvoiceFailure.RenewalRequestStale);
			}

			if(!renewal.SelectedPlanId.HasValue ||
				string.IsNullOrWhiteSpace(renewal.PlanNameSnapshot) ||
				!renewal.PriceSnapshot.HasValue ||
				renewal.PriceSnapshot.Value <= 0 ||
				string.IsNullOrWhiteSpace(renewal.CurrencySnapshot) ||
				!renewal.BillingPeriodFrom.HasValue ||
				!renewal.BillingPeriodTo.HasValue ||
				
				invoice.TenantId != renewal.TenantId ||
				invoice.SubscriptionId != renewal.SubscriptionId ||
				
				!invoice.BillingPeriodFrom.HasValue ||
				!invoice.BillingPeriodTo.HasValue ||

				invoice.BillingPeriodFrom.Value != renewal.BillingPeriodFrom.Value ||
				invoice.BillingPeriodTo.Value != renewal.BillingPeriodTo.Value ||
				
				invoice.TotalAmount != renewal.PriceSnapshot.Value ||
				invoice.PaidAmount != 0 ||
				
				!string.Equals(
					invoice.Currency.Trim(),
					renewal.CurrencySnapshot.Trim(),
					StringComparison.OrdinalIgnoreCase) ||
					
				invoice.DueAt != CalculateDueAt(renewal.BillingPeriodFrom.Value))
			{
				return IssueSubscriptionInvoiceResult.Failed(
					IssueSubscriptionInvoiceFailure.InvoiceSnapshotMismatch);
			}

			var now = timeProvider.GetUtcNow();

			if (invoice.DueAt <= now)
			{
				return IssueSubscriptionInvoiceResult.Failed(
					IssueSubscriptionInvoiceFailure.InvoiceDueDateElapsed);
			}

			await using var transaction = await dbcontext.Database
				.BeginTransactionAsync(cancellationToken);

			var affectedInvoiceRows = await dbcontext.SubscriptionInvoices
				.Where(
					x =>
						x.Id == invoiceId &&
						!x.IsDeleted &&
						x.Status == SubscriptionInvoiceStatus.Draft &&
						x.IssuedByAdminId == null &&
						x.IssuedAt == null &&
						x.PaidAmount == 0)
				.ExecuteUpdateAsync(
					setters => setters
						.SetProperty(
							x => x.Status,
							SubscriptionInvoiceStatus.Issued)
						.SetProperty(
							x => x.IssuedByAdminId,
							(Guid?)platformAdminId)
						.SetProperty(
							x => x.IssuedAt,
							(DateTimeOffset?)now)
						.SetProperty(
							x => x.UpdatedAt,
							(DateTimeOffset?)now),
					cancellationToken);

			if(affectedInvoiceRows == 0)
			{
				await transaction.RollbackAsync(cancellationToken);

				return IssueSubscriptionInvoiceResult.Failed(
					IssueSubscriptionInvoiceFailure.InvoiceNotDraft);
			}

			var affectedRenewalRows =
				await dbcontext.SubscriptionRenewalRequests
					.Where(
						x =>
							x.Id == renewal.RenewalRequestId &&
							x.SubscriptionInvoiceId == invoiceId &&
							x.Status == SubscriptionRenewalRequestStatus.InvoiceDraftCreated)
					.ExecuteUpdateAsync(
						setters => setters
							.SetProperty(
								x => x.Status,
								SubscriptionRenewalRequestStatus.InvoiceIssued)
							.SetProperty(
								x => x.UpdatedAt,
								(DateTimeOffset?)now),
						cancellationToken);

			if(affectedRenewalRows == 0)
			{
				await transaction.RollbackAsync(cancellationToken);

				return IssueSubscriptionInvoiceResult.Failed(
					IssueSubscriptionInvoiceFailure.RenewalRequestNotReady);
			}

			await transaction.CommitAsync(cancellationToken);

			var response = new IssueSubscriptionInvoiceResponse(
				invoice.InvoiceId,
				invoice.InvoiceNo,

				invoice.TenantId,
				invoice.TenantCode,
				invoice.TenantSlug,
				invoice.TenantLegalName,

				invoice.SubscriptionId,
				renewal.RenewalRequestId,

				renewal.SelectedPlanId.Value,
				renewal.PlanNameSnapshot!,

				invoice.BillingPeriodFrom.Value,
				invoice.BillingPeriodTo.Value,
				invoice.DueAt,

				SubscriptionInvoiceStatus.Issued,
				SubscriptionRenewalRequestStatus.InvoiceIssued,

				invoice.TotalAmount,
				invoice.PaidAmount,
				invoice.Currency.Trim(),

				platformAdminId,
				now);

			return IssueSubscriptionInvoiceResult.Success(response);
		}

		private sealed record IssueInvoiceSource(
			Guid InvoiceId,
			string InvoiceNo,

			Guid TenantId,
			string TenantCode,
			string TenantSlug,
			string TenantLegalName,
			TenantStatusEnum TenantStatus,

			Guid SubscriptionId,
			SubscriptionStatusEnum SubscriptionStatus,
			DateOnly SubscriptionEndsAt,

			SubscriptionInvoiceStatus InvoiceStatus,
			DateOnly? BillingPeriodFrom,
			DateOnly? BillingPeriodTo,
			DateTimeOffset DueAt,

			decimal TotalAmount,
			decimal PaidAmount,
			string Currency,

			Guid? IssuedByAdminId,
			DateTimeOffset? IssuedAt);

		private sealed record IssueRenewalSource(
			Guid RenewalRequestId,
			Guid TenantId,
			Guid SubscriptionId,
			SubscriptionRenewalRequestStatus RenewalStatus,
			DateOnly CurrentEndsAtSnapshot,

			Guid? SelectedPlanId,
			string? PlanNameSnapshot,
			decimal? PriceSnapshot,
			string? CurrencySnapshot,
			DateOnly? BillingPeriodFrom,
			DateOnly? BillingPeriodTo,

			Guid? SubscriptionInvoiceId);

		private bool IsActivePeriodUniqueViolation(DbUpdateException exception)
		{
			return exception.InnerException is PostgresException
			{
				SqlState: PostgresErrorCodes.UniqueViolation,
				ConstraintName: ActivePeriodUniqueIndex
			};
		}

		private DateTimeOffset CalculateDueAt(DateOnly billingPeriodFrom)
		{
			var localDeadline = billingPeriodFrom
				.ToDateTime(TimeOnly.MinValue);

			return new DateTimeOffset(
				localDeadline,
				VietNamUtcOffset)
				.ToUniversalTime();
		}

		private static string CreateInvoiceNumber(
			long sequenceValue, 
			DateTimeOffset now)
		{
			var vietnamNow = now.ToOffset(VietNamUtcOffset);

			return $"SUB-{vietnamNow:yyyyMM}-{sequenceValue:D8}";
		}

		private bool TryGetCurrentUserId(out Guid userId)
		{
			userId = Guid.Empty;

			var value = httpContextAccessor
				.HttpContext?
				.User
				.FindFirstValue(AppClaimTypes.Subject);

			return Guid.TryParse(value, out userId);
		}

		private Task<bool> IsPlatformAdminAsync(
			Guid userId,
			CancellationToken cancellationToken)
		{
			return dbcontext.UserRoles
				.AsNoTracking()
				.AnyAsync(
					x =>
						x.UserId == userId &&
						x.Scope == RoleScopeEnum.Platform &&
						x.TenantId == null &&
						x.RevokedAt == null &&

						x.Role.Scope == RoleScopeEnum.Platform &&
						x.Role.TenantId == null &&
						x.Role.Code == DbInitializer.PlatformAdminRoleCode &&
						!x.Role.IsDeleted &&

						x.User.TenantId == null &&
						!x.User.IsDeleted &&
						x.User.Status == UserStatusEnum.Active &&
						x.User.EmailConfirmed,
					cancellationToken);
		}

		private sealed record RenewalInvoiceSource(
			Guid RequestId,
			Guid TenantId,
			string TenantCode,
			string TenantSlug,
			string TenantLegalName,
			TenantStatusEnum TenantStatus,
			Guid SubscriptionId,
			SubscriptionStatusEnum SubscriptionStatus,
			DateOnly SubscriptionEndsAt,
			SubscriptionRenewalRequestStatus RenewalStatus,
			DateOnly CurrentEndsAtSnapshot,
			Guid? SelectedPlanId,
			string? PlanNameSnapshot,
			int? BillingCycleMonthsSnapshot,
			decimal? PriceSnapshot,
			string? CurrencySnapshot,
			DateOnly? BillingPeriodFrom,
			DateOnly? BillingPeriodTo,
			Guid? SubscriptionInvoiceId);
	}
}