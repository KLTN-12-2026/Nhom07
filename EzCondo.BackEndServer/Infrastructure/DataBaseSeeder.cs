using EzCondo.BackEndServer.Data;
using EzCondo.BackEndServer.Data.Entities.SchemaIdentity;
using EzCondo.BackEndServer.Data.Entities.SchemaSecurity;
using EzCondo.BackEndServer.Data.Enums;
using EzCondo.BackEndServer.Infrastructure.Authentication.Options;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace EzCondo.BackEndServer.Infrastructure
{
	public static class DataBaseSeeder
	{
		private const long PlatformAccountSeederLockId = 7_398_260_110_100_002;
		public static async Task SeedPlatformAccountsAsync(
			IServiceProvider serviceProvider,
			CancellationToken cancellationToken)
		{
			await using var scope = serviceProvider.CreateAsyncScope();

			var userManager = scope.ServiceProvider
				.GetRequiredService<UserManager<User>>();

			var dbcontext = scope.ServiceProvider
				.GetRequiredService<ApplicationDbContext>();

			var options = scope.ServiceProvider
				.GetRequiredService<IOptions<PlatformAccountsOptions>>().Value;

			var logger = scope.ServiceProvider
				.GetRequiredService<ILoggerFactory>()
				.CreateLogger(nameof(DataBaseSeeder));

			ValidateConfiguration(options);

			if(options.Accounts.Count == 0)
			{
				logger.LogWarning(
					"Không có tài khoản Platform nào được cấu hình.");

				return;
			}

			await using var transaction = await dbcontext.Database
				.BeginTransactionAsync(cancellationToken);

			if(dbcontext.Database.ProviderName ==
				"Npgsql.EntityFrameworkCore.PostgreSQL")
			{
				await dbcontext.Database.ExecuteSqlRawAsync(
					$"SELECT pg_advisory_xact_lock({PlatformAccountSeederLockId})",
					cancellationToken);
			}

			foreach (var account in options.Accounts)
			{
				await SeedAccountAsync(
					account,
					userManager,
					dbcontext,
					logger,
					cancellationToken
				);
			}
				await dbcontext.SaveChangesAsync(cancellationToken);
				await transaction.CommitAsync(cancellationToken);
		}

			private static async Task SeedAccountAsync(
				PlatformAccountConfig account,
				UserManager<User> userManager,
				ApplicationDbContext dbcontext,
				ILogger logger,
				CancellationToken cancellationToken)
		{
			var email = account.Email.Trim();

			var user = await userManager.FindByEmailAsync(email);

			if(user is null)
			{
				user = new User
				{
					Id = Guid.NewGuid(),
					UserName = email,
					Email = email,
					FullName = account.FullName.Trim(),
					CitizenId = account.CitizenId.Trim(),
					TenantId = null,
					Status = UserStatusEnum.Active,
					EmailConfirmed = true,
					LockoutEnabled = true,
					CreatedAt = DateTimeOffset.UtcNow
				};

				var createResult = await userManager.CreateAsync(
					user,
					account.Password);

				EnsureSucceeded(
					createResult,
					$"Không thể tạo tài khoản Platform '{email}'");
			}
			
			var platformUser = user ?? throw new InvalidOperationException(
				$"Không thể khởi tạo tài khoản Platform '{email}'.");

			if(platformUser.TenantId is not null)
			{
				throw new InvalidOperationException(
					$"Email '{email}' đã thuộc một tenant.");
			}

			// Không thay mật khẩu đã có; chỉ bổ sung cho tài khoản chưa có mật khẩu.
			if(!await userManager.HasPasswordAsync(platformUser))
			{
				var passwordResult = await userManager.AddPasswordAsync(
					platformUser,
					account.Password);

				EnsureSucceeded(
					passwordResult,
					$"Không thể thêm mật khẩu cho '{email}'");
			}

			platformUser.UserName = email;
			platformUser.Email = email;
			platformUser.FullName = account.FullName.Trim();
			platformUser.CitizenId = account.CitizenId.Trim();
			platformUser.TenantId = null;
			platformUser.Status = UserStatusEnum.Active;
			platformUser.EmailConfirmed = true;
			platformUser.LockoutEnabled = true;

			var updateResult = await userManager.UpdateAsync(platformUser);

			EnsureSucceeded(
				updateResult,
				$"Không thể cập nhật tài khoản Platform '{email}'"
			);

			await EnsurePlatformAdminRoleAsync(
				platformUser,
				dbcontext,
				cancellationToken);

			logger.LogInformation(
				"Đã khởi tạo hoặc đồng bộ tài khoản Platform {Email}",
				email);
		}

		private static async Task EnsurePlatformAdminRoleAsync(
			User user,
			ApplicationDbContext dbcontext,
			CancellationToken cancellationToken)
		{
			var role = await dbcontext.Roles.SingleOrDefaultAsync(
				x => x.Scope == RoleScopeEnum.Platform &&
					x.TenantId == null &&
					x.Code == DbInitializer.PlatformAdminRoleCode &&
					!x.IsDeleted,
				cancellationToken);

			if(role is null)
			{
				throw new InvalidOperationException(
					$"Platform role '{DbInitializer.PlatformAdminRoleCode}' chưa được khởi tạo.");
			}

			var activeAssignments = await dbcontext.UserRoles
				.Where(x => x.Scope == RoleScopeEnum.Platform &&
					x.UserId == user.Id &&
					x.RevokedAt == null)
				.ToListAsync(cancellationToken);

			var now = DateTimeOffset.UtcNow;
			foreach(var assignment in activeAssignments.Where(x => x.RoleId != role.Id))
			{
				assignment.RevokedAt = now;
			}

			if(activeAssignments.Any(x => x.RoleId == role.Id))
			{
				return;
			}

			dbcontext.UserRoles.Add(new UserRole
			{
				Id = Guid.NewGuid(),
				Scope = RoleScopeEnum.Platform,
				TenantId = null,
				UserId = user.Id,
				RoleId = role.Id,
				GrantedByUserId = null,
				GrantedAt = now
			});
		}

		private static void ValidateConfiguration(PlatformAccountsOptions options)
		{
			foreach(var account in options.Accounts)
			{
				if(string.IsNullOrWhiteSpace(account.Email) ||
					string.IsNullOrWhiteSpace(account.Password) ||
					string.IsNullOrWhiteSpace(account.FullName) ||
					string.IsNullOrWhiteSpace(account.CitizenId))
				{
					throw new InvalidOperationException(
						"Mỗi tài khoản Platform phải có Email, Pasword," +
						" FullName và CitizentId.");
				}
			}

			var duplicateEmails = options.Accounts
				.GroupBy(
				x => x.Email.Trim(),
				StringComparer.OrdinalIgnoreCase)
				.Where(x => x.Count() > 1)
				.Select(x => x.Key)
				.ToArray();

			if(duplicateEmails.Length > 0)
			{
				throw new InvalidOperationException(
					"Cấu hình PlatformAccounts chứa email trùng: " +
					string.Join(", ", duplicateEmails));
			}
		}

		private static void EnsureSucceeded(
			IdentityResult result,
			string message)
		{
			if (result.Succeeded)
			{
				return;
			}

			var errors = string.Join(
				"; ",
				result.Errors.Select(x =>
				$"{x.Code}: {x.Description}"));

			throw new InvalidOperationException(
				$"{message}. Chi tiết: {errors}");
		}
	}
}
