namespace EzCondo.BackEndServer.Data.Entities
{
	public class TenantInvoiceLine
	{
		public Guid Id { get; set; }
		
		public string Description { get; set; }
		public Decimal Quantity { get; set; }
		public string Unit { get; set; }
		public Decimal UnitPrice { get; set; }
		public Decimal LineAmount { get; set; }
		public DateTimeOffset CreatedAt { get; set; }

		public Guid InvoiceId { get; set; }
		public TenantInvoice TenantInvoice { get; set; }
		public Guid ServiceId { get; set; }
		public TenantService TenantService { get; set; }
		public Guid TenantId { get; set; }
		public Tenant Tenant { get; set; }
	}
}
