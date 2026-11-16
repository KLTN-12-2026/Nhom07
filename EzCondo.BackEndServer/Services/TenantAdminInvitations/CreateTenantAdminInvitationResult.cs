using EzCondo.BackEndServer.Contracts.TenantAdminInvitations;

namespace EzCondo.BackEndServer.Services.TenantAdminInvitations
{
	public sealed record CreateTenantAdminInvitationResult(
		TenantAdminInvitationResponse? Value,
		CreateTenantAdminInvitationFailure Failure)
	{
		public bool IsSuccess =>
			Failure == CreateTenantAdminInvitationFailure.None &&
			Value is not null;

		public static CreateTenantAdminInvitationResult Success(
			TenantAdminInvitationResponse value)
		{
			ArgumentNullException.ThrowIfNull(value);

			return new(
				value,
				CreateTenantAdminInvitationFailure.None);
		}

		public static CreateTenantAdminInvitationResult Failed(
			CreateTenantAdminInvitationFailure failure)
		{
			if(failure == CreateTenantAdminInvitationFailure.None)
			{
				throw new ArgumentException(
					"Kết quả thất bại phải có nguyên nhân.",
					nameof(failure));
			}

			return new(null, failure);
		}
	}
}
