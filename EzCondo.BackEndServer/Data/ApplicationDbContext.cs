using EzCondo.BackEndServer.Data.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using System;
using IdentityUser = Microsoft.AspNetCore.Identity.IdentityUser;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using EzCondo.BackEndServer.Data.Entities.SchemaPayment;
using EzCondo.BackEndServer.Data.Entities.SchemaIdentity;
using EzCondo.BackEndServer.Data.Entities.SchemaSecurity;
using EzCondo.BackEndServer.Data.Entities.SchemaPlatform;
using EzCondo.BackEndServer.Data.Entities.SchemaTenant;

namespace EzCondo.BackEndServer.Data
{
	public sealed class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : IdentityUserContext<User, Guid>(options)
	{
		public DbSet<Contract> Contracts => Set<Contract>(); 
		public DbSet<Rule> Rules => Set<Rule>();
		public DbSet<Complaint> Complaints => Set<Complaint>();
		public DbSet<Subscription> Subscriptions => Set<Subscription>();
		public DbSet<SubscriptionPlan> SubscriptionPlans => Set<SubscriptionPlan>();
		public DbSet<Tenant> Tenants => Set<Tenant>();
		public DbSet<Apartment> Apartments => Set<Apartment>();
		public DbSet<Invitation> Invitations => Set<Invitation>();
		public DbSet<Invoice> Invoices => Set<Invoice>();
		public DbSet<InvoiceDetail> InvoiceDetails => Set<InvoiceDetail>();
		public DbSet<PaymentOrder> PaymentOrders => Set<PaymentOrder>();
		public DbSet<PaymentProviderConfig> PaymentProviderConfigs => Set<PaymentProviderConfig>();
		public DbSet<PaymentTransaction> PaymentTransactions => Set<PaymentTransaction>();
		public DbSet<PaymentWebhookEvent> PaymentWebhookEvents => Set<PaymentWebhookEvent>();
		public DbSet<Payroll> Payrolls => Set<Payroll>();
		public DbSet<PayrollDetail> PayrollDetails => Set<PayrollDetail>();
		public DbSet<Permission> Permissions => Set<Permission>();
		public DbSet<Role> Roles => Set<Role>();
		public DbSet<RolePermission> RolePermissions => Set<RolePermission>();
		public DbSet<Service> Services => Set<Service>();
		public DbSet<StaffTimeSheet> StaffTimeSheets => Set<StaffTimeSheet>();
		public DbSet<User> Users=> Set<User>();

		protected override void OnModelCreating(ModelBuilder b)
		{
			base.OnModelCreating(b);

			b.Entity<User>().ToTable("user", "identity");
			b.Entity<IdentityUserClaim<Guid>>().ToTable("user_claims", "identity");
			b.Entity<IdentityUserLogin<Guid>>().ToTable("user_logins", "identity");
			b.Entity<IdentityUserToken<Guid>>().ToTable("user_tokens", "identity");

			b.HasSequence<long>(
				"payos_order_code_seq",
				schema: "tenant")
				.StartsAt(100000)
				.IncrementsBy(1);

			b.HasSequence<long>(
				"subscription_payment_order_code_seq",
				schema: "platform")
				.StartsAt(100000)
				.IncrementsBy(1);

			b.HasPostgresExtension("citext");
			b.HasPostgresExtension("pg_trgm");
			b.HasPostgresExtension("btree_gist");

			b.HasPostgresEnum<PaymentOrderStatus>(schema: "tenant", name: "payment_order_status");
			b.HasPostgresEnum<PayrollStatusEnum>(schema: "tenant", name: "payroll_status_enum");
			b.HasPostgresEnum<TenantInvoiceStatus>(schema: "tenant", name: "tenant_invoice_status");
			b.HasPostgresEnum<UserStatusEnum>(schema: "identity", name: "user_status_enum");
			b.HasPostgresEnum<RoleScopeEnum>(schema: "security", name: "role_scope_enum");
			b.HasPostgresEnum<InvitationStatusEnum>(schema: "tenant", name: "invitation_status_enum");
			b.HasPostgresEnum<TenantRoleCodeEnum>(schema: "tenant", name: "tenant_role_code_enum");

			b.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
		}
	}
}
