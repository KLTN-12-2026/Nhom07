using EzCondo.BackEndServer.Data.Enums;
using NpgsqlTypes;

namespace EzCondo.BackEndServer.Data.Entities
{
	public class SubscriptionInvoice
	{
		public Guid Id { get; set; }
		public string InvoiceNo { get; set; }
		public NpgsqlRange<DateOnly> Period { get; set; }
		public Decimal Amount { get; set; }
		public DateTimeOffset? DueAt { get; set; }
		public Decimal PaidAmount { get; set; }
		public SubscriptionInvoiceStatus Status { get; set; }
		public DateTimeOffset CreatedAt { get; set; }
		public DateTimeOffset? IssuedAt { get; set; }

		public Guid SubscriptionId { get; set; }
		public Subscription Subscription { get; set; }
	}
}
