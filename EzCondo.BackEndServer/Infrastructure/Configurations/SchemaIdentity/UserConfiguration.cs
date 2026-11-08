using EzCondo.BackEndServer.Data.Entities.SchemaIdentity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EzCondo.BackEndServer.Infrastructure.Configurations.SchemaIdentity
{
	internal sealed class UserConfiguration()
		: EntityConfigurationBase<User>("users", "identity")
	{
		protected override void ConfigureEntity(EntityTypeBuilder<User> b)
		{
			ConfigureUuidPrimaryKey(b);
			ConfigureCreatedAt(b);
			ConfigureUpdatedAt(b);
			ConfigureDeletedAt(b);
			ConfigureIsDeleted(b);

			b.Property(x => x.FullName).HasMaxLength(200).IsRequired();
			b.Property(x => x.AvatarUrl).HasColumnType("text");
			b.Property(x => x.Status).HasColumnType("identity.user_status_enum").HasDefaultValueSql("'inviting'::identity.user_status_enum").IsRequired();
			b.Property(x => x.CitizenId).HasMaxLength(50);
			b.Property(x => x.EmployeementContract).HasColumnType("text");
			b.Property(x => x.DateOfBirth).HasColumnType("date");
			b.Property(x => x.BankName).HasMaxLength(100);
			b.Property(x => x.BankAccountNumber).HasMaxLength(50);


			b.HasOne(x => x.Tenant)
				.WithMany(x => x.Users)
				.HasForeignKey(x => x.TenantId)
				.OnDelete(DeleteBehavior.Restrict);

			b.ToTable(x =>
			{
				x.HasCheckConstraint(
					"CK_users_email_or_phone_number_not_null",
					"email IS NOT NULL OR phone_number IS NOT NULL");

				x.HasCheckConstraint(
					"CK_users_is_deleted_and_deleted_at",
					"(is_deleted = false AND deleted_at IS NULL) OR (is_deleted = true AND deleted_at IS NOT NULL)");

				x.HasCheckConstraint(
					"CK_users_bank_name_and_bank_account_name",
					"(bank_name IS NULL AND banl_account_name IS NULL) OR (bank_name IS NOT NULL AND bank_account_name IS NOT NULL)");
			});

			b.HasIndex(x => x.NormalizedEmail).IsUnique();
			b.HasIndex(x => x.PhoneNumber).IsUnique();

			b.HasIndex(x => x.NormalizedEmail)
				.HasDatabaseName("ux_users_normalized_email")
				.IsUnique()
				.HasFilter("normalized_email IS NOT NULL");

			b.HasIndex(x => x.PhoneNumber)
				.HasDatabaseName("ux_users_phone")
				.IsUnique()
				.HasFilter("phone_number IS NOT NULL");

			b.HasIndex(x => new { x.TenantId, x.Id }).IsUnique();
		}
	}
}
