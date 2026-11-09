using EzCondo.BackEndServer.Data.Entities.SchemaPlatform;

namespace EzCondo.BackEndServer.Data.Entities.SchemaTenant
{
	public class InvoiceDetail
	{
		public Guid Id { get; set; }
		public int Quantity { get; set; }
		public decimal UnitPrice { get; set; }
		public decimal Amount { get; set; }
		public string? Description { get; set; }

		public Guid InvoiceId { get; set; }
		public Invoice Invoice { get; set; } = null!;
		public Guid ServiceId { get; set; }
		public Service TenantService { get; set; } = null!;
		public Guid TenantId { get; set; }
		public Tenant Tenant { get; set; } = null!;
	}
}
