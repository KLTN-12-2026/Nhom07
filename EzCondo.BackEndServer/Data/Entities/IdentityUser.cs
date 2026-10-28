using EzCondo.BackEndServer.Data.Enums;
using Microsoft.AspNetCore.Identity;

namespace EzCondo.BackEndServer.Data.Entities
{
	public class IdentityUser : IdentityUser<Guid>, IAuditableEntity
	{
		public string FullName { get; set; } = string.Empty;
		public string? AvatarUrl { get; set; }
		public UserStatusEnum Status { get; set; }
		public DateTimeOffset? LastLogInAt { get; set; }
		public DateTimeOffset CreatedAt { get; set; }
		public DateTimeOffset? UpdatedAt { get; set; }
		public string CitizenId { get; set; } //cccd

		public Guid? TenantId { get; set; }
		public Tenant? Tenant { get; set; }

		public ICollection<IncidentUpdate> IncidentUpdates { get; set; } = new List<IncidentUpdate>();
		public ICollection<KnowledgeDocument> KnowledgeDocuments { get; set; } = new List<KnowledgeDocument>();
		public ICollection<TenantPaymentOrder> PaymentOrders { get; set; } = new List<TenantPaymentOrder>();
		public ICollection<PlatformInvitation> PlatformInvitations { get; set; } = new List<PlatformInvitation>();
		public ICollection<PlatformUserRole> PlatformUserRoles { get; set; } = new List<PlatformUserRole>();
		public ICollection<ResidentComplaint> CreatedByUser { get; set; } = new List<ResidentComplaint>();
		public ICollection<ResidentComplaint> ReceivedByUser { get; set; } = new List<ResidentComplaint>();
		public ICollection<SupportTicket> RequesterUser { get; set; } = new List<SupportTicket>();
		public ICollection<SupportTicket> ReceiverUser { get; set; } = new List<SupportTicket>();
		public ICollection<TenantAiChatSession> TenantAiChatSessions { get; set; } = new List<TenantAiChatSession>();
		public ICollection<TenantBillingPeriod> TenantBillingPeriods { get; set; } = new List<TenantBillingPeriod>();
		public ICollection<TenantInvitation> TenantInvitations { get; set; } = new List<TenantInvitation>();
		public ICollection<TenantInvoice> TenantInvoices { get; set; } = new List<TenantInvoice>();
		public ICollection<TenantPayroll> TenantPayrolls { get; set; } = new List<TenantPayroll>();
		public ICollection<TenantResident> TenantResidents { get; set; } = new List<TenantResident>();
		public ICollection<SystemIncident> SystemIncidents { get; set; } = new List<SystemIncident>();
		public ICollection<TenantUserRole> TenantUserRoles { get; set; } = new List<TenantUserRole>();
	}
}