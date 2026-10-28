using EzCondo.BackEndServer.Data.Enums;

namespace EzCondo.BackEndServer.Data.Entities
{
	public class TenantStaffProfile : ISoftDeletableEntity, IAuditableEntity
	{
		public Guid Id { get; set; }
		public DateTimeOffset? DateOfBirth { get; set; }
		public string BankName { get; set; }
		public string BankAccountNumber { get; set; }
		public bool IsDeleted { get; set; }
		public DateTimeOffset? DeletedAt { get; set; }
		public DateTimeOffset CreatedAt { get; set; }
		public DateTimeOffset? UpdatedAt { get; set; }

		public Guid TenantId { get; set; }
		public Tenant Tenant { get; set; }
		public Guid UserId { get; set; }
		public IdentityUser User { get; set; }

		public ICollection<TenantStaffTimeSheet> TenantStaffTimeSheets { get; set; } = new List<TenantStaffTimeSheet>();
		public ICollection<TenantPayrollDetail> TenantPayrollDetails { get; set; } = new List<TenantPayrollDetail>();

	}
}
