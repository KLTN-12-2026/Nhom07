using EzCondo.BackEndServer.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EzCondo.BackEndServer.Infrastructure.Configurations
{
	internal sealed class IdentityUserConfiguration()
		: EntityConfigurationBase<IdentityUser>("users", "identity")
	{
		protected override void ConfigureEntity(EntityTypeBuilder<IdentityUser> b)
		{
			ConfigureUuidPrimaryKey(b);
			ConfigureCreatedAt(b);
			ConfigureUpdatedAt(b);

			b.Property(x => x.FullName).HasMaxLength(200).IsRequired();
			b.Property(x => x.Email).HasColumnType("citext").HasMaxLength(256);
			b.Property(x => x.PhoneNumber).HasMaxLength(30);
			b.Property(x => x.AvatarUrl).HasColumnType("text");
			b.Property(x => x.Status).HasColumnType("identity.user_status_enum").HasDefaultValueSql("'pending'::identity.user_status_enum");
			b.Property(x => x.LastLogInAt).HasColumnName("last_login_at");
			b.Property(x => x.CitizenId).HasMaxLength(50).IsRequired();

			b.HasOne(x => x.Tenant).WithMany().HasForeignKey(x => x.TenantId).OnDelete(DeleteBehavior.Cascade);

			b.HasIndex(x => x.Email).IsUnique();
			b.HasIndex(x => x.PhoneNumber).IsUnique();

			b.ToTable(t => t.HasCheckConstraint(
				"CK_users_email_or_phone_number",
				"email IS NOT NULL OR phone_number IS NOT NULL"
			));

			b.HasAlternateKey(x => new { x.TenantId, x.Id });
		}
	}
}
