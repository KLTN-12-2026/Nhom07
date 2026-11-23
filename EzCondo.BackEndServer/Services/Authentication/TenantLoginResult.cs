using EzCondo.BackEndServer.Contracts.Authentication.Tenant;

namespace EzCondo.BackEndServer.Services.Authentication
{
	public sealed record TenantLoginResult(
		TenantLoginResponse? Value,
		LoginFailure Failure)
	{
		public bool IsSuccess =>
			Failure == LoginFailure.None &&
			Value is not null;

		public static TenantLoginResult Success(
			TenantLoginResponse value)
		{
			ArgumentNullException.ThrowIfNull(value);

			return new(value, LoginFailure.None);
		}

		public static TenantLoginResult Failed(LoginFailure failure)
		{
			if(failure == LoginFailure.None)
			{
				throw new ArgumentException(
					"Kết quả thất bại phải có nguyên nhân.",
					nameof(failure));
			}

			return new(null, failure);
		}
	}
}
