using System.ComponentModel.DataAnnotations;

namespace EzCondo.BackEndServer.Contracts.TenantAdminInvitations
{
	public sealed class CreateTenantAdminInvitationRequest
	{
		[Required]
		[EmailAddress]
		[StringLength(256)]
		public string Email { get; set; } = string.Empty;

		[Required]
		[StringLength(
			200,
			MinimumLength = 2,
			ErrorMessage = "Họ tên phải có 2 đến 200 ký tự.")]
		public string FullName { get; set; } = string.Empty;

		[StringLength(
			50,
			ErrorMessage = "Số giấy tờ không được vượt quá 50 ký tự.")]
		public string? CitizenId { get; set; }
}
}
