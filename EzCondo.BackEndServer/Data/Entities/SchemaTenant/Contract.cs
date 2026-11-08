using EzCondo.BackEndServer.Data.Entities.SchemaIdentity;
using EzCondo.BackEndServer.Data.Enums;
using NpgsqlTypes;
using EzCondo.BackEndServer.Data.Entities.SchemaPlatform;


namespace EzCondo.BackEndServer.Data.Entities.SchemaTenant
{
	public class Contract
	{
		public Guid Id { get; set; }
		public string ContractNo { get; set; } = string.Empty;
		public string? ContractUrl { get; set; }
		public DateTimeOffset StartsAt { get; set; }
		public DateTimeOffset EndsAt { get; set; }
		public Guid TenantId { get; set; }
		public Guid ImportedBy { get; set; }
		public Guid ApartmentId { get; set; }

		public Tenant Tenant { get; set; }
		public User StaffImported { get; set; }
		public Apartment Apartment { get; set; }
	}
}
