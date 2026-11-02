using EzCondo.BackEndServer.Contracts.SubscriptionPlans;
using EzCondo.BackEndServer.Data;
using EzCondo.BackEndServer.Data.Entities;
using EzCondo.ViewModel.Contents;
using Microsoft.AspNetCore.Authentication.BearerToken;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace EzCondo.BackEndServer.Services.SubscriptionPlans
{
	public sealed class SubscriptionPlanService(ApplicationDbContext dbcontext) 
		: ISubscriptionPlanService
	{
		public async Task<SubscriptionPlanResponse> CreateAsync(
			CreateSubscriptionPlanRequest request, 
			CancellationToken cancellationToken = default)
		{
			ArgumentNullException.ThrowIfNull(request);

			var subscriptionPlan = new SubscriptionPlan
			{
				Name = request.Name.Trim(),

				Description = string.IsNullOrWhiteSpace(request.Description)
				? null
				: request.Description.Trim(),

				BillingCycleMonths = request.BillingCycleMonths,
				Price = request.Price,

				FeatureLimits = request.FeatureLimits?.GetRawText() ?? "{}",

				IsAvailable = request.IsAvailable
			};

			dbcontext.SubscriptionPlans.Add(subscriptionPlan);

			await dbcontext.SaveChangesAsync(cancellationToken);

			return ToResponse(subscriptionPlan);
		}

		private static SubscriptionPlanResponse ToResponse(SubscriptionPlan subscriptionPlan)
		{
			using var document = JsonDocument.Parse(subscriptionPlan.FeatureLimits);

			var featureLimits = document.RootElement.Clone();

			return new SubscriptionPlanResponse(
				subscriptionPlan.Id,
				subscriptionPlan.Name,
				subscriptionPlan.Description,
				subscriptionPlan.BillingCycleMonths,
				subscriptionPlan.Price,
				featureLimits,
				subscriptionPlan.IsAvailable,
				subscriptionPlan.CreatedAt,
				subscriptionPlan.UpdatedAt
				);
		}

		public async Task<SubscriptionPlanResponse?> UpdateAsync(
			Guid id,
			UpdateSubscriptionPlanRequest request, 
			CancellationToken cancellationToken = default)
		{
			ArgumentNullException.ThrowIfNull(request);

			var subscriptionPlan = await dbcontext.SubscriptionPlans
				.SingleOrDefaultAsync(
				x => x.Id == id && !x.IsDeleted,
				cancellationToken);

			if (subscriptionPlan is null)
			{
				return null;
			}

			subscriptionPlan.Name = request.Name.Trim();

			subscriptionPlan.Description = string.IsNullOrWhiteSpace(request.Description)
				? null
				: request.Description.Trim();

			subscriptionPlan.Price = request.Price;
			subscriptionPlan.FeatureLimits = request.FeatureLimits?.GetRawText() ?? "{}";
			subscriptionPlan.IsAvailable = request.IsAvailable;
			subscriptionPlan.UpdatedAt = DateTimeOffset.UtcNow;

			await dbcontext.SaveChangesAsync(cancellationToken);

			return ToResponse(subscriptionPlan);
		}

		public async Task<IReadOnlyList<SubscriptionPlanResponse>> GetAsync(
			CancellationToken cancellationToken = default)
		{
			var subscriptionPlans = await dbcontext.SubscriptionPlans
				.AsNoTracking() //Chỉ đọc
				.Where(x => !x.IsDeleted)
				.OrderBy(x => x.Name)
				.ToListAsync(cancellationToken);

			return subscriptionPlans
				.Select(ToResponse)
				.ToList();
		}

		public async Task<bool> DeleteAsync(Guid id, 
			CancellationToken cancellationToken = default)
		{
			var subscriptionPlan = await dbcontext.SubscriptionPlans
				.SingleOrDefaultAsync(
				x => x.Id == id && !x.IsDeleted,
				cancellationToken);

			if(subscriptionPlan is null)
			{
				return false;
			}

			subscriptionPlan.IsDeleted = true;
			subscriptionPlan.IsAvailable = false;
			subscriptionPlan.DeletedAt = DateTimeOffset.UtcNow;
			subscriptionPlan.UpdatedAt = DateTimeOffset.UtcNow;

			await dbcontext.SaveChangesAsync(cancellationToken);

			return true;
		}
	}
}
