using System.ComponentModel.DataAnnotations;
using System.Text.Json;

namespace EzCondo.BackEndServer.Contracts.SubscriptionPlans
{
	public sealed class CreateSubscriptionPlanRequest : IValidatableObject
	{
		[Required]
		[StringLength(50)]
		public string Name { get; set; } = string.Empty;
		public string? Description { get; set; }
		public int BillingCycleMonths { get; set; }
		[Range(
			typeof(decimal),
			"0",
			"9999999999999999.99",
			ErrorMessage = "Giá gói phải nằm trong phạm vi hợp lệ")]
		public decimal Price { get; set; }
		public JsonElement? FeatureLimits { get; set; }
		public bool IsAvailable { get; set; } = true;

		public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
		{
			if(BillingCycleMonths is not (6 or 12 or 24))
			{
				yield return new ValidationResult(
					"Chu kỳ thanh toán chỉ được nhận 6, 12, hoặc 24 tháng.",
					new[] { nameof(BillingCycleMonths) });
			}

			if(decimal.Round(Price, 2) != Price)
			{
				yield return new ValidationResult(
					"Giá gói chỉ được có tối đa 2 chữ số thập phân.",
					new[] { nameof(Price) });
			}

			if(FeatureLimits is JsonElement featureLimits &&
					featureLimits.ValueKind != JsonValueKind.Object){
				yield return new ValidationResult(
					"FeatureLimits phải là một Json object.",
					new[] { nameof(FeatureLimits) });
			}
		}
	}
}
