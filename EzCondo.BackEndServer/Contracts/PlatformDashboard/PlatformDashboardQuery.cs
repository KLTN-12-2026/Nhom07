using System.ComponentModel.DataAnnotations;

namespace EzCondo.BackEndServer.Contracts.PlatformDashboard
{
	public enum DashboardGranularity
	{
		Month,
		Quarter,
		Year
	}

	public sealed class PlatformDashboardQuery
	{
		public DateOnly? From { get; set; }
		public DateOnly? To { get; set; }
		public DashboardGranularity Granularity { get; set; } = DashboardGranularity.Month;
		[Range(1, 180)]
		public int ExpiringWithinDays { get; set; } = 60;
	}
}
