using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EzCondo.BackEndServer.Migrations
{
    /// <inheritdoc />
    public partial class AddSubscriptionInvoiceDraft : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {

			migrationBuilder.Sql(
	            """
                ALTER TYPE platform.subscription_invoice_status
                ADD VALUE IF NOT EXISTS 'draft' BEFORE 'issued';
                """,
	            suppressTransaction: true);

			migrationBuilder.DropIndex(
                name: "IX_subscription_invoices_tenant_id_subscription_id",
                schema: "platform",
                table: "subscription_invoices");

            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:Enum:identity.user_status_enum", "active,locked,inviting")
                .Annotation("Npgsql:Enum:payment.payment_request_status", "pending,underpaid,paid,cancelled,expired,failed")
                .Annotation("Npgsql:Enum:payment.payment_transaction_status", "received,verified,rejected")
                .Annotation("Npgsql:Enum:payment.payout_batch_status", "created,submitting,processing,partially_succeeded,succeeded,failed,cancelled")
                .Annotation("Npgsql:Enum:payment.payout_item_status_enum", "created,processing,succeeded,failed,cancelled")
                .Annotation("Npgsql:Enum:payment.processing_status_enum", "received,processing,succeeded,failed")
                .Annotation("Npgsql:Enum:payment.provider_owner_scope", "platform,tenant")
				.Annotation("Npgsql:Enum:platform.subscription_invoice_status", "issued,partially_paid,paid,overdue,cancelled")
				.Annotation("Npgsql:Enum:platform.subscription_payment_attempt_status", "pending,partially_paid,paid,cancelled,failed")
                .Annotation("Npgsql:Enum:platform.subscription_status_enum", "trial,active,past_due,cancelled,expired")
                .Annotation("Npgsql:Enum:platform.tenant_status_enum", "trial,active,suspended,closed")
                .Annotation("Npgsql:Enum:security.role_scope_enum", "platform,tenant")
                .Annotation("Npgsql:Enum:tenant.invitation_status_enum", "accepted,sent,revoked,expired,pending_delivery")
                .Annotation("Npgsql:Enum:tenant.invoice_payment_attempt_status", "pending,partially_paid,paid,cancelled,failed")
                .Annotation("Npgsql:Enum:tenant.payroll_disbursement_item_status", "ready,processing,succeeded,failed")
                .Annotation("Npgsql:Enum:tenant.payroll_disbursement_status", "ready,issuing,processing,partially_succeeded,succeeded,failed")
                .Annotation("Npgsql:Enum:tenant.payroll_status_enum", "draft,pending_approval,approved,rejected,published")
                .Annotation("Npgsql:Enum:tenant.tenant_invoice_status", "draft,pending_approval,approved,rejected,published,paid,unpaid")
                .Annotation("Npgsql:PostgresExtension:citext", ",,")
                .OldAnnotation("Npgsql:Enum:identity.user_status_enum", "active,locked,inviting")
                .OldAnnotation("Npgsql:Enum:payment.payment_request_status", "pending,underpaid,paid,cancelled,expired,failed")
                .OldAnnotation("Npgsql:Enum:payment.payment_transaction_status", "received,verified,rejected")
                .OldAnnotation("Npgsql:Enum:payment.payout_batch_status", "created,submitting,processing,partially_succeeded,succeeded,failed,cancelled")
                .OldAnnotation("Npgsql:Enum:payment.payout_item_status_enum", "created,processing,succeeded,failed,cancelled")
                .OldAnnotation("Npgsql:Enum:payment.processing_status_enum", "received,processing,succeeded,failed")
                .OldAnnotation("Npgsql:Enum:payment.provider_owner_scope", "platform,tenant")
                .OldAnnotation("Npgsql:Enum:platform.subscription_invoice_status", "issued,partially_paid,paid,overdue,cancelled")
                .OldAnnotation("Npgsql:Enum:platform.subscription_payment_attempt_status", "pending,partially_paid,paid,cancelled,failed")
                .OldAnnotation("Npgsql:Enum:platform.subscription_status_enum", "trial,active,past_due,cancelled,expired")
                .OldAnnotation("Npgsql:Enum:platform.tenant_status_enum", "trial,active,suspended,closed")
                .OldAnnotation("Npgsql:Enum:security.role_scope_enum", "platform,tenant")
                .OldAnnotation("Npgsql:Enum:tenant.invitation_status_enum", "accepted,sent,revoked,expired,pending_delivery")
                .OldAnnotation("Npgsql:Enum:tenant.invoice_payment_attempt_status", "pending,partially_paid,paid,cancelled,failed")
                .OldAnnotation("Npgsql:Enum:tenant.payroll_disbursement_item_status", "ready,processing,succeeded,failed")
                .OldAnnotation("Npgsql:Enum:tenant.payroll_disbursement_status", "ready,issuing,processing,partially_succeeded,succeeded,failed")
                .OldAnnotation("Npgsql:Enum:tenant.payroll_status_enum", "draft,pending_approval,approved,rejected,published")
                .OldAnnotation("Npgsql:Enum:tenant.tenant_invoice_status", "draft,pending_approval,approved,rejected,published,paid,unpaid")
                .OldAnnotation("Npgsql:PostgresExtension:citext", ",,");

            migrationBuilder.CreateSequence(
                name: "subscription_invoice_no_seq",
                schema: "platform");

            migrationBuilder.AddColumn<Guid>(
                name: "created_by_admin_id",
                schema: "platform",
                table: "subscription_invoices",
                type: "uuid",
                nullable: false);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "deleted_at",
                schema: "platform",
                table: "subscription_invoices",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "deleted_by_admin_id",
                schema: "platform",
                table: "subscription_invoices",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "is_deleted",
                schema: "platform",
                table: "subscription_invoices",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateIndex(
                name: "IX_subscription_invoices_created_by_admin_id",
                schema: "platform",
                table: "subscription_invoices",
                column: "created_by_admin_id");

            migrationBuilder.CreateIndex(
                name: "IX_subscription_invoices_deleted_by_admin_id",
                schema: "platform",
                table: "subscription_invoices",
                column: "deleted_by_admin_id");

            migrationBuilder.CreateIndex(
                name: "ux_subscription_invoices_active_period",
                schema: "platform",
                table: "subscription_invoices",
                columns: new[] { "tenant_id", "subscription_id", "billing_period_from", "billing_period_to" },
                unique: true,
                filter: "is_deleted = false AND billing_period_from IS NOT NULL AND billing_period_to IS NOT NULL");

            migrationBuilder.AddCheckConstraint(
                name: "ck_subscription_invoices_draft_unpaid",
                schema: "platform",
                table: "subscription_invoices",
                sql: "status <> 'draft' OR paid_amount = 0");

            migrationBuilder.AddCheckConstraint(
                name: "ck_subscription_invoices_soft_delete",
                schema: "platform",
                table: "subscription_invoices",
                sql: "(\r\n	is_deleted = false\r\n	AND deleted_at IS NULL\r\n	AND deleted_by_admin_id IS NULL\r\n)\r\nOR\r\n(\r\n	is_deleted = true\r\n	AND deleted_at IS NOT NULL\r\n	AND deleted_by_admin_id IS NOT NULL\r\n)");

            migrationBuilder.AddForeignKey(
                name: "FK_subscription_invoices_users_created_by_admin_id",
                schema: "platform",
                table: "subscription_invoices",
                column: "created_by_admin_id",
                principalSchema: "identity",
                principalTable: "users",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_subscription_invoices_users_deleted_by_admin_id",
                schema: "platform",
                table: "subscription_invoices",
                column: "deleted_by_admin_id",
                principalSchema: "identity",
                principalTable: "users",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_subscription_invoices_users_created_by_admin_id",
                schema: "platform",
                table: "subscription_invoices");

            migrationBuilder.DropForeignKey(
                name: "FK_subscription_invoices_users_deleted_by_admin_id",
                schema: "platform",
                table: "subscription_invoices");

            migrationBuilder.DropIndex(
                name: "IX_subscription_invoices_created_by_admin_id",
                schema: "platform",
                table: "subscription_invoices");

            migrationBuilder.DropIndex(
                name: "IX_subscription_invoices_deleted_by_admin_id",
                schema: "platform",
                table: "subscription_invoices");

            migrationBuilder.DropIndex(
                name: "ux_subscription_invoices_active_period",
                schema: "platform",
                table: "subscription_invoices");

            migrationBuilder.DropCheckConstraint(
                name: "ck_subscription_invoices_draft_unpaid",
                schema: "platform",
                table: "subscription_invoices");

            migrationBuilder.DropCheckConstraint(
                name: "ck_subscription_invoices_soft_delete",
                schema: "platform",
                table: "subscription_invoices");

            migrationBuilder.DropColumn(
                name: "created_by_admin_id",
                schema: "platform",
                table: "subscription_invoices");

            migrationBuilder.DropColumn(
                name: "deleted_at",
                schema: "platform",
                table: "subscription_invoices");

            migrationBuilder.DropColumn(
                name: "deleted_by_admin_id",
                schema: "platform",
                table: "subscription_invoices");

            migrationBuilder.DropColumn(
                name: "is_deleted",
                schema: "platform",
                table: "subscription_invoices");

            migrationBuilder.DropSequence(
                name: "subscription_invoice_no_seq",
                schema: "platform");

            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:Enum:identity.user_status_enum", "active,locked,inviting")
                .Annotation("Npgsql:Enum:payment.payment_request_status", "pending,underpaid,paid,cancelled,expired,failed")
                .Annotation("Npgsql:Enum:payment.payment_transaction_status", "received,verified,rejected")
                .Annotation("Npgsql:Enum:payment.payout_batch_status", "created,submitting,processing,partially_succeeded,succeeded,failed,cancelled")
                .Annotation("Npgsql:Enum:payment.payout_item_status_enum", "created,processing,succeeded,failed,cancelled")
                .Annotation("Npgsql:Enum:payment.processing_status_enum", "received,processing,succeeded,failed")
                .Annotation("Npgsql:Enum:payment.provider_owner_scope", "platform,tenant")
				.OldAnnotation("Npgsql:Enum:platform.subscription_invoice_status", "issued,partially_paid,paid,overdue,cancelled")
				.Annotation("Npgsql:Enum:platform.subscription_payment_attempt_status", "pending,partially_paid,paid,cancelled,failed")
                .Annotation("Npgsql:Enum:platform.subscription_status_enum", "trial,active,past_due,cancelled,expired")
                .Annotation("Npgsql:Enum:platform.tenant_status_enum", "trial,active,suspended,closed")
                .Annotation("Npgsql:Enum:security.role_scope_enum", "platform,tenant")
                .Annotation("Npgsql:Enum:tenant.invitation_status_enum", "accepted,sent,revoked,expired,pending_delivery")
                .Annotation("Npgsql:Enum:tenant.invoice_payment_attempt_status", "pending,partially_paid,paid,cancelled,failed")
                .Annotation("Npgsql:Enum:tenant.payroll_disbursement_item_status", "ready,processing,succeeded,failed")
                .Annotation("Npgsql:Enum:tenant.payroll_disbursement_status", "ready,issuing,processing,partially_succeeded,succeeded,failed")
                .Annotation("Npgsql:Enum:tenant.payroll_status_enum", "draft,pending_approval,approved,rejected,published")
                .Annotation("Npgsql:Enum:tenant.tenant_invoice_status", "draft,pending_approval,approved,rejected,published,paid,unpaid")
                .Annotation("Npgsql:PostgresExtension:citext", ",,")
                .OldAnnotation("Npgsql:Enum:identity.user_status_enum", "active,locked,inviting")
                .OldAnnotation("Npgsql:Enum:payment.payment_request_status", "pending,underpaid,paid,cancelled,expired,failed")
                .OldAnnotation("Npgsql:Enum:payment.payment_transaction_status", "received,verified,rejected")
                .OldAnnotation("Npgsql:Enum:payment.payout_batch_status", "created,submitting,processing,partially_succeeded,succeeded,failed,cancelled")
                .OldAnnotation("Npgsql:Enum:payment.payout_item_status_enum", "created,processing,succeeded,failed,cancelled")
                .OldAnnotation("Npgsql:Enum:payment.processing_status_enum", "received,processing,succeeded,failed")
                .OldAnnotation("Npgsql:Enum:payment.provider_owner_scope", "platform,tenant")
                .OldAnnotation("Npgsql:Enum:platform.subscription_invoice_status", "draft,issued,partially_paid,paid,overdue,cancelled")
                .OldAnnotation("Npgsql:Enum:platform.subscription_payment_attempt_status", "pending,partially_paid,paid,cancelled,failed")
                .OldAnnotation("Npgsql:Enum:platform.subscription_status_enum", "trial,active,past_due,cancelled,expired")
                .OldAnnotation("Npgsql:Enum:platform.tenant_status_enum", "trial,active,suspended,closed")
                .OldAnnotation("Npgsql:Enum:security.role_scope_enum", "platform,tenant")
                .OldAnnotation("Npgsql:Enum:tenant.invitation_status_enum", "accepted,sent,revoked,expired,pending_delivery")
                .OldAnnotation("Npgsql:Enum:tenant.invoice_payment_attempt_status", "pending,partially_paid,paid,cancelled,failed")
                .OldAnnotation("Npgsql:Enum:tenant.payroll_disbursement_item_status", "ready,processing,succeeded,failed")
                .OldAnnotation("Npgsql:Enum:tenant.payroll_disbursement_status", "ready,issuing,processing,partially_succeeded,succeeded,failed")
                .OldAnnotation("Npgsql:Enum:tenant.payroll_status_enum", "draft,pending_approval,approved,rejected,published")
                .OldAnnotation("Npgsql:Enum:tenant.tenant_invoice_status", "draft,pending_approval,approved,rejected,published,paid,unpaid")
                .OldAnnotation("Npgsql:PostgresExtension:citext", ",,");

            migrationBuilder.CreateIndex(
                name: "IX_subscription_invoices_tenant_id_subscription_id",
                schema: "platform",
                table: "subscription_invoices",
                columns: new[] { "tenant_id", "subscription_id" });
        }
    }
}
