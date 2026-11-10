using System.Text.Json.Serialization;

namespace EzCondo.BackEndServer.Contracts.Tenants
{
	[JsonConverter(typeof(JsonStringEnumConverter))]
	public enum TenantSuspensionReasonCode
	{
		Unspecified,
		OwnerRequest,
		PolicyViolation
	}
}
