namespace EzCondo.BackEndServer.Data.Entities
{
	public interface ISoftDeletableEntity
	{
		bool IsDeleted { get; set; }
		DateTimeOffset? DeletedAt { get; set; }
	}
}
