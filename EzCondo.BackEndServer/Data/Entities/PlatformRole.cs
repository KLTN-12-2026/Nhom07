using Microsoft.AspNetCore.Identity;

namespace EzCondo.BackEndServer.Data.Entities
{
	public class PlatformRole
	{
		public Guid Id { get; set; }
		public string Code { get; set; }
		public string Name { get; set; }
		public bool IsSystem { get; set; }
		public DateTimeOffset CreatedAt { get; set; }
	}
}
