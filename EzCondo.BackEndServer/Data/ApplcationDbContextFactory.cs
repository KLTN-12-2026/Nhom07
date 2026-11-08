using EzCondo.BackEndServer.Data.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Npgsql;

namespace EzCondo.BackEndServer.Data
{
	public class ApplcationDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
	{
		public ApplicationDbContext CreateDbContext(string[] args)
		{
			IConfigurationRoot conf = new ConfigurationBuilder()
				.SetBasePath(Directory.GetCurrentDirectory())
				.AddJsonFile("appsettings.json", optional: true)
				.AddJsonFile("appsettings.Development.json", optional: true)
				.AddUserSecrets<ApplcationDbContextFactory>(optional: true)
				.AddEnvironmentVariables()
				.Build();

			var connectionString = conf.GetConnectionString("DefaultConnection")
				?? throw new InvalidOperationException(
					"Không tìm thấy ConnectionStrings:DefaultConnection. " +
					"Hãy cấu hình bằng .NET User Secrets hoặc biến môi trường.");

			var dataSourceBuilder = new NpgsqlDataSourceBuilder(connectionString);
			dataSourceBuilder.EnableDynamicJson();

			dataSourceBuilder.MapEnum<PaymentOrderStatus>("tenant.payment_order_status");
			dataSourceBuilder.MapEnum<PayrollStatusEnum>("tenant.payroll_status_enum");
			dataSourceBuilder.MapEnum<TenantInvoiceStatus>("tenant.tenant_invoice_status");
			dataSourceBuilder.MapEnum<UserStatusEnum>("identity.user_status_enum");
			dataSourceBuilder.MapEnum<RoleScopeEnum>("role_scope_enum");
			dataSourceBuilder.MapEnum<InvitationStatusEnum>("tenant.invitation_status_enum");
			dataSourceBuilder.MapEnum<TenantRoleCodeEnum>("tenant.tenant_role_code_enum");

			var dataSource = dataSourceBuilder.Build();

			var builder = new DbContextOptionsBuilder<ApplicationDbContext>();
			builder.UseNpgsql(dataSource);

			return new ApplicationDbContext(builder.Options);
		}
	}
}
