using System.ComponentModel.DataAnnotations;

namespace EzCondo.BackEndServer.Contracts.Tenants
{
	public sealed class ScheduleTenantSuspensionRequest
		: IValidatableObject
	{
		public TenantSuspensionReasonCode ReasonCode { get; set; }
		[Required]
		[StringLength(
			1000,
			MinimumLength = 5,
			ErrorMessage = "Lý do phải có từ 5 đến 1000 ký tự.")]
		public string Reason { get; set; } = String.Empty;

		[Range(
			12,
			24,
			ErrorMessage ="Thời gian báo trước phải từ 12 đến 24 giờ.")]
		public int NoticeHours { get; set; } = 24;

		public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
		{
			if(!Enum.IsDefined(
				typeof(TenantSuspensionReasonCode),
				ReasonCode) || 
				ReasonCode == TenantSuspensionReasonCode.Unspecified)
			{
				yield return new ValidationResult(
					"Loại lý do tạm ngưng không hợp lệ.",
					new[] { nameof(ReasonCode) });
			}
		}
	}
}
