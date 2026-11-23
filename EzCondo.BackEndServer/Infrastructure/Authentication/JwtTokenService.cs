using EzCondo.BackEndServer.Data.Entities.SchemaIdentity;
using EzCondo.BackEndServer.Data.Enums;
using EzCondo.BackEndServer.Infrastructure.Authentication.Options;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace EzCondo.BackEndServer.Infrastructure.Authentication
{
	public sealed class JwtTokenService(
		IOptions<JwtOptions> options,
		TimeProvider timeProvider)
		: IJwtTokenService
	{
		private readonly JwtOptions _options = options.Value;
		public AccessTokenResult CreateAccessToken(
			User user,
			RoleScopeEnum scope,
			IReadOnlyCollection<string> roleCodes)
		{
			ArgumentNullException.ThrowIfNull(user);
			ArgumentNullException.ThrowIfNull(roleCodes);

			ValidateScope(user, scope);

			if (string.IsNullOrWhiteSpace(user.SecurityStamp))
			{
				throw new InvalidOperationException(
					$"User '{user.Id}' không có SecurityStamp.");
			}

			if (string.IsNullOrWhiteSpace(user.Email))
			{
				throw new InvalidOperationException(
					$"User '{user.Id}' không có email.");
			}

			var now = timeProvider.GetUtcNow();

			var expiresAt = now.AddMinutes(
				_options.AccessTokenMinutes);

			var claims = new List<Claim>
			{
				new(
					AppClaimTypes.Subject,
					user.Id.ToString()),

				new(
					AppClaimTypes.JwtId,
					Guid.NewGuid().ToString()),

				new(
					AppClaimTypes.Name,
					user.FullName),

				new(
					AppClaimTypes.Email,
					user.Email),

				new(
					AppClaimTypes.Scope,
					ToScopeClaim(scope)),

				new(
					AppClaimTypes.SecurityStamp,
					user.SecurityStamp)
			};

			if(scope == RoleScopeEnum.Tenant)
			{
				claims.Add(
					new Claim(AppClaimTypes.TenantId, user.TenantId!.Value.ToString()));
			}

			foreach(var roleCode in roleCodes
				.Where(x => !string.IsNullOrWhiteSpace(x))
				.Distinct(StringComparer.Ordinal)
				.OrderBy(x => x, StringComparer.Ordinal))
			{
				claims.Add(new Claim(AppClaimTypes.Role, roleCode));
			}

			var signingKeyBytes = Convert.FromBase64String(_options.SigningKey);

			var signingCredentials = new SigningCredentials(
				new SymmetricSecurityKey(signingKeyBytes),
				SecurityAlgorithms.HmacSha256);

			var token = new JwtSecurityToken(
				issuer: _options.Issuer,
				audience: _options.Audience,
				claims: claims,
				notBefore: now.UtcDateTime,
				expires: expiresAt.UtcDateTime,
				signingCredentials: signingCredentials);

			var serializedToken = new JwtSecurityTokenHandler().WriteToken(token);

			return new AccessTokenResult(serializedToken, expiresAt);
		}

		private static void ValidateScope(User user, RoleScopeEnum scope)
		{
			if(scope == RoleScopeEnum.Platform
				&& user.TenantId is not null)
			{
				throw new InvalidOperationException(
					"Không thể phát Platform token cho tài khoản thuộc tenant.");
			}

			if(scope == RoleScopeEnum.Tenant && user.TenantId is null)
			{
				throw new InvalidOperationException("Không thể phát Tenant token cho tìa khoản Platform.");
			}
		}

		private static string ToScopeClaim(RoleScopeEnum scope)
		{
			return scope switch
			{
				RoleScopeEnum.Platform => "platform",
				RoleScopeEnum.Tenant => "tenant",
				_ => throw new ArgumentOutOfRangeException(
					nameof(scope),
					scope,
					"Scope không được hỗ trợ.")
			};
		}
	}
}
