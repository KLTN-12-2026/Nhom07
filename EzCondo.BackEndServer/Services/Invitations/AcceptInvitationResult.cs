using EzCondo.BackEndServer.Contracts.Invitations;

namespace EzCondo.BackEndServer.Services.Invitations
{
	public sealed record AcceptInvitationResult(
		AcceptInvitationResponse? Value,
		AcceptInvitationFailure Failure,
		IReadOnlyCollection<string> Errors)
	{
		public bool IsSuccess =>
			Failure == AcceptInvitationFailure.None &&
			Value is not null;

		public static AcceptInvitationResult Success(
			AcceptInvitationResponse value)
		{
			ArgumentNullException.ThrowIfNull(value);

			return new(
				value,
				AcceptInvitationFailure.None,
				Array.Empty<string>());
		}

		public static AcceptInvitationResult Failed(
			AcceptInvitationFailure failure)
		{
			if(failure == AcceptInvitationFailure.None)
			{
				throw new ArgumentException(
					"Kết quả thất bại phải có nguyên nhân.",
					nameof(failure));
			}

			return new(
				null,
				failure,
				Array.Empty<string>());
		}

		public static AcceptInvitationResult PasswordRejected(
			IEnumerable<string> errors)
		{
			ArgumentNullException.ThrowIfNull(errors);

			var errorList = errors
				.Where(x => !string.IsNullOrWhiteSpace(x))
				.Distinct(StringComparer.Ordinal)
				.ToArray();

			if(errorList.Length == 0)
			{
				errorList =
					[
						"Mật khẩu không hợp lệ."
					];
			}

			return new(
				null,
				AcceptInvitationFailure.PasswordRejected,
				errorList);
		}
	}
}
