namespace EzCondo.BackEndServer.Services.SubscriptionRenewals
{
	public static class SubscriptionLifecycleClock
	{
		private static readonly TimeSpan VietnamOffset = TimeSpan.FromHours(7);

		public static DateTimeOffset ToExpiryInstant(
			DateOnly endsAt)
		{
			var local = endsAt.ToDateTime(
				TimeOnly.MinValue,
				DateTimeKind.Unspecified);

			return new DateTimeOffset(
				local,
				VietnamOffset).ToUniversalTime();
		}
	}
}
