using EzCondo.BackEndServer.Contracts.Tenants;
using EzCondo.BackEndServer.Data;
using EzCondo.BackEndServer.Data.Entities.SchemaPlatform;
using EzCondo.BackEndServer.Data.Enums;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace EzCondo.BackEndServer.Services.Tenants
{
	public class TenantService(
		ApplicationDbContext dbcontext,
		DbInitializer dbInitializer,
		ITenantIdentityGenerator identityGenerator,
		TimeProvider timeProvider)
		: ITenantService
	{
		private const int TrialDurationDays = 30;

		public async Task<CreateTenantResult> CreateAsync(
			CreateTenantRequest request, 
			CancellationToken cancellationToken = default)
		{
			ArgumentNullException.ThrowIfNull(request);

			var legalName = request.LegalName.Trim();
			var taxCode = request.TaxCode.Trim();
			var address = request.Address.Trim();

			var subscriptionPlan = await dbcontext.SubscriptionPlans
				.AsNoTracking()
				.SingleOrDefaultAsync(
				x => x.Id == request.SubscriptionPlanId &&
				!x.IsDeleted,
				cancellationToken);

			if(subscriptionPlan is null)
			{
				return CreateTenantResult.Failed(
					CreateTenantFailure.SubscriptionPlanNotFound);
			}

			if (!subscriptionPlan.IsAvailable)
			{
				return CreateTenantResult.Failed(
					CreateTenantFailure.SubscriptionPlanUnavailable);
			}

			var taxCodeExists = await dbcontext.Tenants
				.AsNoTracking()
				.AnyAsync(
				x => x.TaxCode == taxCode,
				cancellationToken);

			if (taxCodeExists)
			{
				return CreateTenantResult.Failed(
					CreateTenantFailure.TaxCodeAlreadyExists);
			}

			var tenantId = Guid.NewGuid();
			var identity = identityGenerator.Generate(
				tenantId,
				legalName);
			var now = timeProvider.GetUtcNow();
			var trialEndsAt = now.AddDays(TrialDurationDays);
			var startsAt = DateOnly.FromDateTime(now.UtcDateTime);
			var endsAt = DateOnly.FromDateTime(
				trialEndsAt.UtcDateTime);

			var tenant = new Tenant
			{
				Id = tenantId,
				Code = identity.Code,
				Slug = identity.Slug,
				LegalName = legalName,
				TaxCode = taxCode,
				Address = address,
				Status = TenantStatusEnum.Trial,
				TrialEndsAt = trialEndsAt
			};

			var subscription = new Subscription
			{
				Id = Guid.NewGuid(),
				TenantId = tenantId,
				PlanId = subscriptionPlan.Id,
				BillingCycleMonthsSnapshot = subscriptionPlan.BillingCycleMonths,
				Status = SubscriptionStatusEnum.Trial,
				StartsAt = startsAt,
				EndsAt = endsAt
			};

			await using var transaction =
				await dbcontext.Database.BeginTransactionAsync(
					cancellationToken);

			try
			{
				dbcontext.Tenants.Add(tenant);
				dbcontext.Subscriptions.Add(subscription);

				await dbcontext.SaveChangesAsync(cancellationToken);

				await dbInitializer
					.SeedTenantDefaultCurrentTransactionAsync(
					tenantId,
					cancellationToken);

				await transaction.CommitAsync(cancellationToken);
			}
			catch(DbUpdateException exception)
			{
				await transaction.RollbackAsync(cancellationToken);

				var failure = MaqUniqueViolation(exception);

				if (failure.HasValue)
				{
					return CreateTenantResult.Failed(failure.Value);
				}

				throw;
			}
			catch
			{
				await transaction.RollbackAsync(cancellationToken);
				throw;
			}

			return CreateTenantResult.Success(
				ToResponse(
					tenant,
					subscription,
					subscriptionPlan.Name));
		}

		private CreateTenantFailure? MaqUniqueViolation(
			DbUpdateException exception)
		{
			if(exception.InnerException is not PostgresException
				{
					SqlState: PostgresErrorCodes.UniqueViolation
				} postgreException)
			{
				return null;
			}

			return postgreException.ConstraintName switch
			{
				"IX_tenants_tax_code" =>
				CreateTenantFailure.TaxCodeAlreadyExists,

				"IX_tenants_code" or "IX_tenants_slug" =>
					CreateTenantFailure.GeneratedidentityCollision,

				_ => null
			};
		}

		private static TenantResponse ToResponse(
			Tenant tenant,
			Subscription subscription,
			string planName)
		{
			return new TenantResponse(
				tenant.Id,
				tenant.Code,
				tenant.Slug,
				tenant.LegalName,
				tenant.TaxCode,
				tenant.Address,
				tenant.TrialEndsAt!.Value,
				tenant.Status,
				new TenantSubscriptionResponse(
					subscription.Id,
					subscription.PlanId,
					planName,
					subscription.Status,
					subscription.StartsAt,
					subscription.EndsAt),
				tenant.CreatedAt,
				tenant.UpdatedAt);
		}
	}
}
