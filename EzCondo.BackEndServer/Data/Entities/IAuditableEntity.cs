namespace EzCondo.BackEndServer.Data.Entities
{
	public interface IAuditableEntity
	{
		DateTimeOffset CreatedAt { get; set; }
		DateTimeOffset? UpdatedAt { get; set; }
	}
}
