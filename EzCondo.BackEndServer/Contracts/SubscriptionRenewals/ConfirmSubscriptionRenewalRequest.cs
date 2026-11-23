using System.ComponentModel.DataAnnotations;

namespace EzCondo.BackEndServer.Contracts.SubscriptionRenewals
{
	public sealed class ConfirmSubscriptionRenewalRequest
		: IValidatableObject
	{
		public Guid SelectedPlanId { get; set; }

		public IEnumerable<ValidationResult> Validate(
			ValidationContext validationContext)
		{
			if(SelectedPlanId == Guid.Empty)
			{
				yield return new ValidationResult(
					"Bạn phải chọn một gói dịch vụ.",
					new[] { nameof(SelectedPlanId) });
			}
		}
	}
}
