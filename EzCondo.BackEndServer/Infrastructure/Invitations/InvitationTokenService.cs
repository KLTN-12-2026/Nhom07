using Microsoft.AspNetCore.WebUtilities;
using System.Security.Cryptography;
using System.Text;

namespace EzCondo.BackEndServer.Infrastructure.Invitations
{
	public sealed class InvitationTokenService
		: IInvitationTokenService
	{
		private const int TokenSizeBytes = 32;

		public string ComputeHash(string token)
		{
			ArgumentException.ThrowIfNullOrWhiteSpace(token);

			var tokenBytes = Encoding.UTF8.GetBytes(token);
			var hashBytes = SHA256.HashData(tokenBytes);

			return Convert.ToHexString(hashBytes);
		}

		public InvitationToken Generate()
		{
			var randomBytes = RandomNumberGenerator.GetBytes(TokenSizeBytes);
			var token = WebEncoders.Base64UrlEncode(randomBytes);
			var hash = ComputeHash(token);

			return new InvitationToken(token, hash);
		}
	}
}
