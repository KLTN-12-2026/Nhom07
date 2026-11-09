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

			b.Property(x => x.Provider)
				.HasMaxLength(30)
				.HasDefaultValue("payos")
				.IsRequired();

			b.Property(x => x.OwnerScope)
				.HasColumnType("payment.provider_owner_scope")
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
				.OnDelete(DeleteBehavior.Restrict);

			b.ToTable(x =>
			{
				x.HasCheckConstraint(
					"ck_provider_accounts_provider",
					"(provider = 'payos')");

				x.HasCheckConstraint(
					"ck_provider_accounts_owner",
					"((owner_scope = 'platform' AND tenant_id IS NULL) OR (owner_scope = 'tenant' AND tenant_id IS NOT NULL))");

				x.HasCheckConstraint(
					"ck_provider_accounts_capability",
					"(supports_collection OR supports_payout)");

				x.HasCheckConstraint(
					"ck_provider_accounts_supports_collection_secret",
					"(NOT supports_collection OR payment_credentials_ref IS NOT NULL)");

				x.HasCheckConstraint(
					"ck_provider_accounts_payout_secret",
					"(NOT supports_payout OR payout_credentials_ref IS NOT NULL)");

				x.HasCheckConstraint(
					"ck_provider_accounts_settings",
					"(jsonb_typeof(settings) = 'object')");
			});

			b.HasIndex(x => x.Code)
				.HasDatabaseName("ux_provider_accounts_platform_code")
				.IsUnique()
				.HasFilter("owner_scope = 'platform'");

			b.HasIndex(x => new { x.TenantId, x.Code })
				.HasDatabaseName("ux_provider_accounts_tenant_code")
				.IsUnique()
				.HasFilter("owner_scope = 'tenant'");

			b.HasIndex(x => x.WebhookKey)
				.IsUnique();

			b.HasIndex(x => new { x.Provider, x.MerchantCode })
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
