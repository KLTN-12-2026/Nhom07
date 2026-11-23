using EzCondo.BackEndServer.Data;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EzCondo.BackEndServer.Migrations
{
	[DbContext(typeof(ApplicationDbContext))]
	[Migration("20261122130000_AddSubscriptionExpiryLifecycle")]
	public partial class AddSubscriptionExpiryLifecycle : Migration
	{
		protected override void Up(MigrationBuilder migrationBuilder)
		{
			// PostgreSQL enum labels cannot be removed safely. IF NOT EXISTS also
			// makes this safe when the label was added manually already.
			migrationBuilder.Sql(
				"ALTER TYPE platform.tenant_status_enum " +
				"ADD VALUE IF NOT EXISTS 'expired' AFTER 'suspended';",
				suppressTransaction: true);

			migrationBuilder.AddColumn<DateTimeOffset>(
				name: "expired_at",
				schema: "platform",
				table: "tenants",
				type: "timestamptz",
				nullable: true);

			migrationBuilder.AddColumn<string>(
				name: "expiration_reason_code",
				schema: "platform",
				table: "tenants",
				type: "character varying(50)",
				maxLength: 50,
				nullable: true);

			migrationBuilder.AddColumn<DateTimeOffset>(
				name: "expiry_notice_last_attempt_at",
				schema: "platform",
				table: "subscription_renewal_requests",
				type: "timestamptz",
				nullable: true);

			migrationBuilder.AddColumn<string>(
				name: "expiry_notice_last_error",
				schema: "platform",
				table: "subscription_renewal_requests",
				type: "character varying(1000)",
				maxLength: 1000,
				nullable: true);

			migrationBuilder.AddColumn<DateTimeOffset>(
				name: "expiry_notice_sent_at",
				schema: "platform",
				table: "subscription_renewal_requests",
				type: "timestamptz",
				nullable: true);
		}

		protected override void Down(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.DropColumn(
				name: "expired_at",
				schema: "platform",
				table: "tenants");

			migrationBuilder.DropColumn(
				name: "expiration_reason_code",
				schema: "platform",
				table: "tenants");

			migrationBuilder.DropColumn(
				name: "expiry_notice_last_attempt_at",
				schema: "platform",
				table: "subscription_renewal_requests");

			migrationBuilder.DropColumn(
				name: "expiry_notice_last_error",
				schema: "platform",
				table: "subscription_renewal_requests");

			migrationBuilder.DropColumn(
				name: "expiry_notice_sent_at",
				schema: "platform",
				table: "subscription_renewal_requests");

			// The expired enum label is intentionally retained because PostgreSQL
			// has no safe DROP VALUE operation.
		}
	}
}
