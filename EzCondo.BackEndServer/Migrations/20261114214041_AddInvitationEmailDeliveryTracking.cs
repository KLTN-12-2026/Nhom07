using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EzCondo.BackEndServer.Migrations
{
    /// <inheritdoc />
    public partial class AddInvitationEmailDeliveryTracking : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {

			migrationBuilder.Sql(
	            """
                ALTER TYPE tenant.invitation_status_enum
                ADD VALUE IF NOT EXISTS 'pending_delivery';
                """,
	            suppressTransaction: true);

			migrationBuilder.DropIndex(
                name: "ix_invitations_active",
                schema: "tenant",
                table: "invitations");

            migrationBuilder.DropIndex(
                name: "ix_invitations_expiring",
                schema: "tenant",
                table: "invitations");

            migrationBuilder.AddColumn<int>(
                name: "email_send_attempt_count",
                schema: "tenant",
                table: "invitations",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "email_sent_at",
                schema: "tenant",
                table: "invitations",
                type: "timestamptz",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "last_email_attempt_at",
                schema: "tenant",
                table: "invitations",
                type: "timestamptz",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "last_email_error_code",
                schema: "tenant",
                table: "invitations",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

			migrationBuilder.Sql(
	            """
                UPDATE tenant.invitations
                SET status = 'pending_delivery'
                WHERE status = 'sent'
                  AND email_sent_at IS NULL;
                """);

			migrationBuilder.CreateIndex(
                name: "ix_invitations_active",
                schema: "tenant",
                table: "invitations",
                columns: new[] { "tenant_id", "email", "role_id" },
                unique: true,
                filter: "status IN ('pending_delivery', 'sent') AND is_deleted = false");

            migrationBuilder.CreateIndex(
                name: "ix_invitations_expiring",
                schema: "tenant",
                table: "invitations",
                columns: new[] { "status", "expires_at" },
                filter: "status IN ('pending_delivery', 'sent')");

            migrationBuilder.AddCheckConstraint(
                name: "ck_invitations_email_attempt_count",
                schema: "tenant",
                table: "invitations",
                sql: "email_send_attempt_count >= 0");

            migrationBuilder.AddCheckConstraint(
                name: "ck_invitations_sent_email",
                schema: "tenant",
                table: "invitations",
                sql: "status <> 'sent' OR email_sent_at IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_invitations_active",
                schema: "tenant",
                table: "invitations");

            migrationBuilder.DropIndex(
                name: "ix_invitations_expiring",
                schema: "tenant",
                table: "invitations");

            migrationBuilder.DropCheckConstraint(
                name: "ck_invitations_email_attempt_count",
                schema: "tenant",
                table: "invitations");

            migrationBuilder.DropCheckConstraint(
                name: "ck_invitations_sent_email",
                schema: "tenant",
                table: "invitations");

			migrationBuilder.Sql(
	            """
                UPDATE tenant.invitations
                SET status = 'sent'
                WHERE status = 'pending_delivery';
                """);

			migrationBuilder.DropColumn(
                name: "email_send_attempt_count",
                schema: "tenant",
                table: "invitations");

            migrationBuilder.DropColumn(
                name: "email_sent_at",
                schema: "tenant",
                table: "invitations");

            migrationBuilder.DropColumn(
                name: "last_email_attempt_at",
                schema: "tenant",
                table: "invitations");

            migrationBuilder.DropColumn(
                name: "last_email_error_code",
                schema: "tenant",
                table: "invitations");

            migrationBuilder.CreateIndex(
                name: "ix_invitations_active",
                schema: "tenant",
                table: "invitations",
                columns: new[] { "tenant_id", "email", "role_id" },
                unique: true,
                filter: "status = 'sent' AND is_deleted = false");

            migrationBuilder.CreateIndex(
                name: "ix_invitations_expiring",
                schema: "tenant",
                table: "invitations",
                columns: new[] { "status", "expires_at" },
                filter: "status = 'sent'");
        }
    }
}
