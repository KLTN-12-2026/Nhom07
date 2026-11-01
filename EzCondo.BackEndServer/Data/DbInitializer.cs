using Microsoft.EntityFrameworkCore;

using EzCondo.BackEndServer.Data.Entities;

namespace EzCondo.BackEndServer.Data
{
	public sealed class DbInitializer
	{
		public const string PlatformAdminRoleCode = "PLATFORM_ADMIN";
		public const string TenantAdminRoleCode = "TENANT_ADMIN";
		public const string ManagerRoleCode = "MANAGER";
		public const string StaffRoleCode = "STAFF";
		public const string ResidentRoleCode = "RESIDENT";

		private const long SeederAdvisoryLockId = 7_398_260_110_100_001;

		private readonly ApplicationDbContext _context;
		private readonly ILogger<DbInitializer> _logger;

		public DbInitializer(ApplicationDbContext context, ILogger<DbInitializer> logger)
		{
			_context = context;
			_logger = logger;
		}

		public async Task InitializeAsync(CancellationToken cancellationToken = default)
		{
			await _context.Database.MigrateAsync(cancellationToken);
			await SeedAsync(cancellationToken);
		}

		public async Task SeedAsync(CancellationToken cancellationToken = default)
		{
			ValidateSeedDefinitions();

			await using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);

			await AcquireSeederLockAsync(cancellationToken);
			await UpsertPlatformPermissionAsync(cancellationToken);
			await UpsertTenantPermissionAsync(cancellationToken);
			await SeedPlatformAdminRoleAsync(cancellationToken);

			var tenantIds = await _context.Tenants
				.AsNoTracking()
				.Select(x => x.Id)
				.ToListAsync(cancellationToken);

			foreach(var tenantId in tenantIds)
			{
				await SeedTenantRolesCoreAsync(tenantId, cancellationToken);
			}

			await _context.SaveChangesAsync(cancellationToken);
			await transaction.CommitAsync(cancellationToken);

			_logger.LogInformation(
				"Database reference data was initialized for {TenantCount} tenant(s).",
				tenantIds.Count);
		}

		public async Task SeedTenantDefaultsAsync(Guid tenantId, CancellationToken cancellationToken = default)
		{
			ValidateSeedDefinitions();

			await using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);

			await AcquireSeederLockAsync(cancellationToken);

			var tenantExists = await _context.Tenants.AnyAsync(x => x.Id == tenantId, cancellationToken);

			if (!tenantExists)
			{
				throw new InvalidOperationException($"Cannot seed default roles because tenant '{tenantId}' does not exist.");
			}

			await UpsertTenantPermissionAsync(cancellationToken);
			await SeedTenantRolesCoreAsync(tenantId, cancellationToken);

			await _context.SaveChangesAsync(cancellationToken);
			await transaction.CommitAsync(cancellationToken);

