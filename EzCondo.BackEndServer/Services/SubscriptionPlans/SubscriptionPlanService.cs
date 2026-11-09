using System.Security.Claims;
using System.Text.Json;
using EzCondo.BackEndServer.Contracts.SubscriptionPlans;
using EzCondo.BackEndServer.Data;
using EzCondo.BackEndServer.Data.Entities.SchemaPlatform;
using Microsoft.EntityFrameworkCore;

namespace EzCondo.BackEndServer.Services.SubscriptionPlans
{
	public sealed class SubscriptionPlanService(
		ApplicationDbContext dbcontext,
		IHttpContextAccessor httpContextAccessor)
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
				IsAvailable = request.IsAvailable,
				CreatedByAdmin = GetCurrentUserId()
			};

			dbcontext.SubscriptionPlans.Add(subscriptionPlan);
			await dbcontext.SaveChangesAsync(cancellationToken);
			return ToResponse(subscriptionPlan);
		}

		public async Task<SubscriptionPlanResponse?> UpdateAsync(
			Guid id,
			UpdateSubscriptionPlanRequest request,
			CancellationToken cancellationToken = default)
		{
			ArgumentNullException.ThrowIfNull(request);

			var subscriptionPlan = await dbcontext.SubscriptionPlans
				.SingleOrDefaultAsync(x => x.Id == id && !x.IsDeleted, cancellationToken);

			if(subscriptionPlan is null)
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
			subscriptionPlan.UpdatedByAdmin = GetCurrentUserId();

			await dbcontext.SaveChangesAsync(cancellationToken);
			return ToResponse(subscriptionPlan);
		}

		public async Task<IReadOnlyList<SubscriptionPlanResponse>> GetAsync(
			CancellationToken cancellationToken = default)
		{
			var subscriptionPlans = await dbcontext.SubscriptionPlans
				.AsNoTracking()
				.Where(x => !x.IsDeleted)
				.OrderBy(x => x.Name)
				.ToListAsync(cancellationToken);

			return subscriptionPlans.Select(ToResponse).ToList();
		}

		public async Task<bool> DeleteAsync(
			Guid id,
			CancellationToken cancellationToken = default)
		{
			var subscriptionPlan = await dbcontext.SubscriptionPlans
				.SingleOrDefaultAsync(x => x.Id == id && !x.IsDeleted, cancellationToken);

			if(subscriptionPlan is null)
			{
				return false;
			}

			subscriptionPlan.IsDeleted = true;
			subscriptionPlan.IsAvailable = false;
			subscriptionPlan.DeletedByAdmin = GetCurrentUserId();
			await dbcontext.SaveChangesAsync(cancellationToken);
			return true;
		}

		public async Task<SubscriptionPlanResponse?> DiscontinueAsync(
			Guid id,
			CancellationToken cancellationToken = default)
		{
			var subscriptionPlan = await dbcontext.SubscriptionPlans
				.SingleOrDefaultAsync(x => x.Id == id && !x.IsDeleted, cancellationToken);

			if(subscriptionPlan is null)
			{
				return null;
			}

			if(subscriptionPlan.IsAvailable)
			{
				subscriptionPlan.IsAvailable = false;
				subscriptionPlan.UpdatedByAdmin = GetCurrentUserId();
				await dbcontext.SaveChangesAsync(cancellationToken);
			}

			return ToResponse(subscriptionPlan);
		}

		private Guid GetCurrentUserId()
		{
			var value = httpContextAccessor.HttpContext?.User
				.FindFirstValue(ClaimTypes.NameIdentifier);

			return Guid.TryParse(value, out var userId)
				? userId
				: throw new UnauthorizedAccessException("Không xác định được tài khoản Platform hiện tại.");
		}

		private static SubscriptionPlanResponse ToResponse(SubscriptionPlan subscriptionPlan)
		{
			using var document = JsonDocument.Parse(subscriptionPlan.FeatureLimits);
			return new SubscriptionPlanResponse(
				subscriptionPlan.Id,
				subscriptionPlan.Name,
				subscriptionPlan.Description,
				subscriptionPlan.BillingCycleMonths,
				subscriptionPlan.Price,
				document.RootElement.Clone(),
				subscriptionPlan.IsAvailable,
				subscriptionPlan.CreatedAt,
				subscriptionPlan.UpdatedAt);
		}
	}
}
