using EzCondo.BackEndServer.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EzCondo.BackEndServer.Infrastructure.Configurations
{
	internal sealed class SupportTicketConfiguration()
		: EntityConfigurationBase<SupportTicket>("support_tickets", "platform")
	{
		protected override void ConfigureEntity(EntityTypeBuilder<SupportTicket> b)
		{
			ConfigureUuidPrimaryKey(b);
			ConfigureCreatedAt(b);
			ConfigureUpdatedAt(b);
			ConfigureIsDeleted(b);
			ConfigureDeletedAt(b);

			b.Property(x => x.Subject)
				.HasMaxLength(255)
				.IsRequired();

			b.Property(x => x.Status)
				.HasColumnType("platform.support_ticket_status")
				.HasDefaultValueSql("'open'::platform.support_ticket_status")
				.IsRequired();

			b.Property(x => x.TenantId)
				.IsRequired();

			b.Property(x => x.RequesterId)
				.IsRequired();

			b.Property(x => x.Message)
				.HasColumnType("text")
				.IsRequired();

			b.Property(x => x.ReceiverId)
				.IsRequired();

			b.HasOne(x => x.Tenant)
				.WithMany(x => x.SupportTickets)
				.HasForeignKey(x => x.TenantId)
				.OnDelete(DeleteBehavior.NoAction);

			b.HasOne(x => x.RequesterUser)
				.WithMany(x => x.RequesterUser)
				.HasForeignKey(x => new {x.TenantId, x.RequesterId })
				.HasPrincipalKey(x => new {x.TenantId, x.Id})
				.OnDelete(DeleteBehavior.NoAction);

			b.HasOne(x => x.ReceiverUser)
				.WithMany(x => x.ReceiverUser)
				.HasForeignKey(x => new { x.TenantId, x.ReceiverId })
				.HasPrincipalKey(x => new { x.TenantId, x.Id })
				.OnDelete(DeleteBehavior.NoAction);

			b.HasIndex(x => new { x.Id, x.TenantId }).IsUnique();
		}
	}
}
