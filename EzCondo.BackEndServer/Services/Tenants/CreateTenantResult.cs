using EzCondo.BackEndServer.Contracts.Tenants;

namespace EzCondo.BackEndServer.Services.Tenants
{
	public sealed record CreateTenantResult(//Các case thất bại của luồng tạo tenant
		TenantResponse? Value,
		CreateTenantFailure Failure)
	{
		public bool IsSuccess =>
			Failure == CreateTenantFailure.None &&
			Value is not null;

		public static CreateTenantResult Success(TenantResponse value)
		{
			ArgumentNullException.ThrowIfNull(value);

			return new CreateTenantResult(value, CreateTenantFailure.None);
		}

		public static CreateTenantResult Failed(CreateTenantFailure failure)
		{
			if(failure == CreateTenantFailure.None)
			{
				throw new ArgumentException(
					"Kết quả thất bại phải có nguyên nhân:",
					nameof(failure));
			}

			return new CreateTenantResult(null, failure);
		}
	}
}
