using EzCondo.BackEndServer.Data.Entities.SchemaIdentity;
using EzCondo.BackEndServer.Data.Enums;

namespace EzCondo.BackEndServer.Infrastructure.Authentication
{
	public interface IJwtTokenService
	{
		AccessTokenResult CreateAccessToken(
			User user,
			RoleScopeEnum scope,
			IReadOnlyCollection<string> roleCodes);
	}
}
