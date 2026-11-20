using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EzCondo.BackEndServer.Migrations
{
    /// <inheritdoc />
    public partial class AddSubscriptionInvoiceIssuanceAudit : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "issued_at",
                schema: "platform",
                table: "subscription_invoices",
                type: "timestamptz",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "issued_by_admin_id",
                schema: "platform",
                table: "subscription_invoices",
                type: "uuid",
                nullable: true);

			migrationBuilder.Sql(
	            """
                UPDATE platform.subscription_invoices
                SET
                    issued_by_admin_id = created_by_admin_id,
                    issued_at = created_at
                WHERE status <> 'draft';
                """);

			migrationBuilder.CreateIndex(
                name: "IX_subscription_invoices_issued_by_admin_id",
                schema: "platform",
                table: "subscription_invoices",
                column: "issued_by_admin_id");

            migrationBuilder.AddCheckConstraint(
                name: "ck_subscription_invoices_issue_audit",
                schema: "platform",
                table: "subscription_invoices",
                sql: "(\r\n	status = 'draft'\r\n	AND issued_at IS NULL\r\n	AND issued_by_admin_id IS NULL\r\n)\r\nOR\r\n(\r\n	status <> 'draft'\r\n	AND issued_at IS NOT NULL\r\n	AND issued_by_admin_id IS NOT NULL\r\n)");

            migrationBuilder.AddForeignKey(
                name: "FK_subscription_invoices_users_issued_by_admin_id",
                schema: "platform",
                table: "subscription_invoices",
                column: "issued_by_admin_id",
                principalSchema: "identity",
                principalTable: "users",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_subscription_invoices_users_issued_by_admin_id",
                schema: "platform",
                table: "subscription_invoices");

            migrationBuilder.DropIndex(
                name: "IX_subscription_invoices_issued_by_admin_id",
                schema: "platform",
                table: "subscription_invoices");

            migrationBuilder.DropCheckConstraint(
                name: "ck_subscription_invoices_issue_audit",
                schema: "platform",
                table: "subscription_invoices");

            migrationBuilder.DropColumn(
                name: "issued_at",
                schema: "platform",
                table: "subscription_invoices");

            migrationBuilder.DropColumn(
                name: "issued_by_admin_id",
                schema: "platform",
                table: "subscription_invoices");
        }
    }
}