			_logger.LogInformation(
				"Default roles were initialized for tenant {tenantId}.",
				tenantId);
		}

		private async Task UpsertPlatformPermissionAsync(CancellationToken cancellationToken)
		{
			var existingPermissions = await _context.PlatformPermissions
				.ToDictionaryAsync(x => x.Code, cancellationToken);

			foreach(var seed in PlatformPermissionSeeds)
			{
				if(existingPermissions.TryGetValue(seed.Code, out var permission))
				{
					permission.Description = seed.Description;
					continue;
				}

				_context.PlatformPermissions.Add(new PlatformPermission
				{
					Code = seed.Code,
					Description = seed.Description
				});
			}
		}

		private async Task UpsertTenantPermissionAsync(CancellationToken cancellationToken)
		{
			var existingPermissions = await _context.TenantPermissions
				.ToDictionaryAsync(x => x.Code, cancellationToken);

			foreach(var seed in TenantPermissionSeeds)
			{
				if(existingPermissions.TryGetValue(seed.Code, out var permission))
				{
					permission.Description = seed.Description;
					continue;
				}

				_context.TenantPermissions.Add(new TenantPermission
				{
					Code = seed.Code,
					Description = seed.Description
				});
			}
		}

		private async Task SeedPlatformAdminRoleAsync(CancellationToken cancellationToken)
		{
			var role = await _context.PlatformRoles
				.SingleOrDefaultAsync(
				x => x.Code == PlatformAdminRoleCode,
				cancellationToken);

			if(role is null)
			{
				role = new PlatformRole
				{
					Id = Guid.NewGuid(),
					Code = PlatformAdminRoleCode,
					Name = "Quản trị viên nền tảng",
					IsSystem = true,
					CreatedAt = DateTimeOffset.UtcNow
				};

				_context.PlatformRoles.Add(role);
			}

			var assignedPermissionCodeList = await _context.PlatformRolePermissions
				.Where(x => x.RoleId == role.Id)
				.Select(x => x.PermissionCode)
				.ToListAsync(cancellationToken);

			var assignedPermissionCodes = assignedPermissionCodeList
				.ToHashSet(StringComparer.Ordinal);

			foreach(var permission in PlatformPermissionSeeds)
			{
				if (!assignedPermissionCodes.Contains(permission.Code))
				{
					_context.PlatformRolePermissions.Add(new PlatformRolePermission
					{
						RoleId = role.Id,
						PermissionCode = permission.Code
					});
				}
			}
		}

		private async Task SeedTenantRolesCoreAsync(Guid tenantId, CancellationToken cancellationToken)
		{
			var existingRoles = await _context.TenantRoles
				.Where(x => x.TenantId == tenantId)
				.ToDictionaryAsync(x => x.Code, cancellationToken);

			foreach(var roleSeed in TenantRoleSeeds)
			{
				if (existingRoles.ContainsKey(roleSeed.Code))
				{
					continue;
				}

				var role = new TenantRole
				{
					Role = Guid.NewGuid(),
					TenantId = tenantId,
					Code = roleSeed.Code,
					Name = roleSeed.Name,
					HierarchyLevel = roleSeed.HierarchyLevel,
					IsSystem = true,
					CreatedAt = DateTimeOffset.UtcNow
				};

				_context.TenantRoles.Add(role);

				foreach(var permissionCode in roleSeed.PermissionCodes)
				{
					_context.TenantRolePermissions.Add(new TenantRolePermission
					{
						TenantId = tenantId,
						RoleId = role.Role,
						PermissionCode = permissionCode
					});
				}
			}
		}

		private async Task AcquireSeederLockAsync(CancellationToken cancellationToken)
		{
			if(_context.Database.ProviderName != "Npgsql.EntityFrameworkCore.PostgreSQL")
			{
				return;
			}

			await _context.Database.ExecuteSqlRawAsync(
				$"SELECT pg_advisory_xact_lock({SeederAdvisoryLockId})",
				cancellationToken);
		}

		private static void ValidateSeedDefinitions()
		{
			EnsureUniqueCodes(PlatformPermissionSeeds.Select(x => x.Code), "Platform permission");
			EnsureUniqueCodes(TenantPermissionSeeds.Select(x => x.Code), "Tenant permission");
			EnsureUniqueCodes(TenantRoleSeeds.Select(x => x.Code), "Tenant role");

			var tenantPermissionCodes = TenantPermissionSeeds
				.Select(x => x.Code)
				.ToHashSet(StringComparer.Ordinal);

			foreach(var role in TenantRoleSeeds)
			{
				var unknownCodes = role.PermissionCodes
					.Where(x => !tenantPermissionCodes.Contains(x))
					.ToArray();

				if(unknownCodes.Length > 0)
				{
					throw new InvalidOperationException(
						$"Role '{role.Code}' contains unknown permission codes: " +
						string.Join(", ", unknownCodes));
				}
			}
		}

		private static void EnsureUniqueCodes(IEnumerable<string> codes, string definitionName)
		{
			var duplicateCodes = codes
				.GroupBy(x => x, StringComparer.Ordinal)
				.Where(x => x.Count() > 1)
				.Select(x => x.Key)
				.ToArray();

			if(duplicateCodes.Length > 0)
			{
				throw new InvalidOperationException(
					$"{definitionName} seed contains duplicate codes: " +
					string.Join(", ", duplicateCodes));
			}
		}

		private sealed record PermissionSeed(string Code, string Description);

		private sealed record TenantRoleSeed(
			string Code,
			string Name,
			int HierarchyLevel,
			IReadOnlyCollection<string> PermissionCodes);

		private static readonly PermissionSeed[] PlatformPermissionSeeds =
		{
			new("platform.tenants.read", "Xem danh sách và chi tiết tenant"),
			new("platform.tenants.create", "Tạo tenant"),
			new("platform.tenants.update", "Cập nhật thông tin tenant"),
			new("platform.tenants.change_status", "Thay đổi trạng thái tenant"),
			new("platform.tenant_invitations.read", "Xem lời mời chủ chung cư"),
			new("platform.tenant_invitations.create", "Tạo lời mời chủ chung cư"),
			new("platform.tenant_invitations.revoke", "Thu hồi lời mời chủ chung cư"),
			new("platform.subscription_plans.read", "Xem gói thuê bao"),
			new("platform.subscription_plans.create", "Tạo gói thuê bao"),
			new("platform.subscription_plans.update", "Cập nhật gói thuê bao"),
			new("platform.subscription_plans.retire", "Ngừng cung cấp gói thuê bao"),
			new("platform.subscriptions.read", "Xem thuê bao của tenant"),
			new("platform.subscriptions.create", "Tạo thuê bao cho tenant"),
			new("platform.subscriptions.update", "Cập nhật thuê bao"),
			new("platform.subscriptions.cancel", "Hủy thuê bao"),
			new("platform.subscriptions.notify_renewal", "Gửi thông báo gia hạn thuê bao"),
			new("platform.subscription_invoices.read", "Xem hóa đơn thuê bao"),
			new("platform.subscription_invoices.create", "Tạo hóa đơn thuê bao"),
			new("platform.subscription_invoices.issue", "Phát hành hóa đơn thuê bao"),
			new("platform.subscription_invoices.cancel", "Hủy hóa đơn thuê bao"),
			new("platform.subscription_payments.read", "Xem thanh toán thuê bao"),
			new("platform.subscription_payments.reconcile", "Đối soát thanh toán thuê bao"),
			new("platform.subscription_payments.refund", "Hoàn tiền thanh toán thuê bao"),
			new("platform.subscription_payments.cancel", "Hủy lệnh thanh toán thuê bao"),
			new("platform.support_tickets.read", "Xem yêu cầu hỗ trợ"),
			new("platform.support_tickets.assign", "Phân công yêu cầu hỗ trợ"),
			new("platform.support_tickets.update", "Cập nhật yêu cầu hỗ trợ"),
			new("platform.support_tickets.resolve", "Hoàn tất yêu cầu hỗ trợ"),
			new("platform.incidents.read", "Xem sự cố hệ thống"),
			new("platform.incidents.create", "Ghi nhận sự cố hệ thống"),
			new("platform.incidents.assign", "Phân công xử lý sự cố hệ thống"),
			new("platform.incidents.update", "Cập nhật sự cố hệ thống"),
			new("platform.incidents.resolve", "Kết thúc sự cố hệ thống"),
			new("platform.incidents.notify", "Gửi thông báo sự cố cho tenant bị ảnh hưởng"),
			new("platform.monitoring.read", "Xem giám sát vận hành nền tảng"),
			new("platform.reports.read", "Xem báo cáo nền tảng"),
			new("platform.reports.export", "Xuất báo cáo nền tảng"),
			new("platform.users.read", "Xem người dùng nền tảng"),
			new("platform.users.invite", "Mời người dùng nền tảng"),
			new("platform.users.assign_role", "Gán vai trò nền tảng")
		};

		private static readonly PermissionSeed[] TenantPermissionSeeds =
		{
			new("tenant.settings.read", "Xem cấu hình tenant"),
			new("tenant.settings.update", "Cập nhật cấu hình tenant"),
			new("tenant.roles.read", "Xem vai trò tenant"),
			new("tenant.roles.create", "Tạo vai trò tenant"),
			new("tenant.roles.update", "Cập nhật vai trò tenant"),
			new("tenant.roles.delete", "Xóa vai trò tenant"),
			new("tenant.roles.assign", "Gán vai trò tenant cho người dùng"),
			new("tenant.staff.read", "Xem nhân viên"),
			new("tenant.staff.create", "Tạo hồ sơ nhân viên"),
			new("tenant.staff.update", "Cập nhật hồ sơ nhân viên"),
			new("tenant.staff.invite", "Mời nhân viên tham gia tenant"),
			new("tenant.staff.revoke_invitation", "Thu hồi lời mời nhân viên"),
			new("tenant.staff.assign_role", "Thay đổi vai trò nhân viên"),
			new("tenant.apartments.read", "Xem căn hộ trong tenant"),
			new("tenant.apartments.create", "Tạo căn hộ"),
			new("tenant.apartments.update", "Cập nhật căn hộ"),
			new("tenant.apartments.read_own", "Xem căn hộ của bản thân"),
			new("tenant.residents.read", "Xem cư dân"),
			new("tenant.residents.create", "Tạo hồ sơ cư dân"),
			new("tenant.residents.update", "Cập nhật hồ sơ cư dân"),
			new("tenant.residents.invite", "Mời cư dân tham gia tenant"),
			new("tenant.contracts.read", "Xem hợp đồng căn hộ"),
			new("tenant.contracts.create", "Tạo hợp đồng căn hộ"),
			new("tenant.contracts.update", "Cập nhật hợp đồng căn hộ"),
			new("tenant.contracts.import", "Nhập hợp đồng căn hộ"),
			new("tenant.contracts.renew", "Gia hạn hợp đồng căn hộ"),
			new("tenant.contracts.read_own", "Xem hợp đồng của bản thân"),
			new("tenant.services.read", "Xem dịch vụ tenant"),
			new("tenant.services.create", "Tạo dịch vụ tenant"),
			new("tenant.services.update", "Cập nhật dịch vụ tenant"),
			new("tenant.services.change_status", "Thay đổi trạng thái dịch vụ tenant"),
			new("tenant.billing_periods.read", "Xem kỳ tính phí"),
			new("tenant.billing_periods.create", "Tạo kỳ tính phí"),
			new("tenant.billing_periods.update", "Cập nhật kỳ tính phí"),
			new("tenant.billing_periods.issue", "Phát hành kỳ tính phí"),
			new("tenant.billing_periods.close", "Đóng kỳ tính phí"),
			new("tenant.invoices.read", "Xem hóa đơn tenant"),
			new("tenant.invoices.create", "Tạo hóa đơn tenant"),
			new("tenant.invoices.update", "Cập nhật hóa đơn tenant"),
			new("tenant.invoices.issue", "Phát hành hóa đơn tenant"),
			new("tenant.invoices.cancel", "Hủy hóa đơn tenant"),
			new("tenant.invoices.send", "Gửi hóa đơn cho cư dân"),
			new("tenant.invoices.read_own", "Xem hóa đơn của bản thân"),
			new("tenant.payments.read", "Xem thanh toán tenant"),
			new("tenant.payments.create", "Tạo lệnh thanh toán tenant"),
			new("tenant.payments.reconcile", "Đối soát thanh toán tenant"),
			new("tenant.payments.pay_own", "Thanh toán nghĩa vụ của bản thân"),
			new("tenant.payroll_batches.read", "Xem bảng lương"),
			new("tenant.payroll_batches.create", "Lập bảng lương nháp"),
			new("tenant.payroll_batches.update", "Cập nhật bảng lương nháp"),
			new("tenant.payroll_batches.submit", "Trình duyệt bảng lương"),
			new("tenant.payroll_batches.approve", "Duyệt bảng lương"),
			new("tenant.payroll_batches.reject", "Từ chối bảng lương"),
			new("tenant.payroll_batches.issue", "Phát hành bảng lương đã duyệt"),
			new("tenant.payroll_entries.read_own", "Xem mục lương của bản thân"),
			new("tenant.refunds.read", "Xem yêu cầu hoàn tiền"),
			new("tenant.refunds.create", "Lập yêu cầu hoàn tiền nháp"),
			new("tenant.refunds.update", "Cập nhật yêu cầu hoàn tiền nháp"),
			new("tenant.refunds.submit", "Trình duyệt yêu cầu hoàn tiền"),
			new("tenant.refunds.approve", "Duyệt yêu cầu hoàn tiền"),
			new("tenant.refunds.reject", "Từ chối yêu cầu hoàn tiền"),
			new("tenant.refunds.issue", "Thực hiện hoàn tiền đã duyệt"),
			new("tenant.complaints.read", "Xem khiếu nại trong tenant"),
			new("tenant.complaints.create", "Tạo khiếu nại"),
			new("tenant.complaints.read_own", "Xem khiếu nại của bản thân"),
			new("tenant.complaints.assign", "Phân công xử lý khiếu nại"),
			new("tenant.complaints.update", "Cập nhật quá trình xử lý khiếu nại"),
			new("tenant.complaints.resolve", "Hoàn tất xử lý khiếu nại"),
			new("tenant.documents.read", "Xem tài liệu tenant"),
			new("tenant.documents.create", "Tạo tài liệu tenant"),
			new("tenant.documents.update", "Cập nhật tài liệu tenant"),
			new("tenant.documents.publish", "Công bố tài liệu tenant"),
			new("tenant.documents.archive", "Lưu trữ tài liệu tenant"),
			new("tenant.reports.financial", "Xem báo cáo tài chính"),
			new("tenant.reports.profit", "Xem báo cáo lợi nhuận"),
			new("tenant.reports.staff", "Xem báo cáo nhân sự"),
			new("tenant.reports.population", "Xem báo cáo cư dân"),
			new("tenant.reports.infrastructure", "Xem báo cáo cơ sở vật chất"),
			new("tenant.profile.read_own", "Xem hồ sơ của bản thân"),
			new("tenant.ai.use", "Sử dụng trợ lý AI trong phạm vi tenant")
		};

		private static readonly TenantRoleSeed[] TenantRoleSeeds =
		{
			new(
				TenantAdminRoleCode,
				"Chủ chung cư",
				100,
				new[]
				{
					"tenant.settings.read", "tenant.settings.update",
					"tenant.roles.read", "tenant.roles.create", "tenant.roles.update",
					"tenant.roles.delete", "tenant.roles.assign",
					"tenant.staff.read", "tenant.staff.update", "tenant.staff.invite",
					"tenant.staff.revoke_invitation", "tenant.staff.assign_role",
					"tenant.apartments.read", "tenant.residents.read", "tenant.contracts.read",
					"tenant.services.read", "tenant.billing_periods.read", "tenant.invoices.read",
					"tenant.payments.read",
					"tenant.payroll_batches.read", "tenant.payroll_batches.approve",
					"tenant.payroll_batches.reject",
					"tenant.refunds.read", "tenant.refunds.approve", "tenant.refunds.reject",
					"tenant.complaints.read",
					"tenant.documents.read", "tenant.documents.create", "tenant.documents.update",
					"tenant.documents.publish", "tenant.documents.archive",
					"tenant.reports.financial", "tenant.reports.profit", "tenant.reports.staff",
					"tenant.reports.population", "tenant.reports.infrastructure",
					"tenant.profile.read_own", "tenant.ai.use"
				}),

			new(
				ManagerRoleCode,
				"Quản lý",
				80,
				new[]
				{
					"tenant.settings.read", "tenant.roles.read",
					"tenant.staff.read", "tenant.staff.create", "tenant.staff.update",
					"tenant.staff.invite", "tenant.staff.revoke_invitation", "tenant.staff.assign_role",
					"tenant.apartments.read", "tenant.apartments.create", "tenant.apartments.update",
					"tenant.residents.read", "tenant.residents.create", "tenant.residents.update",
					"tenant.residents.invite",
					"tenant.contracts.read", "tenant.contracts.create", "tenant.contracts.update",
					"tenant.contracts.import", "tenant.contracts.renew",
					"tenant.services.read", "tenant.services.create", "tenant.services.update",
					"tenant.services.change_status",
					"tenant.billing_periods.read", "tenant.billing_periods.create",
					"tenant.billing_periods.update", "tenant.billing_periods.issue",
					"tenant.billing_periods.close",
					"tenant.invoices.read", "tenant.invoices.create", "tenant.invoices.update",
					"tenant.invoices.issue", "tenant.invoices.cancel", "tenant.invoices.send",
					"tenant.payments.read", "tenant.payments.reconcile",
					"tenant.payroll_batches.read", "tenant.payroll_batches.create",
					"tenant.payroll_batches.update", "tenant.payroll_batches.submit",
					"tenant.payroll_batches.issue",
					"tenant.refunds.read", "tenant.refunds.create", "tenant.refunds.update",
					"tenant.refunds.submit", "tenant.refunds.issue",
					"tenant.complaints.read", "tenant.complaints.assign",
					"tenant.complaints.update", "tenant.complaints.resolve",
					"tenant.documents.read",
					"tenant.reports.financial", "tenant.reports.staff",
					"tenant.reports.population", "tenant.reports.infrastructure",
					"tenant.profile.read_own", "tenant.ai.use"
				}),

			new(
				StaffRoleCode,
				"Nhân viên",
				50,
				new[]
				{
					"tenant.apartments.read", "tenant.apartments.update",
					"tenant.residents.read", "tenant.residents.create", "tenant.residents.update",
					"tenant.residents.invite",
					"tenant.contracts.read", "tenant.contracts.create", "tenant.contracts.update",
					"tenant.contracts.import", "tenant.contracts.renew",
					"tenant.services.read", "tenant.billing_periods.read",
					"tenant.invoices.read", "tenant.invoices.update", "tenant.invoices.send",
					"tenant.payments.read", "tenant.payments.reconcile",
					"tenant.payroll_entries.read_own",
					"tenant.complaints.read", "tenant.complaints.assign",
					"tenant.complaints.update", "tenant.complaints.resolve",
					"tenant.documents.read", "tenant.profile.read_own", "tenant.ai.use"
				}),

			new(
				ResidentRoleCode,
				"Cư dân",
				10,
				new[]
				{
					"tenant.apartments.read_own", "tenant.contracts.read_own",
					"tenant.services.read", "tenant.invoices.read_own", "tenant.payments.pay_own",
					"tenant.complaints.create", "tenant.complaints.read_own",
					"tenant.documents.read", "tenant.profile.read_own", "tenant.ai.use"
				})
		};
	}
}
