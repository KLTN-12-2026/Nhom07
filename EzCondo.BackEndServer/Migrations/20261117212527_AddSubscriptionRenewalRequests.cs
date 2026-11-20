using System;
using EzCondo.BackEndServer.Data.Enums;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EzCondo.BackEndServer.Migrations
{
    /// <inheritdoc />
    public partial class AddSubscriptionRenewalRequests : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:Enum:identity.user_status_enum", "active,locked,inviting")
                .Annotation("Npgsql:Enum:payment.payment_request_status", "pending,underpaid,paid,cancelled,expired,failed")
                .Annotation("Npgsql:Enum:payment.payment_transaction_status", "received,verified,rejected")
                .Annotation("Npgsql:Enum:payment.payout_batch_status", "created,submitting,processing,partially_succeeded,succeeded,failed,cancelled")
                .Annotation("Npgsql:Enum:payment.payout_item_status_enum", "created,processing,succeeded,failed,cancelled")
                .Annotation("Npgsql:Enum:payment.processing_status_enum", "received,processing,succeeded,failed")
                .Annotation("Npgsql:Enum:payment.provider_owner_scope", "platform,tenant")
                .Annotation("Npgsql:Enum:platform.subscription_invoice_status", "draft,issued,partially_paid,paid,overdue,cancelled")
                .Annotation("Npgsql:Enum:platform.subscription_payment_attempt_status", "pending,partially_paid,paid,cancelled,failed")
                .Annotation("Npgsql:Enum:platform.subscription_renewal_request_status", "awaiting_tenant_decision,tenant_declined,awaiting_platform_review,invoice_draft_created,invoice_issued,paid_awaiting_activation,activated,expired")
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

            migrationBuilder.CreateTable(
                name: "subscription_renewal_requests",
                schema: "platform",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: false),
                    subscription_id = table.Column<Guid>(type: "uuid", nullable: false),
                    current_ends_at_snapshot = table.Column<DateOnly>(type: "date", nullable: false),
                    status = table.Column<SubscriptionRenewalRequestStatus>(type: "platform.subscription_renewal_request_status", nullable: false),
                    selected_plan_id = table.Column<Guid>(type: "uuid", nullable: true),
                    plan_name_snapshot = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    billing_cycle_months_snapshot = table.Column<short>(type: "smallint", nullable: true),
                    price_snapshot = table.Column<decimal>(type: "numeric(18,2)", nullable: true),
                    currency_snapshot = table.Column<string>(type: "char(3)", nullable: true),
                    billing_period_from = table.Column<DateOnly>(type: "date", nullable: true),
                    billing_period_to = table.Column<DateOnly>(type: "date", nullable: true),
                    tenant_responded_by_user_id = table.Column<Guid>(type: "uuid", nullable: true),
                    tenant_responded_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true),
                    platform_reviewed_by_admin_id = table.Column<Guid>(type: "uuid", nullable: true),
                    platform_reviewed_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true),
                    subscription_invoice_id = table.Column<Guid>(type: "uuid", nullable: true),
                    activated_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_subscription_renewal_requests", x => x.id);
                    table.CheckConstraint("ck_subscription_renewal_requests_activation", "(\n	status = 'activated'\n	AND activated_at IS NOT NULL\n)\nOR\n(\n	status <> 'activated'\n	AND activated_at IS NULL\n)");
                    table.CheckConstraint("ck_subscription_renewal_requests_invoice", "status = 'expired'\nOR\n(\n	status IN (\n		'awaiting_tenant_decision',\n		'tenant_declined',\n		'awaiting_platform_review'\n	)\n	AND subscription_invoice_id IS NULL\n)\nOR\n(\n	status IN (\n		'invoice_draft_created',\n		'invoice_issued',\n		'paid_awaiting_activation',\n		'activated'\n	)\n	AND subscription_invoice_id IS NOT NULL\n)");
                    table.CheckConstraint("ck_subscription_renewal_requests_platform_review", "(\n	platform_reviewed_by_admin_id IS NULL\n	AND platform_reviewed_at IS NULL\n)\nOR\n(\n	platform_reviewed_by_admin_id IS NOT NULL\n	AND platform_reviewed_at IS NOT NULL\n)");
                    table.CheckConstraint("ck_subscription_renewal_requests_selection", "(\n	selected_plan_id IS NULL\n	AND plan_name_snapshot IS NULL\n	AND billing_cycle_months_snapshot IS NULL\n	AND price_snapshot IS NULL\n	AND currency_snapshot IS NULL\n	AND billing_period_from IS NULL\n	AND billing_period_to IS NULL\n)\nOR\n(\n	selected_plan_id IS NOT NULL\n	AND plan_name_snapshot IS NOT NULL\n	AND billing_cycle_months_snapshot IN (6, 12, 24)\n	AND price_snapshot > 0\n	AND currency_snapshot IS NOT NULL\n	AND billing_period_from IS NOT NULL\n	AND billing_period_to IS NOT NULL\n	AND billing_period_from = current_ends_at_snapshot\n	AND billing_period_to = (\n		billing_period_from\n		+ make_interval(\n			months => billing_cycle_months_snapshot\n		)\n	)::date\n)");
                    table.CheckConstraint("ck_subscription_renewal_requests_status_payload", "(\n	status = 'awaiting_tenant_decision'\n	AND tenant_responded_by_user_id IS NULL\n	AND selected_plan_id IS NULL\n	AND platform_reviewed_by_admin_id IS NULL\n	AND subscription_invoice_id IS NULL\n)\nOR\n(\n	status = 'tenant_declined'\n	AND tenant_responded_by_user_id IS NOT NULL\n	AND selected_plan_id IS NULL\n	AND platform_reviewed_by_admin_id IS NULL\n	AND subscription_invoice_id IS NULL\n)\nOR\n(\n	status = 'awaiting_platform_review'\n	AND tenant_responded_by_user_id IS NOT NULL\n	AND selected_plan_id IS NOT NULL\n	AND platform_reviewed_by_admin_id IS NULL\n	AND subscription_invoice_id IS NULL\n)\nOR\n(\n	status IN (\n		'invoice_draft_created',\n		'invoice_issued',\n		'paid_awaiting_activation',\n		'activated'\n	)\n	AND tenant_responded_by_user_id IS NOT NULL\n	AND selected_plan_id IS NOT NULL\n	AND platform_reviewed_by_admin_id IS NOT NULL\n	AND subscription_invoice_id IS NOT NULL\n)\nOR status = 'expired'");
                    table.CheckConstraint("ck_subscription_renewal_requests_tenant_response", "(\n	tenant_responded_by_user_id IS NULL\n	AND tenant_responded_at IS NULL\n)\nOR\n(\n	tenant_responded_by_user_id IS NOT NULL\n	AND tenant_responded_at IS NOT NULL\n)");
                    table.ForeignKey(
                        name: "FK_subscription_renewal_requests_subscription_invoices_subscri~",
                        column: x => x.subscription_invoice_id,
                        principalSchema: "platform",
                        principalTable: "subscription_invoices",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_subscription_renewal_requests_subscription_plans_selected_p~",
                        column: x => x.selected_plan_id,
                        principalSchema: "platform",
                        principalTable: "subscription_plans",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_subscription_renewal_requests_subscriptions_tenant_id_subsc~",
                        columns: x => new { x.tenant_id, x.subscription_id },
                        principalSchema: "platform",
                        principalTable: "subscriptions",
                        principalColumns: new[] { "tenant_id", "id" },
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_subscription_renewal_requests_tenants_tenant_id",
                        column: x => x.tenant_id,
                        principalSchema: "platform",
                        principalTable: "tenants",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_subscription_renewal_requests_users_platform_reviewed_by_ad~",
                        column: x => x.platform_reviewed_by_admin_id,
                        principalSchema: "identity",
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_subscription_renewal_requests_users_tenant_responded_by_use~",
                        column: x => x.tenant_responded_by_user_id,
                        principalSchema: "identity",
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "ix_subscription_renewal_requests_activation",
                schema: "platform",
                table: "subscription_renewal_requests",
                columns: new[] { "status", "billing_period_from" });

            migrationBuilder.CreateIndex(
                name: "IX_subscription_renewal_requests_platform_reviewed_by_admin_id",
                schema: "platform",
                table: "subscription_renewal_requests",
                column: "platform_reviewed_by_admin_id");

            migrationBuilder.CreateIndex(
                name: "IX_subscription_renewal_requests_selected_plan_id",
                schema: "platform",
                table: "subscription_renewal_requests",
                column: "selected_plan_id");

            migrationBuilder.CreateIndex(
                name: "IX_subscription_renewal_requests_tenant_responded_by_user_id",
                schema: "platform",
                table: "subscription_renewal_requests",
                column: "tenant_responded_by_user_id");

            migrationBuilder.CreateIndex(
                name: "ix_subscription_renewal_requests_tenant_status",
                schema: "platform",
                table: "subscription_renewal_requests",
                columns: new[] { "tenant_id", "status", "current_ends_at_snapshot" });

            migrationBuilder.CreateIndex(
                name: "ux_subscription_renewal_requests_invoice",
                schema: "platform",
                table: "subscription_renewal_requests",
                column: "subscription_invoice_id",
                unique: true,
                filter: "subscription_invoice_id IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "ux_subscription_renewal_requests_period",
                schema: "platform",
                table: "subscription_renewal_requests",
                columns: new[] { "tenant_id", "subscription_id", "current_ends_at_snapshot" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "subscription_renewal_requests",
                schema: "platform");

            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:Enum:identity.user_status_enum", "active,locked,inviting")
                .Annotation("Npgsql:Enum:payment.payment_request_status", "pending,underpaid,paid,cancelled,expired,failed")
                .Annotation("Npgsql:Enum:payment.payment_transaction_status", "received,verified,rejected")
                .Annotation("Npgsql:Enum:payment.payout_batch_status", "created,submitting,processing,partially_succeeded,succeeded,failed,cancelled")
                .Annotation("Npgsql:Enum:payment.payout_item_status_enum", "created,processing,succeeded,failed,cancelled")
                .Annotation("Npgsql:Enum:payment.processing_status_enum", "received,processing,succeeded,failed")
                .Annotation("Npgsql:Enum:payment.provider_owner_scope", "platform,tenant")
                .Annotation("Npgsql:Enum:platform.subscription_invoice_status", "draft,issued,partially_paid,paid,overdue,cancelled")
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
                .OldAnnotation("Npgsql:Enum:platform.subscription_renewal_request_status", "awaiting_tenant_decision,tenant_declined,awaiting_platform_review,invoice_draft_created,invoice_issued,paid_awaiting_activation,activated,expired")
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
        }
    }
}
