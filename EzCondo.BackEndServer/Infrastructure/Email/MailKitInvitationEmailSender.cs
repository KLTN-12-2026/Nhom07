using EzCondo.BackEndServer.Infrastructure.Email.Options;
using EzCondo.BackEndServer.Infrastructure.Invitations.Options;
using MailKit;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit;
using System.Text.Encodings.Web;

namespace EzCondo.BackEndServer.Infrastructure.Email
{
	public sealed class MailKitInvitationEmailSender(
		IOptions<SmtpOptions> smtpOptions,
		IOptions<InvitationLinkOptions> linkOptions)
		: IInvitationEmailSender
	{
		private readonly SmtpOptions _smtpOptions = smtpOptions.Value;

		private readonly InvitationLinkOptions _linkOptions = linkOptions.Value;

		public async Task SendAsync(
			InvitationEmailMessage email, 
			CancellationToken cancellationToken = default)
		{
			ArgumentNullException.ThrowIfNull(email);

			var roleDisplayName = GetRoleDisplayName(email.RecipientType);

			var requiresMobileApp = RequiresMobileApp(email.RecipientType) && email.AcceptanceUrl is null;

			var acceptUrl = email.AcceptanceUrl is { } url
				? new UriBuilder(url) { Fragment = "token=" + Uri.EscapeDataString(email.RawToken) }.Uri.AbsoluteUri
				: BuildAcceptUrl(email.RawToken);

			var tokenExpiresAtVietNam = 
				email.TokenExpiresAt.ToOffset(TimeSpan.FromHours(7));

			var tokenExpiresAtText =
				$"{tokenExpiresAtVietNam:dd/MM/yyyy HH:mm} (UTC+7)";

			var message = new MimeMessage();

			message.From.Add(
				new MailboxAddress(
					_smtpOptions.FromName,
					_smtpOptions.FromAddress));

			message.To.Add(
				new MailboxAddress(
					email.RecipientName,
					email.RecipientEmail));

			message.Subject = $"Lời mời {roleDisplayName} - {email.TenantName}";

			var encodedName = HtmlEncoder.Default.Encode(email.RecipientName);
			var encodedTenantName = HtmlEncoder.Default.Encode(email.TenantName);
			var encodedAcceptUrl = HtmlEncoder.Default.Encode(acceptUrl);

			var encodedRoleDisplayName =
				HtmlEncoder.Default.Encode(roleDisplayName);

			var setupInstructionText = requiresMobileApp
	? "Hãy tải và cài đặt ứng dụng EzCondo trên điện thoại trước. "
	  + "Sau đó mở liên kết bên dưới bằng thiết bị đã cài ứng dụng "
	  + "để thiết lập mật khẩu."
	: "Mở liên kết bên dưới để thiết lập mật khẩu "
	  + "và chấp nhận lời mời.";

			var setupInstructionHtml = requiresMobileApp
				? """
      <p>
          Hãy tải và cài đặt ứng dụng EzCondo trên điện thoại trước.
          Sau đó mở liên kết bên dưới bằng thiết bị đã cài ứng dụng
          để thiết lập mật khẩu.
      </p>
      """
				: """
      <p>
          Mở liên kết bên dưới để thiết lập mật khẩu
          và chấp nhận lời mời.
      </p>
      """;

			var bodyBuilder = new BodyBuilder
			{
				TextBody = string.Join(
					Environment.NewLine,
					$"Xin chào {email.RecipientName},",
					string.Empty,
					$"Bạn được mời trở thành {roleDisplayName} "
						+ $"của {email.TenantName} trên EzCondo.",
					string.Empty,
					setupInstructionText,
					string.Empty,
					"Liên kết thiết lập mật khẩu:",
					acceptUrl,
					string.Empty,
					"Liên kết thiết lập mật khẩu hết hạn lúc:",
					tokenExpiresAtText,
					string.Empty,
					"Nếu bạn không mong đợi email này, hãy bỏ qua."),

				HtmlBody = string.Join(
					Environment.NewLine,
					"<!DOCTYPE html>",
					"<html lang=\"vi\">",
					"<body>",
					$"<p>Xin chào {encodedName},</p>",
					"<p>",
					"Bạn được mời trở thành",
					$"<strong>{encodedRoleDisplayName}</strong>",
					"của",
					$"<strong>{encodedTenantName}</strong>",
					"trên EzCondo.",
					"</p>",
					setupInstructionHtml,
					"<p>",
					$"<a href=\"{encodedAcceptUrl}\">",
					"Thiết lập mật khẩu và chấp nhận lời mời",
					"</a>",
					"</p>",
					"<p>",
					"Liên kết thiết lập mật khẩu hết hạn lúc:",
					tokenExpiresAtText,
					"</p>",
					"<p>",
					"Nếu bạn không mong đợi email này, hãy bỏ qua.",
					"</p>",
					"</body>",
					"</html>")
			};

			message.Body = bodyBuilder.ToMessageBody();

			using var client =  new SmtpClient();

			var socketOptions = _smtpOptions.UseStartTls
				? SecureSocketOptions.StartTls
				: SecureSocketOptions.SslOnConnect;

			try
			{
				await client.ConnectAsync(
					_smtpOptions.Host,
					_smtpOptions.Port,
					socketOptions,
					cancellationToken);

				await client.AuthenticateAsync(
					_smtpOptions.Username,
					_smtpOptions.AppPassword,
					cancellationToken);

				await client.SendAsync(
					message,
					cancellationToken);
			}
			finally
			{
				if (client.IsConnected)
				{
					await client.DisconnectAsync(
						quit: true,
						CancellationToken.None);
				}
			}
		}

		private string BuildAcceptUrl(string rawToken)
		{
			ArgumentException.ThrowIfNullOrWhiteSpace(rawToken);

			var baseUrl =
				_linkOptions.PortalBaseUrl.TrimEnd('/');

			var path = _linkOptions.AcceptPath.StartsWith('/')
				? _linkOptions.AcceptPath
				: $"/{_linkOptions.AcceptPath}";

			return
				$"{baseUrl}{path}" +
				$"#token={Uri.EscapeDataString(rawToken)}";
		}

		private static string GetRoleDisplayName(
			InvitationRecipientType recipientType)
		{
			return recipientType switch
			{
				InvitationRecipientType.TenantAdmin => "Chủ chung cư",
				InvitationRecipientType.Manager => "Quản lý",
				InvitationRecipientType.Staff => "Nhân viên",
				InvitationRecipientType.Resident => "Cư dân",

				_ => throw new ArgumentOutOfRangeException(
					nameof(recipientType),
					recipientType,
					"Loại người nhận invitation không được hỗ trợ.")
			};
		}

		private static bool RequiresMobileApp(
			InvitationRecipientType recipientType)
		{
			return recipientType is
				InvitationRecipientType.Staff or
				InvitationRecipientType.Resident;
		}
	}
}
