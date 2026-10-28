using EzCondo.BackEndServer.Data.Enums;

namespace EzCondo.BackEndServer.Data.Entities
{
	public class TenantPayroll : ISoftDeletableEntity, IAuditableEntity
	{
		public Guid Id { get; set; }
		public string Title { get; set; }
		public Decimal TotalAmount { get; set; }
		public PayrollStatusEnum Status { get; set; }
		public bool IsDeleted { get; set; }
		public DateTimeOffset? DeletedAt { get; set; }
		public DateTimeOffset CreatedAt { get; set; }
		public DateTimeOffset? UpdatedAt { get; set; }
		public DateTimeOffset? SubmittedAt { get; set; }
		public DateTimeOffset? ApprovedAt { get; set; }
		public DateTimeOffset? RejectedAt { get; set; }
		public DateTimeOffset? IssuedAt { get; set; }

		public Guid TenantId { get; set; }
		public Tenant Tenant { get; set; }
		public Guid CreatedBy { get; set; }
		public IdentityUser? Manager { get; set; }
		public Guid? SubmittedBy { get; set; }
		public IdentityUser? ManagerSubmitted { get; set; }
		public Guid? ApprovedBy { get; set; }
		public IdentityUser? ApprovedByUser { get; set; }
		public Guid? RejectedBy { get; set; }
		public IdentityUser? RejectedByUser { get; set; }
		public Guid? IssuedBy { get; set; }
		public IdentityUser? IssuedByUser { get; set; }
		public Guid ReceiveBy { get; set; }

		public ICollection<TenantPayrollDetail> TenantPayrollDetails { get; set; } = new List<TenantPayrollDetail>();
	}
}
