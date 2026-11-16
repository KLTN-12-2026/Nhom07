using EzCondo.BackEndServer.Contracts.Authentication.Platform;

namespace EzCondo.BackEndServer.Services.Authentication
{
	public sealed record PlatformLoginResult(
		PlatformLoginResponse? Value,
		LoginFailure Failure)
	{
		public bool IsSuccess =>
			Failure == LoginFailure.None &&
			Value is not null;

		public static PlatformLoginResult Success(
			PlatformLoginResponse value)
		{
			ArgumentNullException.ThrowIfNull(value);

			return new(value, LoginFailure.None);
		}

		public static PlatformLoginResult Failed(LoginFailure failure)
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
