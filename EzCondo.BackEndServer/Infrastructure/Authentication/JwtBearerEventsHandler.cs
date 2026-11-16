using EzCondo.BackEndServer.Data;
using EzCondo.BackEndServer.Data.Entities.SchemaIdentity;
using EzCondo.BackEndServer.Data.Enums;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace EzCondo.BackEndServer.Infrastructure.Authentication
{
	public sealed class JwtBearerEventsHandler(
		UserManager<User> userManager,
		ApplicationDbContext dbcontext,
		TimeProvider timeProvider)
		: JwtBearerEvents
	{
		public override async Task TokenValidated(
			TokenValidatedContext context)
		{
			var principal = context.Principal;

			var subject = principal?.FindFirstValue(
				AppClaimTypes.Subject);

			var tokenSecurityStamp = principal?.FindFirstValue(AppClaimTypes.SecurityStamp);

			if(!Guid.TryParse(subject, out var userId) ||
				string.IsNullOrWhiteSpace(tokenSecurityStamp))
			{
				context.Fail(
					"Token không chứa thông tin người dùng hợp lệ.");
				return;
			}
			var user = await userManager.FindByIdAsync(userId.ToString());

			if(user is null ||
				user.IsDeleted ||
				user.Status != UserStatusEnum.Active)
			{
				context.Fail("Tài khoản không còn hoạt động.");
				return;
			}

			if(!string.Equals(
				user.SecurityStamp,
				tokenSecurityStamp,
				StringComparison.Ordinal))
			{
				context.Fail("Token đã bị thu hồi.");
				return;
			}

			var scope = principal?.FindFirstValue(AppClaimTypes.Scope);

			if(scope == "platform")
			{
				if(user.TenantId is not null)
				{
					context.Fail("Platform token không khớp tài khoản.");
					return;
				}
				return;
			}

			if(scope != "tenant")
			{
				context.Fail("Token không có scope hợp lệ.");
				return;
			}

			var tenantIdValue = principal?.FindFirstValue(AppClaimTypes.TenantId);

			if(!Guid.TryParse(
				tenantIdValue, 
				out var tokenTenantId) ||
				user.TenantId != tokenTenantId)
			{
				context.Fail("Tenant context trong token không hợp lệ.");
				return;
			}

			var tenant = await dbcontext.Tenants
				.AsNoTracking()
				.SingleOrDefaultAsync(
				x => x.Id == tokenTenantId,
				context.HttpContext.RequestAborted);

			if(tenant is null)
			{
				context.Fail("Không tìm thấy tenant.");
				return;
			}

			var now = timeProvider.GetUtcNow();

			var tenantIsAvailable =
				tenant.Status == TenantStatusEnum.Active ||
				(
					tenant.Status == TenantStatusEnum.Trial &&
					tenant.TrialEndsAt.HasValue &&
					tenant.TrialEndsAt.Value > now
				);

			if (!tenantIsAvailable)
			{
				context.Fail("Tenant không còn hoạt động.");
			}
		}
	}
}
