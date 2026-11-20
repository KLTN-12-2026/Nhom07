using System.ComponentModel.DataAnnotations;

namespace EzCondo.BackEndServer.Contracts.SubscriptionRenewals
{
	public sealed class GetPendingRenewalsRequest
	{
		[Range(
			1,
			int.MaxValue,
			ErrorMessage ="PageNumber phải lớn hơn hoặc bằng 1.")]
		public int PageNumber { get; set; } = 1;

		[Range(
			1,
			100,
			ErrorMessage ="PageSize phải từ 1 đến 100.")]
		public int PageSize { get; set; } = 20;
	}
}
