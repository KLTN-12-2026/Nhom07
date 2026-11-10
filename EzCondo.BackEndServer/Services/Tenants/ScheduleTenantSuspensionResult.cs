using EzCondo.BackEndServer.Contracts.Tenants;

namespace EzCondo.BackEndServer.Services.Tenants
{
	public sealed record ScheduleTenantSuspensionResult(
		TenantSuspensionResponse? Value,
		ScheduleTenantSuspensionFailure Failure)
	{
		public bool IsSuccess =>
			Failure == ScheduleTenantSuspensionFailure.None &&
			Value is not null;

		public static ScheduleTenantSuspensionResult Success(
			TenantSuspensionResponse value)
		{
			ArgumentNullException.ThrowIfNull(value);
			return new(value, ScheduleTenantSuspensionFailure.None);
		}

		public static ScheduleTenantSuspensionResult Failed(
			ScheduleTenantSuspensionFailure failure)
		{
			if(failure == ScheduleTenantSuspensionFailure.None)
			{
				throw new ArgumentException(
					"Kết quả thất bại phải có nguyên nhân.",
					nameof(failure));
			}

			return new(null, failure);
		}
	}
}
