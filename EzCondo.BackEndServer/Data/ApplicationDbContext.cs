using EzCondo.BackEndServer.Data.Entities;
using EzCondo.BackEndServer.Data.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using System;
using IdentityUser = Microsoft.AspNetCore.Identity.IdentityUser;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;

namespace EzCondo.BackEndServer.Data
{
	//public sealed class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : IdentityUserContext<IdentityUser, Guid>(options)
	//{

	public sealed class ApplicationDbContext : DbContext
	{
		public ApplicationDbContext(DbContextOptions options) : base(options)
		{
		}

		public DbSet<IdentityUser> IdentityUsers => Set<IdentityUser>();
		public DbSet<ApartmentContract> ApartmentContracts => Set<ApartmentContract>();
		public DbSet<ApartmentResident> ApartmentResidents => Set<ApartmentResident>();
		public DbSet<IncidentAffectedTenant> IncidentAffectedTenants => Set<IncidentAffectedTenant>();
		public DbSet<IncidentUpdate> IncidentUpdates => Set<IncidentUpdate>();
		public DbSet<KnowledgeDocument> KnowledgeDocuments => Set<KnowledgeDocument>();
		public DbSet<PlatformInvitation> PlatformInvitations => Set<PlatformInvitation>();
		public DbSet<PlatformPaymentProviderConfig> PlatformPaymentProviderConfigs => Set<PlatformPaymentProviderConfig>();
		public DbSet<PlatformPermission> PlatformPermissions => Set<PlatformPermission>();
		public DbSet<PlatformRole> PlatformRoles => Set<PlatformRole>();
		public DbSet<PlatformRolePermission> PlatformRolePermissions => Set<PlatformRolePermission>();
		public DbSet<PlatformUserRole> PlatformUserRoles => Set<PlatformUserRole>();
		public DbSet<ResidentComplaint> ResidentComplaints => Set<ResidentComplaint>();
		public DbSet<Subscription> Subscriptions => Set<Subscription>();
		public DbSet<SubscriptionEvent> SubscriptionEvents => Set<SubscriptionEvent>();
		public DbSet<SubscriptionInvoice> SubscriptionInvoices => Set<SubscriptionInvoice>();
		public DbSet<SubscriptionPaymentOrder> SubscriptionPaymentOrders => Set<SubscriptionPaymentOrder>();
		public DbSet<SubscriptionPaymentTransaction> SubscriptionPaymentTransactions => Set<SubscriptionPaymentTransaction>();
		public DbSet<SubscriptionPaymentWebhookEvent> SubscriptionPaymentWebhookEvents => Set<SubscriptionPaymentWebhookEvent>();
		public DbSet<SubscriptionPlan> SubscriptionPlans => Set<SubscriptionPlan>();
		public DbSet<SupportTicket> SupportTickets => Set<SupportTicket>();
		public DbSet<SystemIncident> SystemIncidents => Set<SystemIncident>();
		public DbSet<Tenant> Tenants => Set<Tenant>();
		public DbSet<TenantAiChatMessage> TenantAiChatMessages => Set<TenantAiChatMessage>();
		public DbSet<TenantAiChatSession> TenantAiChatSessions => Set<TenantAiChatSession>();
		public DbSet<TenantApartment> TenantApartments => Set<TenantApartment>();
		public DbSet<TenantBillingPeriod> TenantBillingPeriods => Set<TenantBillingPeriod>();
		public DbSet<TenantBuilding> TenantBuildings => Set<TenantBuilding>();
		public DbSet<TenantInvitation> TenantInvitations => Set<TenantInvitation>();
		public DbSet<TenantInvoice> TenantInvoices => Set<TenantInvoice>();
		public DbSet<TenantInvoiceLine> TenantInvoiceLines => Set<TenantInvoiceLine>();
		public DbSet<TenantPaymentOrder> TenantPaymentOrders => Set<TenantPaymentOrder>();
		public DbSet<TenantPaymentProviderConfig> TenantPaymentProviderConfigs => Set<TenantPaymentProviderConfig>();
		public DbSet<TenantPaymentTransaction> TenantPaymentTransactions => Set<TenantPaymentTransaction>();
		public DbSet<TenantPaymentWebhookEvent> TenantPaymentWebhookEvents => Set<TenantPaymentWebhookEvent>();
		public DbSet<TenantPayroll> TenantPayrolls => Set<TenantPayroll>();
		public DbSet<TenantPayrollDetail> TenantPayrollDetails => Set<TenantPayrollDetail>();
		public DbSet<TenantPermission> TenantPermissions => Set<TenantPermission>();
		public DbSet<TenantResident> TenantResidents => Set<TenantResident>();
		public DbSet<TenantRole> TenantRoles => Set<TenantRole>();
		public DbSet<TenantRolePermission> TenantRolePermissions => Set<TenantRolePermission>();
		public DbSet<TenantService> TenantServices => Set<TenantService>();
		public DbSet<TenantStaffProfile> TenantStaffProfiles => Set<TenantStaffProfile>();
		public DbSet<TenantStaffTimeSheet> TenantStaffTimeSheets => Set<TenantStaffTimeSheet>();
		public DbSet<TenantStatusHistory> TenantStatusHistories => Set<TenantStatusHistory>();
		public DbSet<TenantUserRole> TenantUserRoles => Set<TenantUserRole>();

