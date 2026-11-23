namespace EzCondo.BackEndServer.Infrastructure.Subscriptions.Options
{
	public sealed class SubscriptionLifecycleOptions
	{
		public const string SectionName = "SubscriptionLifecycle";
		public bool Enabled { get; set; }
		public int CheckIntervalMinutes { get; set; } = 60;
		public int ExpiryNoticeLeadHours { get; set; } = 24;
		public int RetryAfterMinutes { get; set; } = 15;
	}
}
