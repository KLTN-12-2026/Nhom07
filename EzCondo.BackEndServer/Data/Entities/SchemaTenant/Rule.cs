using EzCondo.BackEndServer.Data.Entities.SchemaIdentity;
using EzCondo.BackEndServer.Data.Enums;
using NpgsqlTypes;
using EzCondo.BackEndServer.Data.Entities.SchemaPlatform;

namespace EzCondo.BackEndServer.Data.Entities.SchemaTenant
{
	public class Rule
	{
		public Guid Id { get; set; }
		public string Title { get; set; } = null!;
		public string SourceUrl { get; set; } = null!;

		public Guid TenantId { get; set; }
		public Tenant Tenant { get; set; } = null!;
		public Guid ImportedBy { get; set; }
		public User TenantOwnerImported { get; set; } = null!;
	}
}
