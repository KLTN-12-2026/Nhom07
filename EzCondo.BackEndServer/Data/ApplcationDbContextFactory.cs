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

			dataSourceBuilder.MapEnum<SubscriptionInvoiceStatus>("platform.subscription_invoice_status");
			dataSourceBuilder.MapEnum<SubscriptionPaymentAttemptStatus>("platform.subscription_payment_attempt_status");
			dataSourceBuilder.MapEnum<SubscriptionStatusEnum>("platform.subscription_status_enum");
			dataSourceBuilder.MapEnum<TenantStatusEnum>("platform.tenant_status_enum");
			dataSourceBuilder.MapEnum<RoleScopeEnum>("security.role_scope_enum");
			dataSourceBuilder.MapEnum<UserStatusEnum>("identity.user_status_enum");
			dataSourceBuilder.MapEnum<InvoicePaymentAttemptStatus>("tenant.invoice_payment_attempt_status");
			dataSourceBuilder.MapEnum<InvitationStatusEnum>("tenant.invitation_status_enum");
			dataSourceBuilder.MapEnum<PayrollDisbursementItemStatus>("tenant.payroll_disbursement_item_status");
			dataSourceBuilder.MapEnum<PayrollDisbursementStatus>("tenant.payroll_disbursement_status");
			dataSourceBuilder.MapEnum<PayrollStatusEnum>("tenant.payroll_status_enum");
			dataSourceBuilder.MapEnum<TenantInvoiceStatus>("tenant.tenant_invoice_status");
			dataSourceBuilder.MapEnum<PaymentRequestStatus>("payment.payment_request_status");
			dataSourceBuilder.MapEnum<PaymentTransactionStatus>("payment.payment_transaction_status");
			dataSourceBuilder.MapEnum<PayoutBatchStatus>("payment.payout_batch_status");
			dataSourceBuilder.MapEnum<PayoutItemStatusEnum>("payment.payout_item_status_enum");
			dataSourceBuilder.MapEnum<ProcessingStatusEnum>("payment.processing_status_enum");
			dataSourceBuilder.MapEnum<ProviderOwnerScope>("payment.provider_owner_scope");

			var dataSource = dataSourceBuilder.Build();

			var builder = new DbContextOptionsBuilder<ApplicationDbContext>();
			builder.UseNpgsql(dataSource);

			return new ApplicationDbContext(builder.Options);
		}
	}
}
