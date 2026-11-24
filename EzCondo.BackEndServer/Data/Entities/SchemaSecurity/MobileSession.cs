namespace EzCondo.BackEndServer.Data.Entities.SchemaSecurity;

public sealed class MobileSession
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public DateTimeOffset ExpiresAt { get; set; }
}