		protected override void OnModelCreating(ModelBuilder b)
		{
			base.OnModelCreating(b);

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

			b.HasPostgresEnum<AiRoleEnum>(schema: "tenant", name: "ai_role_enum");
			b.HasPostgresEnum<ApartmentStatusEnum>(schema: "tenant", name: "apartment_status_enum");
			b.HasPostgresEnum<BillingPeriodStatus>(schema: "tenant", name: "billing_period_status");
			b.HasPostgresEnum<DocumentStatusEnum>(schema: "tenant", name: "document_status_enum");
			b.HasPostgresEnum<DocumentTypeEnum>(schema: "tenant", name: "document_type_enum");
			b.HasPostgresEnum<PaymentOrderStatus>(schema: "tenant", name: "payment_order_status");
			b.HasPostgresEnum<PaymentTransactionStatus>(schema: "tenant", name: "payment_transaction_status");
			b.HasPostgresEnum<PayrollStatusEnum>(schema: "tenant", name: "payroll_status_enum");
			b.HasPostgresEnum<ResidentComplaintStatus>(schema: "tenant", name: "resident_complaint_status");
			b.HasPostgresEnum<ServiceBillingType>(schema: "tenant", name: "service_billing_type");
			b.HasPostgresEnum<SubscriptionInvoiceStatus>(schema: "platform", name: "subscription_invoice_status");
			b.HasPostgresEnum<SubscriptionStatusEnum>(schema: "platform", name: "subscription_status_enum");
			b.HasPostgresEnum<SupportTicketStatus>(schema: "platform", name: "support_ticket_status");
			b.HasPostgresEnum<SystemIncidentStatus>(schema: "platform", name: "system_incident_status");
			b.HasPostgresEnum<TenantContractStatus>(schema: "tenant", name: "tenant_contract_status");
			b.HasPostgresEnum<TenantInvoiceStatus>(schema: "tenant", name: "tenant_invoice_status");
			b.HasPostgresEnum<TenantResidentStatus>(schema: "tenant", name: "tenant_resident_status");
			b.HasPostgresEnum<TimeSheetStatus>(schema: "tenant", name: "time_sheet_status");
			b.HasPostgresEnum<UserStatusEnum>(schema: "identity", name: "user_status_enum");
			b.HasPostgresEnum<TenantStatusEnum>(schema: "platform", name: "tenant_status_enum");

			b.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
		}
	}
}
