using EzCondo.BackEndServer.Infrastructure.Payments.PayOS.Models;
using System.Security.Cryptography;
using System.Text;

namespace EzCondo.BackEndServer.Services.Payments.PayOS
{
	public static class PayOSWebhookEventKeyFactory
	{
		public static string Create(PayOSVerifiedWebhookData data)
		{
			var canonicalValue = string.Join(
				"|",
				data.Reference?.Trim() ?? string.Empty,
				data.PaymentLinkId?.Trim() ?? string.Empty,
				data.OrderCode,
				data.Amount,
				data.Currency?.Trim().ToUpperInvariant() ?? string.Empty,
				data.TransactionDateTime?.Trim() ?? string.Empty);

			var hash = SHA256.HashData(
				Encoding.UTF8.GetBytes(canonicalValue));

			return Convert.ToHexString(hash)
				.ToLowerInvariant();
		}
	}
}
