using EzCondo.BackEndServer.Data.Enums;

namespace EzCondo.BackEndServer.Data.Entities
{
	public class Tenant : IAuditableEntity
	{
		public Guid Id { get; set; }
		public string Code { get; set; }
		public string Slug { get; set; }
		public string LegalName { get; set; }
		public string? TaxCode { get; set; }
		public string Contact { get; set; }
		public TenantStatusEnum Status { get; set; }
		public DateTimeOffset? TrialStartedAt { get; set; }
		public DateTimeOffset? TrialEndsAt { get; set; }
		public DateTimeOffset? ActivatedAt { get; set; }
		public DateTimeOffset? SuspendedAt { get; set; }
		public string? SuspensionReason { get; set; }
		public DateTimeOffset CreatedAt { get; set; }
		public DateTimeOffset? UpdatedAt { get; set; }

		public ICollection<ApartmentContract> ApartmentContracts { get; set; } = new List<ApartmentContract>();
		public ICollection<ApartmentResident> ApartmentResidents { get; set; } = new List<ApartmentResident>();
		public ICollection<IncidentAffectedTenant> IncidentAffectedTenants { get; set; } = new List<IncidentAffectedTenant>();
		public ICollection<KnowledgeDocument> KnowledgeDocuments { get; set; } = new List<KnowledgeDocument>();
		public ICollection<TenantPaymentOrder> PaymentOrders { get; set; } = new List<TenantPaymentOrder>();
		public ICollection<TenantPaymentProviderConfig> PaymentProviderConfigs { get; set; } = new List<TenantPaymentProviderConfig>();
		public ICollection<TenantPaymentTransaction> PaymentTransactions { get; set; } = new List<TenantPaymentTransaction>();
		public ICollection<ResidentComplaint> ResidentComplaints { get; set; } = new List<ResidentComplaint>();
		public ICollection<Subscription> Subscriptions { get; set; } = new List<Subscription>();
		public ICollection<SupportTicket> SupportTickets { get; set; } = new List<SupportTicket>();
		public ICollection<TenantAiChatSession> TenantAiChatSessions { get; set; } = new List<TenantAiChatSession>();
		public ICollection<TenantApartment> TenantApartments { get; set; } = new List<TenantApartment>();
		public ICollection<TenantBillingPeriod> TenantBillingPeriods { get; set; } = new List<TenantBillingPeriod>();
		public ICollection<TenantBuilding> TenantBuildings { get; set; } = new List<TenantBuilding>();
		public ICollection<TenantInvitation> TenantInvitations { get; set; } = new List<TenantInvitation>();
		public ICollection<TenantInvoice> TenantInvoices { get; set; } = new List<TenantInvoice>();
		public ICollection<TenantPayroll> TenantPayrolls { get; set; } = new List<TenantPayroll>();
		public ICollection<TenantPayrollDetail> TenantPayrollDetails { get; set; } = new List<TenantPayrollDetail>();
		public ICollection<TenantResident> TenantResidents { get; set; } = new List<TenantResident>();
		public ICollection<TenantRole> TenantRoles { get; set; } = new List<TenantRole>();
		public ICollection<TenantService> TenantServices { get; set; } = new List<TenantService>();
		public ICollection<TenantStatusHistory> TenantStatusHistories { get; set; } = new List<TenantStatusHistory>();
	}
}
