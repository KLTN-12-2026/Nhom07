using EzCondo.BackEndServer.Data.Entities.SchemaPayment;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EzCondo.BackEndServer.Infrastructure.Configurations.SchemaPayment
{
	internal sealed class ProviderAccountConfiguration()
		: EntityConfigurationBase<ProviderAccount>("provider_accounts", "payment")
	{
		protected override void ConfigureEntity(EntityTypeBuilder<ProviderAccount> b)
		{
			ConfigureUuidPrimaryKey(b);
			ConfigureCreatedAt(b);
			ConfigureUpdatedAt(b);

			b.Property(x => x.Code)
				.HasMaxLength(80)
				.IsRequired();

			b.Property(x => x.Provier)
				.HasMaxLength(30)
				.HasDefaultValue("PAYOS")
				.IsRequired();

			b.Property(x => x.OwnerScope)
				.HasColumnType("security.role_scope_enum")
				.IsRequired();

			b.Property(x => x.MerchantCode)
				.HasMaxLength(150);

			b.Property(x => x.PaymentCredentialsRef)
				.HasColumnType("text");

			b.Property(x => x.PayoutCredentialsRef)
				.HasColumnType("text");

			b.Property(x => x.SupportsCollection)
				.HasDefaultValue(false)
				.IsRequired();

			b.Property(x => x.SupportsPayout)
				.HasDefaultValue(false)
				.IsRequired();

			b.Property(x => x.Settings)
				.HasColumnType("jsonb")
				.HasDefaultValueSql("'{}'::jsonb")
				.IsRequired();

			b.Property(x => x.IsActive)
				.HasDefaultValue(true)
				.IsRequired();

			b.HasOne(x => x.Tenant)
				.WithMany(x => x.ProviderAccounts)
				.HasForeignKey(x => x.TenantId)
				.OnDelete(DeleteBehavior.Cascade);

			b.ToTable(x =>
			{
				x.HasCheckConstraint(
					"ck_provider_accounts_provider",
					"CHECK(provider = 'payos')");

				x.HasCheckConstraint(
					"ck_provider_accounts_owner",
					"CHECK ((owner_scope = 'platform' AND tenant_id IS NULL) OR (owner_scope = 'tenant' AND tenant_id IS NOT NULL))");

				x.HasCheckConstraint(
					"ck_provider_accounts_capability",
					"CHECK (supports_collection OR supports_payout)");

				x.HasCheckConstraint(
					"ck_provider_accounts_supports_collection_secret",
					"CHECK (NOT supports_collection OR payment_credentials_ref IS NOT NULL)");

				x.HasCheckConstraint(
					"ck_provider_accounts_payout_secret",
					"CHECK (NOT supports_payout OR payout_credentials_ref IS NOT NULL)");

				x.HasCheckConstraint(
					"ck_provider_accounts_settings",
					"CHECK (jsonb_typeof(settings) = 'object')");
			});

			b.HasIndex(x => x.Code)
				.IsUnique();

			b.HasIndex(x => new {x.OwnerScope, x.Id})
				.IsUnique();

			b.HasIndex(x => x.WebhookKey)
				.IsUnique();

			b.HasIndex(x => new { x.TenantId, x.Id })
				.IsUnique();

			b.HasIndex(x => new { x.Provier, x.MerchantCode })
				.HasDatabaseName("ux_provider_accounts_merchant")
				.IsUnique()
				.HasFilter("merchant_code IS NOT NULL");

			b.HasIndex(x => new { x.TenantId, x.IsActive })
				.HasDatabaseName("ix_provider_accounts_tenant_active")
				.HasFilter("owner_scope = 'tenant'");

			b.HasIndex(x => x.IsActive)
				.HasDatabaseName("ix_provider_accounts_platform_active")
				.HasFilter("owner_scope = 'platform'");
		}
	}
}
