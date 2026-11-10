using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EzCondo.BackEndServer.Migrations
{
    /// <inheritdoc />
    public partial class AddTenantSuspension : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "suspended_at",
                schema: "platform",
                table: "tenants",
                type: "timestamptz",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "suspension_effective_at",
                schema: "platform",
                table: "tenants",
                type: "timestamptz",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "suspension_notice_sent_at",
                schema: "platform",
                table: "tenants",
                type: "timestamptz",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "suspension_reason",
                schema: "platform",
                table: "tenants",
                type: "character varying(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "suspension_reason_code",
                schema: "platform",
                table: "tenants",
                type: "character varying(40)",
                maxLength: 40,
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "suspension_requested_by_admin_id",
                schema: "platform",
                table: "tenants",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "ix_tenants_pending_suspension",
                schema: "platform",
                table: "tenants",
                column: "suspension_effective_at",
                filter: "suspension_effective_at IS NOT NULL AND suspended_at IS NULL");

            migrationBuilder.CreateIndex(
                name: "IX_tenants_suspension_requested_by_admin_id",
                schema: "platform",
                table: "tenants",
                column: "suspension_requested_by_admin_id");

            migrationBuilder.AddCheckConstraint(
                name: "ck_tenants_suspended_at",
                schema: "platform",
                table: "tenants",
                sql: "status <> 'suspended' OR suspended_at IS NOT NULL");

            migrationBuilder.AddCheckConstraint(
                name: "ck_tenants_suspension_notice_period",
                schema: "platform",
                table: "tenants",
                sql: "suspension_notice_sent_at IS NULL OR suspension_effective_at >= suspension_notice_sent_at + INTERVAL '12 hours'");

            migrationBuilder.AddCheckConstraint(
                name: "ck_tenants_suspension_reason_code",
                schema: "platform",
                table: "tenants",
                sql: "suspension_reason_code IS NULL OR suspension_reason_code IN ('owner_request', 'policy_violation')");

            migrationBuilder.AddForeignKey(
                name: "FK_tenants_users_suspension_requested_by_admin_id",
                schema: "platform",
                table: "tenants",
                column: "suspension_requested_by_admin_id",
                principalSchema: "identity",
                principalTable: "users",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_tenants_users_suspension_requested_by_admin_id",
                schema: "platform",
                table: "tenants");

            migrationBuilder.DropIndex(
                name: "ix_tenants_pending_suspension",
                schema: "platform",
                table: "tenants");

            migrationBuilder.DropIndex(
                name: "IX_tenants_suspension_requested_by_admin_id",
                schema: "platform",
                table: "tenants");

            migrationBuilder.DropCheckConstraint(
                name: "ck_tenants_suspended_at",
                schema: "platform",
                table: "tenants");

            migrationBuilder.DropCheckConstraint(
                name: "ck_tenants_suspension_notice_period",
                schema: "platform",
                table: "tenants");

            migrationBuilder.DropCheckConstraint(
                name: "ck_tenants_suspension_reason_code",
                schema: "platform",
                table: "tenants");

            migrationBuilder.DropColumn(
                name: "suspended_at",
                schema: "platform",
                table: "tenants");

            migrationBuilder.DropColumn(
                name: "suspension_effective_at",
                schema: "platform",
                table: "tenants");

            migrationBuilder.DropColumn(
                name: "suspension_notice_sent_at",
                schema: "platform",
                table: "tenants");

            migrationBuilder.DropColumn(
                name: "suspension_reason",
                schema: "platform",
                table: "tenants");

            migrationBuilder.DropColumn(
                name: "suspension_reason_code",
                schema: "platform",
                table: "tenants");

            migrationBuilder.DropColumn(
                name: "suspension_requested_by_admin_id",
                schema: "platform",
                table: "tenants");
        }
    }
}
