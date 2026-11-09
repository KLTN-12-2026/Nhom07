using EzCondo.BackEndServer.Data.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using System;
using IdentityUser = Microsoft.AspNetCore.Identity.IdentityUser;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using EzCondo.BackEndServer.Data.Entities.SchemaPayment;
using EzCondo.BackEndServer.Data.Entities;
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
		public DbSet<PaymentTransaction> PaymentTransactions => Set<PaymentTransaction>();
		public DbSet<PaymentWebhookEvent> PaymentWebhookEvents => Set<PaymentWebhookEvent>();
		public DbSet<Payroll> Payrolls => Set<Payroll>();
		public DbSet<PayrollDetail> PayrollDetails => Set<PayrollDetail>();
		public DbSet<Permission> Permissions => Set<Permission>();
		public DbSet<Role> Roles => Set<Role>();
		public DbSet<RolePermission> RolePermissions => Set<RolePermission>();
		public DbSet<UserRole> UserRoles => Set<UserRole>();
		public DbSet<Service> Services => Set<Service>();
		public DbSet<StaffTimeSheet> StaffTimeSheets => Set<StaffTimeSheet>();

		protected override void OnModelCreating(ModelBuilder b)
		{
			base.OnModelCreating(b);

			b.Entity<IdentityUserClaim<Guid>>().ToTable("user_claims", "identity");
			b.Entity<IdentityUserLogin<Guid>>().ToTable("user_logins", "identity");
			b.Entity<IdentityUserToken<Guid>>().ToTable("user_tokens", "identity");

			b.HasPostgresExtension("citext");

			b.HasPostgresEnum<SubscriptionInvoiceStatus>(schema: "platform", name: "subscription_invoice_status");
			b.HasPostgresEnum<SubscriptionPaymentAttemptStatus>(schema: "platform", name: "subscription_payment_attempt_status");
			b.HasPostgresEnum<SubscriptionStatusEnum>(schema: "platform", name: "subscription_status_enum");
			b.HasPostgresEnum<TenantStatusEnum>(schema: "platform", name: "tenant_status_enum");
			b.HasPostgresEnum<RoleScopeEnum>(schema: "security", name: "role_scope_enum");
			b.HasPostgresEnum<UserStatusEnum>(schema: "identity", name: "user_status_enum");
			b.HasPostgresEnum<InvoicePaymentAttemptStatus>(schema: "tenant", name: "invoice_payment_attempt_status");
			b.HasPostgresEnum<InvitationStatusEnum>(schema: "tenant", name: "invitation_status_enum");
			b.HasPostgresEnum<PayrollDisbursementItemStatus>(schema: "tenant", name: "payroll_disbursement_item_status");
			b.HasPostgresEnum<PayrollDisbursementStatus>(schema: "tenant", name: "payroll_disbursement_status");
			b.HasPostgresEnum<PayrollStatusEnum>(schema: "tenant", name: "payroll_status_enum");
			b.HasPostgresEnum<TenantInvoiceStatus>(schema: "tenant", name: "tenant_invoice_status");
			b.HasPostgresEnum<PaymentRequestStatus>(schema: "payment", name: "payment_request_status");
			b.HasPostgresEnum<PaymentTransactionStatus>(schema: "payment", name: "payment_transaction_status");
			b.HasPostgresEnum<PayoutBatchStatus>(schema: "payment", name: "payout_batch_status");
			b.HasPostgresEnum<PayoutItemStatusEnum>(schema: "payment", name: "payout_item_status_enum");
			b.HasPostgresEnum<ProcessingStatusEnum>(schema: "payment", name: "processing_status_enum");
			b.HasPostgresEnum<ProviderOwnerScope>(schema: "payment", name: "provider_owner_scope");

			b.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
		}

		public override int SaveChanges(bool acceptAllChangesOnSuccess)
		{
			ApplyAuditValues();
			return base.SaveChanges(acceptAllChangesOnSuccess);
		}

		public override Task<int> SaveChangesAsync(
			bool acceptAllChangesOnSuccess,
			CancellationToken cancellationToken = default)
		{
			ApplyAuditValues();
			return base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
		}

		private void ApplyAuditValues()
		{
			var now = DateTimeOffset.UtcNow;

			foreach(var entry in ChangeTracker.Entries<IAuditableEntity>())
			{
				if(entry.State == EntityState.Added && entry.Entity.CreatedAt == default)
				{
					entry.Entity.CreatedAt = now;
				}

				if(entry.State == EntityState.Modified)
				{
					entry.Entity.UpdatedAt = now;
				}
			}

			foreach(var entry in ChangeTracker.Entries<ISoftDeletableEntity>())
			{
				if(entry.State is EntityState.Added or EntityState.Modified)
				{
					entry.Entity.DeletedAt = entry.Entity.IsDeleted
						? entry.Entity.DeletedAt ?? now
						: null;
				}
			}
		}
	}
}
