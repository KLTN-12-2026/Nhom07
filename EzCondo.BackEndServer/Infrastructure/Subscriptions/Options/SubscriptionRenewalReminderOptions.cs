namespace EzCondo.BackEndServer.Infrastructure.Subscriptions.Options
{
	public sealed class SubscriptionRenewalReminderOptions
	{
		public const string SectionName = "SubscriptionRenewalReminder";
		public int CheckIntervalHours { get; set; } = 6;
		public int TrialReminderDays { get; set; } = 7;
		public int PaidReminderMonths { get; set; } = 1;
		public bool Enabled { get; set; } = false;
	}
}
