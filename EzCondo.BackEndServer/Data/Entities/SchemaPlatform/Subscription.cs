namespace EzCondo.BackEndServer.Data.Entities.SchemaPlatform
{
	public sealed class Subscription //Bảng trung gian giữa Tenant và SubscriptionPlan
	{
		public Guid Id { get; set; }
		public Guid TenantId { get; set; }
		public Guid PlanId { get; set; }
		public int BillingCycleMonthsSnapshot { get; set; }
		public EzCondo.BackEndServer.Data.Enums.SubscriptionStatusEnum Status { get; set; }
			= EzCondo.BackEndServer.Data.Enums.SubscriptionStatusEnum.Trial;
		public DateOnly StartsAt { get; set; }
		public DateOnly EndsAt { get; set; }
		public DateTimeOffset? CancelledAt { get; set; }

		public Tenant Tenant { get; set; } = null!;
		public SubscriptionPlan Plan { get; set; } = null!;

		public ICollection<SubscriptionInvoice> SubscriptionInvoices { get; set; } = new List<SubscriptionInvoice>();

	}
}
