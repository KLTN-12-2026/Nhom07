namespace EzCondo.BackEndServer.Data.Entities.SchemaPlatform
{
	public sealed class Subscription //Bảng trung gian giữa Tenant và SubscriptionPlan
	{
		public Guid Id { get; set; }
		public Guid TenantId { get; set; }
		public Guid PlanId { get; set; }
		public int BillingCycleMonth { get; set; }
		public DateTimeOffset RegisteredAt { get; set; }

		public Tenant Tenant { get; set; }
		public SubscriptionPlan Plan { get; set; }
	}
}