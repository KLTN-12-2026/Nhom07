using EzCondo.BackEndServer.Infrastructure.Email.Options;
using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit;
using MailKit.Net.Smtp;
using MailKit;

namespace EzCondo.BackEndServer.Infrastructure.Email
{
	public sealed class MailkitSubscriptionExpiryNoticeSender(
		IOptions<SmtpOptions> smtpOptions)
		: ISubscriptionExpiryNoticeSender
	{
		private readonly SmtpOptions options = smtpOptions.Value;

		public async Task SendAsync(
			SubscriptionExpiryNoticeMessage message, 
			CancellationToken cancellationToken = default)
		{
			var mimeMessage = new MimeMessage();

			mimeMessage.From.Add(
				new MailboxAddress(
					options.FromName,
					options.FromAddress));

			mimeMessage.To.Add(
				new MailboxAddress(
					message.RecipientName,
					message.RecipientEmail));

			mimeMessage.Subject = $"Thông báo hết hạn gói dịch vụ - {message.TenantName}";

			mimeMessage.Body = new BodyBuilder
			{
				TextBody = $"""
					Xin chào {message.RecipientName},
					
					Gói {message.PlanName} của {message.TenantName}
					sẽ hết hạn vào ngày {message.SubscriptionEndsAt:dd/MM/yyyy},

					Theo lựa chọn không gia hạn,
					tenant sẽ mất quyền truy cập từ
					{message.AccessExpiresAt.ToOffset(TimeSpan.FromHours(7)):dd/MM/yyyy HH:mm} (UTC+7),

					Trân trọng,
					EzCondo
					"""
			}
			.ToMessageBody();

			using var client = new SmtpClient();

			var socketOptions = options.UseStartTls
				? SecureSocketOptions.StartTls
				: SecureSocketOptions.SslOnConnect;

			try
			{
				await client.ConnectAsync(
					options.Host,
					options.Port,
					socketOptions,
					cancellationToken);

				await client.AuthenticateAsync(
					options.Username,
					options.AppPassword,
					cancellationToken);

				await client.SendAsync(
					mimeMessage,
					cancellationToken);
			}
			finally
			{
				if (client.IsConnected)
				{
					await client.DisconnectAsync(
						true,
						CancellationToken.None);
				}
			}
		}
	}
}
