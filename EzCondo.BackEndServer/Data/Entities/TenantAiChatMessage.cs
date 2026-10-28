using EzCondo.BackEndServer.Data.Enums;
using System.Text.Json;

namespace EzCondo.BackEndServer.Data.Entities
{
	public class TenantAiChatMessage
	{
		public Guid Id { get; set; }
		public AiRoleEnum Role { get; set; }
		public string Content { get; set; }
		public string Citations { get; set; }
		public string? Model { get; set; }
		public DateTimeOffset CreatedAt { get; set; }

		public Guid SessionId { get; set; }
		public TenantAiChatSession TenantAiChatSession { get; set; }
		public Guid TenantId { get; set; }
		public Tenant Tenant { get; set; }
	}
}
