namespace EzCondo.BackEndServer.Services.Authentication
{
	public sealed record LogoutResult(LogoutFailure Failure)
	{
		public bool IsSuccess =>
			Failure == LogoutFailure.None;

		public static LogoutResult Success()
		{
			return new(LogoutFailure.None);
		}

		public static LogoutResult Failed(LogoutFailure failure)
		{
			if(failure == LogoutFailure.None)
			{
				throw new ArgumentException(
					"Kết quả thất bại phải có nguyên nhân.",
					nameof(failure));
			}

			return new(failure);
		}
	}
}
