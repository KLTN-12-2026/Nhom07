using System.ComponentModel.DataAnnotations;

namespace EzCondo.BackEndServer.Contracts.Invitations
{
	public sealed class AcceptInvitationRequest
	{
		[Required]
		[StringLength(
			512,
			MinimumLength =20,
			ErrorMessage = "Token lời mời không hợp lệ.")]
		public string Token { get; set; } = string.Empty;

		[Required]
		[StringLength(
			128,
			MinimumLength =12,
			ErrorMessage = "mật khẩu phải có từ 12 đến 128 ký tự.")]
		[DataType(DataType.Password)]
		public string Password { get; set; } = string.Empty;

		[Required]
		[Compare(
			nameof(Password),
			ErrorMessage = "Mật khẩu xác nhận không chính xác.")]
		[DataType(DataType.Password)]
		public string ConfirmPassword { get; set; } = string.Empty;
	}
}
