using EzCondo.BackEndServer.Contracts.Tenants;

namespace EzCondo.BackEndServer.Services.Tenants
{
	public sealed record RestoreTenantResult(
		TenantRestoreResponse? Value,
		RestoreTenantFailure Failure)
	{
		public bool IsSuccess =>
			Failure == RestoreTenantFailure.None &&
			Value is not null;

		public static RestoreTenantResult Success(
			TenantRestoreResponse value)
		{
			ArgumentNullException.ThrowIfNull(value);

			return new(value, RestoreTenantFailure.None);
		}

		public static RestoreTenantResult Failed(
			RestoreTenantFailure failure)
		{
			if(failure == RestoreTenantFailure.None)
			{
				throw new ArgumentException(
					"Kết quả thất bại phải có nguyên nhân.",
					nameof(failure));
			}

			return new(null, failure);
		}
	}
}
