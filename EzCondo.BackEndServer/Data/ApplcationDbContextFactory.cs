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

			dataSourceBuilder.MapEnum<AiRoleEnum>("tenant.ai_role_enum");
			dataSourceBuilder.MapEnum<ApartmentStatusEnum>("tenant.apartment_status_enum");
			dataSourceBuilder.MapEnum<BillingPeriodStatus>("tenant.billing_period_status");
			dataSourceBuilder.MapEnum<DocumentStatusEnum>("tenant.document_status_enum");
			dataSourceBuilder.MapEnum<DocumentTypeEnum>("tenant.document_type_enum");
			dataSourceBuilder.MapEnum<PaymentOrderStatus>("tenant.payment_order_status");
			dataSourceBuilder.MapEnum<PaymentTransactionStatus>("tenant.payment_transaction_status");
			dataSourceBuilder.MapEnum<PayrollStatusEnum>("tenant.payroll_status_enum");
			dataSourceBuilder.MapEnum<ResidentComplaintStatus>("tenant.resident_complaint_status");
			dataSourceBuilder.MapEnum<ServiceBillingType>("tenant.service_billing_type");
			dataSourceBuilder.MapEnum<SubscriptionInvoiceStatus>("platform.subscription_invoice_status");
			dataSourceBuilder.MapEnum<SubscriptionStatusEnum>("platform.subscription_status_enum");
			dataSourceBuilder.MapEnum<SupportTicketStatus>("platform.support_ticket_status");
			dataSourceBuilder.MapEnum<SystemIncidentStatus>("platform.system_incident_status");
			dataSourceBuilder.MapEnum<TenantContractStatus>("tenant.tenant_contract_status");
			dataSourceBuilder.MapEnum<TenantInvoiceStatus>("tenant.tenant_invoice_status");
			dataSourceBuilder.MapEnum<TenantResidentStatus>("tenant.tenant_resident_status");
			dataSourceBuilder.MapEnum<TimeSheetStatus>("tenant.time_sheet_status");
			dataSourceBuilder.MapEnum<UserStatusEnum>("identity.user_status_enum");
			dataSourceBuilder.MapEnum<TenantStatusEnum>("platform.tenant_status_enum");

			var dataSource = dataSourceBuilder.Build();

			var builder = new DbContextOptionsBuilder<ApplicationDbContext>();
			builder.UseNpgsql(dataSource);

			return new ApplicationDbContext(builder.Options);
		}
	}
}
