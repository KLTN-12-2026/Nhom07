using EzCondo.BackEndServer.Data.Enums;
using NpgsqlTypes;

namespace EzCondo.BackEndServer.Data.Entities
{
	public class TenantBillingPeriod : IAuditableEntity
	{
		public Guid Id { get; set; }
		public string Name { get; set; }
		public NpgsqlRange<DateOnly> Period { get; set; }
		public DateTimeOffset DueAt { get; set; }
		public BillingPeriodStatus Status { get; set; }
		public DateTimeOffset ClosedAt { get; set; }
		public DateTimeOffset CreatedAt { get; set; }
		public DateTimeOffset? UpdatedAt { get; set; }

		public Guid TenantId { get; set; }
		public Tenant Tenant { get; set; }
		public Guid? CreatedBy { get; set; }
		public IdentityUser? User { get; set; }

		public ICollection<TenantInvoice> TenantInvoices { get; set; } = new List<TenantInvoice>();
	}
}
