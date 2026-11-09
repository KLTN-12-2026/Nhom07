using EzCondo.BackEndServer.Data.Enums;
using System.ComponentModel.DataAnnotations;

namespace EzCondo.BackEndServer.Contracts.Tenants
{
	public sealed class CreateTenantRequest : IValidatableObject
	{
		[Required]
		[StringLength(
			255,
			MinimumLength =2,
			ErrorMessage ="Tên pháp lý phải từ 2 đến 255 ký tự.")]
		public string LegalName { get; set; } = string.Empty;
		[Required]
		[StringLength(
			30,
			MinimumLength =3,
			ErrorMessage ="mã số thuế phải có từ 3 đến 30 ký tự.")]
		public string TaxCode { get; set; } = string.Empty;
		[Required]
		[MinLength(
			5,
			ErrorMessage ="Địa chỉ chung cư phải có ít nhất 5 ký tự.")]
		public string Address { get; set; } = string.Empty;
		public Guid SubscriptionPlanId { get; set; }
		public DateTimeOffset? TrialEndsAt { get; set; }

		public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
		{
			if(SubscriptionPlanId == Guid.Empty)
			{
				yield return new ValidationResult(
					"Phải chọn gói dịch vụ cho tenant.",
					new[] { nameof(SubscriptionPlanId) });
			}
		}
	}
}
