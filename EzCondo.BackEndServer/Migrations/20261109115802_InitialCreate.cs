using System;
using EzCondo.BackEndServer.Data.Enums;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace EzCondo.BackEndServer.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                CREATE TABLE IF NOT EXISTS public."__EFMigrationsHistoryLegacy_20261109" (
                    "MigrationId" character varying(150) NOT NULL,
                    "ProductVersion" character varying(32) NOT NULL,
                    CONSTRAINT "PK___EFMigrationsHistoryLegacy_20261109"
                        PRIMARY KEY ("MigrationId")
                );

                INSERT INTO public."__EFMigrationsHistoryLegacy_20261109" ("MigrationId", "ProductVersion")
                SELECT "MigrationId", "ProductVersion"
                FROM public."__EFMigrationsHistory"
                ON CONFLICT ("MigrationId") DO NOTHING;

                DELETE FROM public."__EFMigrationsHistory";
                """);

            // This database was used before the schema consolidation migration existed.
            // Preserve the legacy objects instead of dropping them, then build the new model
            // in the canonical schema names. On a fresh database this block is a no-op.
            migrationBuilder.Sql("""
                DO $migration$
                DECLARE
                    source_schema text;
                    legacy_schema text;
                BEGIN
                    FOREACH source_schema IN ARRAY ARRAY['identity', 'platform', 'tenant']
                    LOOP
                        legacy_schema := 'legacy_' || source_schema || '_20261109';

                        IF EXISTS (
                            SELECT 1 FROM pg_namespace WHERE nspname = source_schema
                        ) THEN
                            IF EXISTS (
                                SELECT 1 FROM pg_namespace WHERE nspname = legacy_schema
                            ) THEN
                                RAISE EXCEPTION
                                    'Cannot preserve schema % because target schema % already exists',
                                    source_schema,
                                    legacy_schema;
                            END IF;

                            EXECUTE format(
                                'ALTER SCHEMA %I RENAME TO %I',
                                source_schema,
                                legacy_schema);
                        END IF;
                    END LOOP;
                END
                $migration$;
                """);

            migrationBuilder.EnsureSchema(
                name: "tenant");

            migrationBuilder.EnsureSchema(
                name: "payment");

            migrationBuilder.EnsureSchema(
                name: "security");

            migrationBuilder.EnsureSchema(
                name: "platform");

            migrationBuilder.EnsureSchema(
                name: "identity");

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
                .Annotation("Npgsql:Enum:tenant.invitation_status_enum", "accepted,sent,revoked,expired")
                .Annotation("Npgsql:Enum:tenant.invoice_payment_attempt_status", "pending,partially_paid,paid,cancelled,failed")
                .Annotation("Npgsql:Enum:tenant.payroll_disbursement_item_status", "ready,processing,succeeded,failed")
                .Annotation("Npgsql:Enum:tenant.payroll_disbursement_status", "ready,issuing,processing,partially_succeeded,succeeded,failed")
                .Annotation("Npgsql:Enum:tenant.payroll_status_enum", "draft,pending_approval,approved,rejected,published")
                .Annotation("Npgsql:Enum:tenant.tenant_invoice_status", "draft,pending_approval,approved,rejected,published,paid,unpaid")
                .Annotation("Npgsql:PostgresExtension:citext", ",,");

            migrationBuilder.CreateTable(
                name: "permissions",
                schema: "security",
                columns: table => new
                {
                    scope = table.Column<RoleScopeEnum>(type: "security.role_scope_enum", nullable: false),
                    code = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    description = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_permissions", x => new { x.scope, x.code });
                    table.CheckConstraint("ck_permissions_code_scope", "(scope = 'platform' AND code LIKE 'platform.%') OR (scope = 'tenant' AND code LIKE 'tenant.%')");
                });

            migrationBuilder.CreateTable(
                name: "tenants",
                schema: "platform",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    code = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    slug = table.Column<string>(type: "citext", nullable: false),
                    legal_name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    tax_code = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    address = table.Column<string>(type: "text", nullable: false),
                    status = table.Column<TenantStatusEnum>(type: "platform.tenant_status_enum", nullable: false),
                    trial_ends_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tenants", x => x.id);
                    table.CheckConstraint("CK_tenants_slug_format", "slug ~ '^[a-z0-9]+(?:-[a-z0-9]+)*$'");
                    table.CheckConstraint("ck_tenants_trial", "status <> 'trial' OR trial_ends_at IS NOT NULL");
                });

            migrationBuilder.CreateTable(
                name: "provider_accounts",
                schema: "payment",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: true),
                    webhook_key = table.Column<Guid>(type: "uuid", nullable: false),
                    code = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    provider = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false, defaultValue: "payos"),
                    owner_scope = table.Column<ProviderOwnerScope>(type: "payment.provider_owner_scope", nullable: false),
                    merchant_code = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
                    payment_credentials_ref = table.Column<string>(type: "text", nullable: true),
                    payout_credentials_ref = table.Column<string>(type: "text", nullable: true),
                    supports_collection = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    supports_payout = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    settings = table.Column<string>(type: "jsonb", nullable: false, defaultValueSql: "'{}'::jsonb"),
                    is_active = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_provider_accounts", x => x.id);
                    table.UniqueConstraint("AK_provider_accounts_owner_scope_id", x => new { x.owner_scope, x.id });
                    table.CheckConstraint("ck_provider_accounts_capability", "(supports_collection OR supports_payout)");
                    table.CheckConstraint("ck_provider_accounts_owner", "((owner_scope = 'platform' AND tenant_id IS NULL) OR (owner_scope = 'tenant' AND tenant_id IS NOT NULL))");
                    table.CheckConstraint("ck_provider_accounts_payout_secret", "(NOT supports_payout OR payout_credentials_ref IS NOT NULL)");
                    table.CheckConstraint("ck_provider_accounts_provider", "(provider = 'payos')");
                    table.CheckConstraint("ck_provider_accounts_settings", "(jsonb_typeof(settings) = 'object')");
                    table.CheckConstraint("ck_provider_accounts_supports_collection_secret", "(NOT supports_collection OR payment_credentials_ref IS NOT NULL)");
                    table.ForeignKey(
                        name: "FK_provider_accounts_tenants_tenant_id",
                        column: x => x.tenant_id,
                        principalSchema: "platform",
                        principalTable: "tenants",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "roles",
                schema: "security",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    scope = table.Column<RoleScopeEnum>(type: "security.role_scope_enum", nullable: false),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: true),
                    code = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    name = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    is_system = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_roles", x => x.id);
                    table.UniqueConstraint("AK_roles_scope_id", x => new { x.scope, x.id });
                    table.CheckConstraint("ck_roles_owner_scope", "(scope = 'platform' AND tenant_id IS NULL) OR (scope = 'tenant' AND tenant_id IS NOT NULL)");
                    table.CheckConstraint("ck_roles_platform_code", "scope <> 'platform' OR code = 'PLATFORM_ADMIN'");
                    table.ForeignKey(
                        name: "FK_roles_tenants_tenant_id",
                        column: x => x.tenant_id,
                        principalSchema: "platform",
                        principalTable: "tenants",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "users",
                schema: "identity",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    full_name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    avatar_url = table.Column<string>(type: "text", nullable: true),
                    status = table.Column<UserStatusEnum>(type: "identity.user_status_enum", nullable: false),
                    citizen_id = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    employeement_contract = table.Column<string>(type: "text", nullable: true),
                    date_of_birth = table.Column<DateOnly>(type: "date", nullable: true),
                    bank_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    bank_account_number = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    deleted_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true, defaultValueSql: "now()"),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: true),
                    user_name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    normalized_user_name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    email = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    normalized_email = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    email_confirmed = table.Column<bool>(type: "boolean", nullable: false),
                    password_hash = table.Column<string>(type: "text", nullable: true),
                    security_stamp = table.Column<string>(type: "text", nullable: true),
                    concurrency_stamp = table.Column<string>(type: "text", nullable: true),
                    phone_number = table.Column<string>(type: "text", nullable: true),
                    phone_number_confirmed = table.Column<bool>(type: "boolean", nullable: false),
                    two_factor_enabled = table.Column<bool>(type: "boolean", nullable: false),
                    lockout_end = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    lockout_enabled = table.Column<bool>(type: "boolean", nullable: false),
                    access_failed_count = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_users", x => x.id);
                    table.CheckConstraint("CK_users_bank_name_and_bank_account_name", "(bank_name IS NULL AND bank_account_number IS NULL) OR (bank_name IS NOT NULL AND bank_account_number IS NOT NULL)");
                    table.CheckConstraint("CK_users_email_or_phone_number_not_null", "email IS NOT NULL OR phone_number IS NOT NULL");
                    table.CheckConstraint("CK_users_is_deleted_and_deleted_at", "(is_deleted = false AND deleted_at IS NULL) OR (is_deleted = true AND deleted_at IS NOT NULL)");
                    table.ForeignKey(
                        name: "FK_users_tenants_tenant_id",
                        column: x => x.tenant_id,
                        principalSchema: "platform",
                        principalTable: "tenants",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "payment_requests",
                schema: "payment",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    provider_account_id = table.Column<Guid>(type: "uuid", nullable: false),
                    order_code = table.Column<long>(type: "bigint", nullable: false),
                    provider_payment_link_id = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    status = table.Column<PaymentRequestStatus>(type: "payment.payment_request_status", nullable: false),
                    amount = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    amount_received = table.Column<decimal>(type: "numeric(18,2)", nullable: false, defaultValue: 0m),
                    currency = table.Column<string>(type: "char(3)", nullable: false, defaultValue: "VND"),
                    checkout_url = table.Column<string>(type: "text", nullable: true),
                    qr_code = table.Column<string>(type: "text", nullable: true),
                    return_url = table.Column<string>(type: "text", nullable: true),
                    cancel_url = table.Column<string>(type: "text", nullable: true),
                    expires_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true),
                    cancelled_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true),
                    cancellation_reason = table.Column<string>(type: "text", nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_payment_requests", x => x.id);
                    table.UniqueConstraint("AK_payment_requests_provider_account_id_id", x => new { x.provider_account_id, x.id });
                    table.CheckConstraint("ck_payment_requests_amount", "(amount > 0)");
                    table.CheckConstraint("ck_payment_requests_amount_received", "(amount_received >= 0)");
                    table.CheckConstraint("ck_payment_requests_integer_vnd", "(amount = trunc(amount) AND amount_received = trunc(amount_received))");
                    table.CheckConstraint("ck_payment_requests_order_code", "(order_code > 0)");
                    table.ForeignKey(
                        name: "FK_payment_requests_provider_accounts_provider_account_id",
                        column: x => x.provider_account_id,
                        principalSchema: "payment",
                        principalTable: "provider_accounts",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "payout_batches",
                schema: "payment",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    provider_account_id = table.Column<Guid>(type: "uuid", nullable: false),
                    reference_id = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    idempotency_key = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    provider_payout_id = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    status = table.Column<PayoutBatchStatus>(type: "payment.payout_batch_status", nullable: false),
                    provider_status = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: true),
                    total_amount = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    estimated_fee = table.Column<decimal>(type: "numeric(18,2)", nullable: true),
                    currency = table.Column<string>(type: "char(3)", nullable: false, defaultValue: "VND"),
                    last_synced_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true),
                    provider_payload = table.Column<string>(type: "jsonb", nullable: false, defaultValueSql: "'{}'::jsonb"),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_payout_batches", x => x.id);
                    table.UniqueConstraint("AK_payout_batches_provider_account_id_id", x => new { x.provider_account_id, x.id });
                    table.ForeignKey(
                        name: "FK_payout_batches_provider_accounts_provider_account_id",
                        column: x => x.provider_account_id,
                        principalSchema: "payment",
                        principalTable: "provider_accounts",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "role_permissions",
                schema: "security",
                columns: table => new
                {
                    scope = table.Column<RoleScopeEnum>(type: "security.role_scope_enum", nullable: false),
                    role_id = table.Column<Guid>(type: "uuid", nullable: false),
                    permission_code = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_role_permissions", x => new { x.scope, x.role_id, x.permission_code });
                    table.ForeignKey(
                        name: "FK_role_permissions_permissions_scope_permission_code",
                        columns: x => new { x.scope, x.permission_code },
                        principalSchema: "security",
                        principalTable: "permissions",
                        principalColumns: new[] { "scope", "code" },
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_role_permissions_roles_scope_role_id",
                        columns: x => new { x.scope, x.role_id },
                        principalSchema: "security",
                        principalTable: "roles",
                        principalColumns: new[] { "scope", "id" },
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "apartments",
                schema: "tenant",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    area_m2 = table.Column<decimal>(type: "numeric(10,2)", precision: 10, scale: 2, nullable: false),
                    apartment_no = table.Column<string>(type: "text", nullable: false),
                    bedroom = table.Column<short>(type: "smallint", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true, defaultValueSql: "now()"),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    deleted_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: false),
                    apartment_renter = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_apartments", x => x.id);
                    table.UniqueConstraint("AK_apartments_tenant_id_id", x => new { x.tenant_id, x.id });
                    table.CheckConstraint("CK_tenant_apartments_areaM2_positive", "area_m2 > 0");
                    table.CheckConstraint("CK_tenant_apartments_bedroom_positive", "bedroom >= 0");
                    table.ForeignKey(
                        name: "FK_apartments_tenants_tenant_id",
                        column: x => x.tenant_id,
                        principalSchema: "platform",
                        principalTable: "tenants",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_apartments_users_apartment_renter",
                        column: x => x.apartment_renter,
                        principalSchema: "identity",
                        principalTable: "users",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "complaints",
                schema: "tenant",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    message = table.Column<string>(type: "text", nullable: false),
                    resolved_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true, defaultValueSql: "now()"),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    deleted_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: false),
                    created_by_resident = table.Column<Guid>(type: "uuid", nullable: false),
                    received_by_staff = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_complaints", x => x.id);
                    table.ForeignKey(
                        name: "FK_complaints_tenants_tenant_id",
                        column: x => x.tenant_id,
                        principalSchema: "platform",
                        principalTable: "tenants",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_complaints_users_created_by_resident",
                        column: x => x.created_by_resident,
                        principalSchema: "identity",
                        principalTable: "users",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_complaints_users_received_by_staff",
                        column: x => x.received_by_staff,
                        principalSchema: "identity",
                        principalTable: "users",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "invitations",
                schema: "tenant",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    email = table.Column<string>(type: "citext", nullable: false),
                    token_hash = table.Column<string>(type: "text", nullable: false),
                    accepted_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true),
                    expires_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: false),
                    revoked_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true),
                    created_by = table.Column<Guid>(type: "uuid", nullable: false),
                    role_id = table.Column<Guid>(type: "uuid", nullable: false),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: false),
                    target_user_id = table.Column<Guid>(type: "uuid", nullable: true),
                    status = table.Column<InvitationStatusEnum>(type: "tenant.invitation_status_enum", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    deleted_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_invitations", x => x.id);
                    table.CheckConstraint("ck_invitations_accepted", "status <> 'accepted' OR accepted_at IS NOT NULL");
                    table.CheckConstraint("ck_invitations_expiry", "expires_at > created_at");
                    table.CheckConstraint("ck_invitations_revoked", "status <> 'revoked' OR revoked_at IS NOT NULL");
                    table.ForeignKey(
                        name: "FK_invitations_roles_role_id",
                        column: x => x.role_id,
                        principalSchema: "security",
                        principalTable: "roles",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_invitations_tenants_tenant_id",
                        column: x => x.tenant_id,
                        principalSchema: "platform",
                        principalTable: "tenants",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_invitations_users_created_by",
                        column: x => x.created_by,
                        principalSchema: "identity",
                        principalTable: "users",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_invitations_users_target_user_id",
                        column: x => x.target_user_id,
                        principalSchema: "identity",
                        principalTable: "users",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "payrolls",
                schema: "tenant",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    status = table.Column<PayrollStatusEnum>(type: "tenant.payroll_status_enum", nullable: false),
                    version = table.Column<int>(type: "integer", nullable: false, defaultValue: 1),
                    netpay = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    created_by = table.Column<Guid>(type: "uuid", nullable: false),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: false),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    deleted_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true, defaultValueSql: "now()"),
                    published_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_payrolls", x => x.id);
                    table.UniqueConstraint("AK_payrolls_tenant_id_id", x => new { x.tenant_id, x.id });
                    table.CheckConstraint("ck_payrolls_version", "version > 0");
                    table.CheckConstraint("CK_tenant_payrolls_netpay_positive", "netpay >= 0");
                    table.ForeignKey(
                        name: "FK_payrolls_tenants_tenant_id",
                        column: x => x.tenant_id,
                        principalSchema: "platform",
                        principalTable: "tenants",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_payrolls_users_created_by",
                        column: x => x.created_by,
                        principalSchema: "identity",
                        principalTable: "users",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "rules",
                schema: "tenant",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    title = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    source_url = table.Column<string>(type: "text", nullable: false),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: false),
                    imported_by = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_rules", x => x.id);
                    table.ForeignKey(
                        name: "FK_rules_tenants_tenant_id",
                        column: x => x.tenant_id,
                        principalSchema: "platform",
                        principalTable: "tenants",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_rules_users_imported_by",
                        column: x => x.imported_by,
                        principalSchema: "identity",
                        principalTable: "users",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "services",
                schema: "tenant",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    name = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    price = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    created_by_manager = table.Column<Guid>(type: "uuid", nullable: false),
                    deleted_by_manager = table.Column<Guid>(type: "uuid", nullable: true),
                    updated_by_manager = table.Column<Guid>(type: "uuid", nullable: true),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: false),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    deleted_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_services", x => x.id);
                    table.UniqueConstraint("AK_services_tenant_id_id", x => new { x.tenant_id, x.id });
                    table.ForeignKey(
                        name: "FK_services_tenants_tenant_id",
                        column: x => x.tenant_id,
                        principalSchema: "platform",
                        principalTable: "tenants",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_services_users_created_by_manager",
                        column: x => x.created_by_manager,
                        principalSchema: "identity",
                        principalTable: "users",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_services_users_deleted_by_manager",
                        column: x => x.deleted_by_manager,
                        principalSchema: "identity",
                        principalTable: "users",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_services_users_updated_by_manager",
                        column: x => x.updated_by_manager,
                        principalSchema: "identity",
                        principalTable: "users",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "staff_time_sheets",
                schema: "tenant",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    payroll_period = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    standard_working_day = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    paid_leave_day = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    unpaid_leave_day = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    overtime_hour_normal = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    overtime_hour_weekend = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    overtime_hour_holiday = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    bonus_amount = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    penalty_amount = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    note = table.Column<string>(type: "text", nullable: true),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    deleted_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true, defaultValueSql: "now()"),
                    created_by_manager = table.Column<Guid>(type: "uuid", nullable: false),
                    staff_id = table.Column<Guid>(type: "uuid", nullable: false),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_staff_time_sheets", x => x.id);
                    table.CheckConstraint("CK_tenant_staff_time_sheets_bonus_amount_positive", "bonus_amount >= 0");
                    table.CheckConstraint("CK_tenant_staff_time_sheets_overtime_hour_holiday_positive", "overtime_hour_holiday >= 0");
                    table.CheckConstraint("CK_tenant_staff_time_sheets_overtime_hour_normal_positive", "overtime_hour_normal >= 0");
                    table.CheckConstraint("CK_tenant_staff_time_sheets_overtime_hour_weekend_positive", "overtime_hour_weekend >= 0");
                    table.CheckConstraint("CK_tenant_staff_time_sheets_paid_leave_day_positive", "paid_leave_day >= 0");
                    table.CheckConstraint("CK_tenant_staff_time_sheets_penalty_amount_positive", "penalty_amount >= 0");
                    table.CheckConstraint("CK_tenant_staff_time_sheets_standard_working_day_positive", "standard_working_day >= 0 AND standard_working_day <= 30");
                    table.CheckConstraint("CK_tenant_staff_time_sheets_unpaid_leave_day_positive", "unpaid_leave_day >= 0");
                    table.ForeignKey(
                        name: "FK_staff_time_sheets_tenants_tenant_id",
                        column: x => x.tenant_id,
                        principalSchema: "platform",
                        principalTable: "tenants",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_staff_time_sheets_users_created_by_manager",
                        column: x => x.created_by_manager,
                        principalSchema: "identity",
                        principalTable: "users",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_staff_time_sheets_users_staff_id",
                        column: x => x.staff_id,
                        principalSchema: "identity",
                        principalTable: "users",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "subscription_plans",
                schema: "platform",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    description = table.Column<string>(type: "text", nullable: true),
                    billing_cycle_months = table.Column<short>(type: "smallint", nullable: false),
                    price = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    feature_limits = table.Column<string>(type: "jsonb", nullable: false, defaultValueSql: "'{}'::jsonb"),
                    is_available = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    created_by_admin = table.Column<Guid>(type: "uuid", nullable: false),
                    updated_by_admin = table.Column<Guid>(type: "uuid", nullable: true),
                    deleted_by_admin = table.Column<Guid>(type: "uuid", nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true, defaultValueSql: "now()"),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    deleted_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_subscription_plans", x => x.id);
                    table.CheckConstraint("ck_subscription_plans_billing_cycle", "billing_cycle_months IN (6, 12, 24)");
                    table.CheckConstraint("CK_subscription_plans_price", "price >= 0");
                    table.ForeignKey(
                        name: "FK_subscription_plans_users_created_by_admin",
                        column: x => x.created_by_admin,
                        principalSchema: "identity",
                        principalTable: "users",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_subscription_plans_users_deleted_by_admin",
                        column: x => x.deleted_by_admin,
                        principalSchema: "identity",
                        principalTable: "users",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_subscription_plans_users_updated_by_admin",
                        column: x => x.updated_by_admin,
                        principalSchema: "identity",
                        principalTable: "users",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "user_claims",
                schema: "identity",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    ClaimType = table.Column<string>(type: "text", nullable: true),
                    ClaimValue = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_user_claims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_user_claims_users_UserId",
                        column: x => x.UserId,
                        principalSchema: "identity",
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "user_logins",
                schema: "identity",
                columns: table => new
                {
                    LoginProvider = table.Column<string>(type: "text", nullable: false),
                    ProviderKey = table.Column<string>(type: "text", nullable: false),
                    ProviderDisplayName = table.Column<string>(type: "text", nullable: true),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_user_logins", x => new { x.LoginProvider, x.ProviderKey });
                    table.ForeignKey(
                        name: "FK_user_logins_users_UserId",
                        column: x => x.UserId,
                        principalSchema: "identity",
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "user_roles",
                schema: "security",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    scope = table.Column<RoleScopeEnum>(type: "security.role_scope_enum", nullable: false),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: true),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    role_id = table.Column<Guid>(type: "uuid", nullable: false),
                    granted_by_user_id = table.Column<Guid>(type: "uuid", nullable: true),
                    granted_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: false),
                    revoked_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_user_roles", x => x.id);
                    table.CheckConstraint("ck_user_roles_revoked_at", "revoked_at IS NULL OR revoked_at >= granted_at");
                    table.CheckConstraint("ck_user_roles_scope_tenant", "(scope = 'platform' AND tenant_id IS NULL) OR (scope = 'tenant' AND tenant_id IS NOT NULL)");
                    table.ForeignKey(
                        name: "FK_user_roles_roles_scope_role_id",
                        columns: x => new { x.scope, x.role_id },
                        principalSchema: "security",
                        principalTable: "roles",
                        principalColumns: new[] { "scope", "id" },
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_user_roles_tenants_tenant_id",
                        column: x => x.tenant_id,
                        principalSchema: "platform",
                        principalTable: "tenants",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_user_roles_users_granted_by_user_id",
                        column: x => x.granted_by_user_id,
                        principalSchema: "identity",
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_user_roles_users_user_id",
                        column: x => x.user_id,
                        principalSchema: "identity",
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "user_tokens",
                schema: "identity",
                columns: table => new
                {
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    LoginProvider = table.Column<string>(type: "text", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Value = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_user_tokens", x => new { x.UserId, x.LoginProvider, x.Name });
                    table.ForeignKey(
                        name: "FK_user_tokens_users_UserId",
                        column: x => x.UserId,
                        principalSchema: "identity",
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "webhook_events",
                schema: "payment",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    provider_account_id = table.Column<Guid>(type: "uuid", nullable: false),
                    payment_request_id = table.Column<Guid>(type: "uuid", nullable: true),
                    event_key = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    order_code = table.Column<long>(type: "bigint", nullable: true),
                    provider_payment_link_id = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    provider_reference = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    signature_valid = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    raw_payload = table.Column<string>(type: "text", nullable: false),
                    processing_status = table.Column<ProcessingStatusEnum>(type: "payment.processing_status_enum", nullable: false),
                    processing_attempts = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    last_attempt_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true),
                    next_attempt_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true),
                    received_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: false, defaultValueSql: "now()"),
                    processed_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true),
                    processing_error = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_webhook_events", x => x.id);
                    table.UniqueConstraint("AK_webhook_events_provider_account_id_id", x => new { x.provider_account_id, x.id });
                    table.CheckConstraint("ck_webhook_event_attempt", "processing_attempts >= 0");
                    table.CheckConstraint("ck_webhook_event_event_key", "(event_key IS NULL OR (signature_valid = true AND btrim(event_key) <> ''))");
                    table.CheckConstraint("ck_webhook_event_processed", "(processed_at IS NULL OR (signature_valid = true AND processing_status = 'succeeded' AND payment_request_id IS NOT NULL AND processing_error IS NULL))");
                    table.CheckConstraint("ck_webhook_event_resolved_request", "(payment_request_id IS NULL OR signature_valid = true)");
                    table.ForeignKey(
                        name: "FK_webhook_events_payment_requests_provider_account_id_payment~",
                        columns: x => new { x.provider_account_id, x.payment_request_id },
                        principalSchema: "payment",
                        principalTable: "payment_requests",
                        principalColumns: new[] { "provider_account_id", "id" },
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_webhook_events_provider_accounts_provider_account_id",
                        column: x => x.provider_account_id,
                        principalSchema: "payment",
                        principalTable: "provider_accounts",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "payout_items",
                schema: "payment",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    provider_account_id = table.Column<Guid>(type: "uuid", nullable: false),
                    payout_batch_id = table.Column<Guid>(type: "uuid", nullable: false),
                    reference_id = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    provider_transaction_id = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    amount = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    currency = table.Column<string>(type: "char(3)", nullable: false, defaultValue: "VND"),
                    description = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    destination_bin = table.Column<string>(type: "character varying(12)", maxLength: 12, nullable: false),
                    destination_account_ciphertext = table.Column<byte[]>(type: "bytea", nullable: false),
                    destination_account_last4 = table.Column<string>(type: "character varying(4)", maxLength: 4, nullable: false),
                    destination_account_name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    status = table.Column<PayoutItemStatusEnum>(type: "payment.payout_item_status_enum", nullable: false),
                    provider_status = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: true),
                    failure_code = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: true),
                    failure_message = table.Column<string>(type: "text", nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_payout_items", x => x.id);
                    table.UniqueConstraint("AK_payout_items_provider_account_id_id", x => new { x.provider_account_id, x.id });
                    table.CheckConstraint("ck_payout_items_amount", "(amount > 0)");
                    table.CheckConstraint("ck_payout_items_currency", "(currency = 'VND')");
                    table.CheckConstraint("ck_payout_items_failure", "status <> 'failed' OR failure_message IS NOT NULL OR failure_code IS NOT NULL");
                    table.CheckConstraint("ck_payout_items_last4", "(destination_account_last4 ~ '^[0-9]{4}$')");
                    table.ForeignKey(
                        name: "FK_payout_items_payout_batches_provider_account_id_payout_batc~",
                        columns: x => new { x.provider_account_id, x.payout_batch_id },
                        principalSchema: "payment",
                        principalTable: "payout_batches",
                        principalColumns: new[] { "provider_account_id", "id" },
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_payout_items_provider_accounts_provider_account_id",
                        column: x => x.provider_account_id,
                        principalSchema: "payment",
                        principalTable: "provider_accounts",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "contracts",
                schema: "tenant",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    contract_no = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    contract_url = table.Column<string>(type: "text", nullable: true),
                    starts_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: false),
                    ends_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: false),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: false),
                    imported_by = table.Column<Guid>(type: "uuid", nullable: false),
                    apartment_id = table.Column<Guid>(type: "uuid", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true, defaultValueSql: "now()"),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    deleted_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_contracts", x => x.id);
                    table.CheckConstraint("ck_contracts_period", "ends_at > starts_at");
                    table.ForeignKey(
                        name: "FK_contracts_apartments_tenant_id_apartment_id",
                        columns: x => new { x.tenant_id, x.apartment_id },
                        principalSchema: "tenant",
                        principalTable: "apartments",
                        principalColumns: new[] { "tenant_id", "id" });
                    table.ForeignKey(
                        name: "FK_contracts_tenants_tenant_id",
                        column: x => x.tenant_id,
                        principalSchema: "platform",
                        principalTable: "tenants",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_contracts_users_imported_by",
                        column: x => x.imported_by,
                        principalSchema: "identity",
                        principalTable: "users",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "invoices",
                schema: "tenant",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    invoice_no = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    status = table.Column<TenantInvoiceStatus>(type: "tenant.tenant_invoice_status", nullable: false),
                    published_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true),
                    billing_period = table.Column<DateTimeOffset>(type: "timestamptz", nullable: false),
                    payment_deadline_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: false),
                    total_amount = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    note = table.Column<string>(type: "text", nullable: true),
                    deleted_by_staff = table.Column<Guid>(type: "uuid", nullable: true),
                    updated_by_staff = table.Column<Guid>(type: "uuid", nullable: true),
                    created_by_staff = table.Column<Guid>(type: "uuid", nullable: false),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: false),
                    apartment_id = table.Column<Guid>(type: "uuid", nullable: false),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    deleted_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_invoices", x => x.id);
                    table.UniqueConstraint("AK_invoices_tenant_id_id", x => new { x.tenant_id, x.id });
                    table.ForeignKey(
                        name: "FK_invoices_apartments_tenant_id_apartment_id",
                        columns: x => new { x.tenant_id, x.apartment_id },
                        principalSchema: "tenant",
                        principalTable: "apartments",
                        principalColumns: new[] { "tenant_id", "id" });
                    table.ForeignKey(
                        name: "FK_invoices_tenants_tenant_id",
                        column: x => x.tenant_id,
                        principalSchema: "platform",
                        principalTable: "tenants",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_invoices_users_created_by_staff",
                        column: x => x.created_by_staff,
                        principalSchema: "identity",
                        principalTable: "users",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_invoices_users_deleted_by_staff",
                        column: x => x.deleted_by_staff,
                        principalSchema: "identity",
                        principalTable: "users",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_invoices_users_updated_by_staff",
                        column: x => x.updated_by_staff,
                        principalSchema: "identity",
                        principalTable: "users",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "payroll_details",
                schema: "tenant",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    base_salary = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    allowance = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    deduction = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    staff_receive = table.Column<Guid>(type: "uuid", nullable: false),
                    payroll_id = table.Column<Guid>(type: "uuid", nullable: false),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_payroll_details", x => x.id);
                    table.UniqueConstraint("AK_payroll_details_tenant_id_id", x => new { x.tenant_id, x.id });
                    table.CheckConstraint("CK_tenant_payroll_details_allowance_positive", "allowance >= 0");
                    table.CheckConstraint("CK_tenant_payroll_details_base_salary_positive", "base_salary > 0");
                    table.CheckConstraint("CK_tenant_payroll_details_deduction_positive", "deduction >= 0");
                    table.ForeignKey(
                        name: "FK_payroll_details_payrolls_tenant_id_payroll_id",
                        columns: x => new { x.tenant_id, x.payroll_id },
                        principalSchema: "tenant",
                        principalTable: "payrolls",
                        principalColumns: new[] { "tenant_id", "id" });
                    table.ForeignKey(
                        name: "FK_payroll_details_tenants_tenant_id",
                        column: x => x.tenant_id,
                        principalSchema: "platform",
                        principalTable: "tenants",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_payroll_details_users_staff_receive",
                        column: x => x.staff_receive,
                        principalSchema: "identity",
                        principalTable: "users",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "payroll_disbursements",
                schema: "tenant",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: false),
                    payroll_id = table.Column<Guid>(type: "uuid", nullable: false),
                    gateway_account_id = table.Column<Guid>(type: "uuid", nullable: false),
                    gateway_payout_batch_id = table.Column<Guid>(type: "uuid", nullable: true),
                    approved_payroll_version = table.Column<int>(type: "integer", nullable: false),
                    approved_content_hash = table.Column<byte[]>(type: "bytea", nullable: false),
                    approved_by_user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    approved_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: false),
                    issued_by_user_id = table.Column<Guid>(type: "uuid", nullable: true),
                    issued_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true),
                    status = table.Column<PayrollDisbursementStatus>(type: "tenant.payroll_disbursement_status", nullable: false),
                    total_amount = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_payroll_disbursements", x => x.id);
                    table.UniqueConstraint("AK_payroll_disbursements_tenant_id_id", x => new { x.tenant_id, x.id });
                    table.CheckConstraint("ck_payroll_disbursement_amount", "(total_amount > 0)");
                    table.CheckConstraint("ck_payroll_disbursement_hash", "(octet_length(approved_content_hash) = 32)");
                    table.CheckConstraint("ck_payroll_disbursement_issued", "((issued_at IS NULL AND issued_by_user_id IS NULL) OR (issued_at IS NOT NULL AND issued_by_user_id IS NOT NULL))");
                    table.CheckConstraint("ck_payroll_disbursement_status_issued", "status = 'ready' OR issued_at IS NOT NULL");
                    table.CheckConstraint("ck_payroll_disbursement_version", "(approved_payroll_version > 0)");
                    table.ForeignKey(
                        name: "FK_payroll_disbursements_payout_batches_gateway_account_id_gat~",
                        columns: x => new { x.gateway_account_id, x.gateway_payout_batch_id },
                        principalSchema: "payment",
                        principalTable: "payout_batches",
                        principalColumns: new[] { "provider_account_id", "id" },
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_payroll_disbursements_payrolls_tenant_id_payroll_id",
                        columns: x => new { x.tenant_id, x.payroll_id },
                        principalSchema: "tenant",
                        principalTable: "payrolls",
                        principalColumns: new[] { "tenant_id", "id" },
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_payroll_disbursements_provider_accounts_gateway_account_id",
                        column: x => x.gateway_account_id,
                        principalSchema: "payment",
                        principalTable: "provider_accounts",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_payroll_disbursements_tenants_tenant_id",
                        column: x => x.tenant_id,
                        principalSchema: "platform",
                        principalTable: "tenants",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_payroll_disbursements_users_approved_by_user_id",
                        column: x => x.approved_by_user_id,
                        principalSchema: "identity",
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_payroll_disbursements_users_issued_by_user_id",
                        column: x => x.issued_by_user_id,
                        principalSchema: "identity",
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "subscriptions",
                schema: "platform",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: false),
                    plan_id = table.Column<Guid>(type: "uuid", nullable: false),
                    billing_cycle_months_snapshot = table.Column<short>(type: "smallint", nullable: false),
                    status = table.Column<SubscriptionStatusEnum>(type: "platform.subscription_status_enum", nullable: false),
                    starts_at = table.Column<DateOnly>(type: "date", nullable: false),
                    ends_at = table.Column<DateOnly>(type: "date", nullable: false),
                    cancelled_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_subscriptions", x => x.id);
                    table.UniqueConstraint("AK_subscriptions_tenant_id_id", x => new { x.tenant_id, x.id });
                    table.CheckConstraint("ck_subscriptions_billing_cycle", "billing_cycle_months_snapshot > 0");
                    table.CheckConstraint("ck_subscriptions_cancelled", "status <> 'cancelled' OR cancelled_at IS NOT NULL");
                    table.CheckConstraint("ck_subscriptions_period", "ends_at > starts_at");
                    table.ForeignKey(
                        name: "FK_subscriptions_subscription_plans_plan_id",
                        column: x => x.plan_id,
                        principalSchema: "platform",
                        principalTable: "subscription_plans",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_subscriptions_tenants_tenant_id",
                        column: x => x.tenant_id,
                        principalSchema: "platform",
                        principalTable: "tenants",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "payment_transactions",
                schema: "payment",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    provider_account_id = table.Column<Guid>(type: "uuid", nullable: false),
                    payment_request_id = table.Column<Guid>(type: "uuid", nullable: true),
                    source_webhook_event_id = table.Column<Guid>(type: "uuid", nullable: true),
                    provider_reference = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    provider_transaction_id = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    amount = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    occurred_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: false),
                    payer_masked = table.Column<string>(type: "jsonb", nullable: false, defaultValueSql: "'{}'::jsonb"),
                    provider_payload = table.Column<string>(type: "jsonb", nullable: false, defaultValueSql: "'{}'::jsonb"),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    currency = table.Column<string>(type: "char(3)", nullable: false, defaultValue: "VND"),
                    status = table.Column<PaymentTransactionStatus>(type: "payment.payment_transaction_status", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_payment_transactions", x => x.id);
                    table.UniqueConstraint("AK_payment_transactions_provider_account_id_id", x => new { x.provider_account_id, x.id });
                    table.CheckConstraint("ck_payment_transaction_amount", "(amount > 0)");
                    table.CheckConstraint("ck_payment_transaction_verified", "status <> 'verified' OR payment_request_id IS NOT NULL");
                    table.ForeignKey(
                        name: "FK_payment_transactions_payment_requests_provider_account_id_p~",
                        columns: x => new { x.provider_account_id, x.payment_request_id },
                        principalSchema: "payment",
                        principalTable: "payment_requests",
                        principalColumns: new[] { "provider_account_id", "id" },
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_payment_transactions_provider_accounts_provider_account_id",
                        column: x => x.provider_account_id,
                        principalSchema: "payment",
                        principalTable: "provider_accounts",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_payment_transactions_webhook_events_provider_account_id_sou~",
                        columns: x => new { x.provider_account_id, x.source_webhook_event_id },
                        principalSchema: "payment",
                        principalTable: "webhook_events",
                        principalColumns: new[] { "provider_account_id", "id" },
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "invoice_details",
                schema: "tenant",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    quantity = table.Column<int>(type: "integer", nullable: false),
                    unit_price = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    amount = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    description = table.Column<string>(type: "text", nullable: true),
                    invoice_id = table.Column<Guid>(type: "uuid", nullable: false),
                    service_id = table.Column<Guid>(type: "uuid", nullable: false),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_invoice_details", x => x.id);
                    table.CheckConstraint("ck_invoice_details_amount", "amount >= 0 AND amount = quantity * unit_price");
                    table.CheckConstraint("ck_invoice_details_quantity", "quantity > 0");
                    table.CheckConstraint("ck_invoice_details_unit_price", "unit_price >= 0");
                    table.ForeignKey(
                        name: "FK_invoice_details_invoices_tenant_id_invoice_id",
                        columns: x => new { x.tenant_id, x.invoice_id },
                        principalSchema: "tenant",
                        principalTable: "invoices",
                        principalColumns: new[] { "tenant_id", "id" },
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_invoice_details_services_tenant_id_service_id",
                        columns: x => new { x.tenant_id, x.service_id },
                        principalSchema: "tenant",
                        principalTable: "services",
                        principalColumns: new[] { "tenant_id", "id" });
                    table.ForeignKey(
                        name: "FK_invoice_details_tenants_tenant_id",
                        column: x => x.tenant_id,
                        principalSchema: "platform",
                        principalTable: "tenants",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "invoice_payment_attempts",
                schema: "tenant",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: false),
                    invoice_id = table.Column<Guid>(type: "uuid", nullable: false),
                    gateway_account_id = table.Column<Guid>(type: "uuid", nullable: false),
                    gateway_payment_request_id = table.Column<Guid>(type: "uuid", nullable: false),
                    requested_by_user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    status = table.Column<InvoicePaymentAttemptStatus>(type: "tenant.invoice_payment_attempt_status", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_invoice_payment_attempts", x => x.id);
                    table.UniqueConstraint("AK_invoice_payment_attempts_tenant_id_gateway_account_id_id", x => new { x.tenant_id, x.gateway_account_id, x.id });
                    table.ForeignKey(
                        name: "FK_invoice_payment_attempts_invoices_tenant_id_invoice_id",
                        columns: x => new { x.tenant_id, x.invoice_id },
                        principalSchema: "tenant",
                        principalTable: "invoices",
                        principalColumns: new[] { "tenant_id", "id" },
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_invoice_payment_attempts_payment_requests_gateway_account_i~",
                        columns: x => new { x.gateway_account_id, x.gateway_payment_request_id },
                        principalSchema: "payment",
                        principalTable: "payment_requests",
                        principalColumns: new[] { "provider_account_id", "id" },
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_invoice_payment_attempts_provider_accounts_gateway_account_~",
                        column: x => x.gateway_account_id,
                        principalSchema: "payment",
                        principalTable: "provider_accounts",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_invoice_payment_attempts_tenants_tenant_id",
                        column: x => x.tenant_id,
                        principalSchema: "platform",
                        principalTable: "tenants",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_invoice_payment_attempts_users_requested_by_user_id",
                        column: x => x.requested_by_user_id,
                        principalSchema: "identity",
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "payroll_disbursement_items",
                schema: "tenant",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: false),
                    payroll_disbursement_id = table.Column<Guid>(type: "uuid", nullable: false),
                    payroll_detail_id = table.Column<Guid>(type: "uuid", nullable: false),
                    staff_user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    gateway_account_id = table.Column<Guid>(type: "uuid", nullable: false),
                    gateway_payout_item_id = table.Column<Guid>(type: "uuid", nullable: true),
                    amount = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    destination_bin = table.Column<string>(type: "character varying(12)", maxLength: 12, nullable: false),
                    destination_account_ciphertext = table.Column<byte[]>(type: "bytea", nullable: false),
                    destination_account_last4 = table.Column<string>(type: "character varying(4)", maxLength: 4, nullable: false),
                    recipient_name_snapshot = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    status = table.Column<PayrollDisbursementItemStatus>(type: "tenant.payroll_disbursement_item_status", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_payroll_disbursement_items", x => x.id);
                    table.CheckConstraint("ck_payroll_item_amount", "(amount > 0)");
                    table.ForeignKey(
                        name: "FK_payroll_disbursement_items_payout_items_gateway_account_id_~",
                        columns: x => new { x.gateway_account_id, x.gateway_payout_item_id },
                        principalSchema: "payment",
                        principalTable: "payout_items",
                        principalColumns: new[] { "provider_account_id", "id" },
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_payroll_disbursement_items_payroll_details_tenant_id_payrol~",
                        columns: x => new { x.tenant_id, x.payroll_detail_id },
                        principalSchema: "tenant",
                        principalTable: "payroll_details",
                        principalColumns: new[] { "tenant_id", "id" },
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_payroll_disbursement_items_payroll_disbursements_tenant_id_~",
                        columns: x => new { x.tenant_id, x.payroll_disbursement_id },
                        principalSchema: "tenant",
                        principalTable: "payroll_disbursements",
                        principalColumns: new[] { "tenant_id", "id" },
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_payroll_disbursement_items_provider_accounts_gateway_accoun~",
                        column: x => x.gateway_account_id,
                        principalSchema: "payment",
                        principalTable: "provider_accounts",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_payroll_disbursement_items_tenants_tenant_id",
                        column: x => x.tenant_id,
                        principalSchema: "platform",
                        principalTable: "tenants",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_payroll_disbursement_items_users_staff_user_id",
                        column: x => x.staff_user_id,
                        principalSchema: "identity",
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "subscription_invoices",
                schema: "platform",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: false),
                    subscription_id = table.Column<Guid>(type: "uuid", nullable: false),
                    invoice_no = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    billing_period_from = table.Column<DateOnly>(type: "date", nullable: true),
                    billing_period_to = table.Column<DateOnly>(type: "date", nullable: true),
                    due_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: false),
                    status = table.Column<SubscriptionInvoiceStatus>(type: "platform.subscription_invoice_status", nullable: false),
                    total_amount = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    paid_amount = table.Column<decimal>(type: "numeric(18,2)", nullable: false, defaultValue: 0m),
                    currency = table.Column<string>(type: "char(3)", nullable: false, defaultValue: "VND"),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_subscription_invoices", x => x.id);
                    table.CheckConstraint("ck_subscription_invoices_amount", "(total_amount > 0 AND paid_amount >= 0 AND paid_amount <= total_amount)");
                    table.CheckConstraint("ck_subscription_invoices_billing_period", "(billing_period_from IS NULL AND billing_period_to IS NULL) OR (billing_period_from IS NOT NULL AND billing_period_to IS NOT NULL AND billing_period_to > billing_period_from)");
                    table.ForeignKey(
                        name: "FK_subscription_invoices_subscriptions_tenant_id_subscription_~",
                        columns: x => new { x.tenant_id, x.subscription_id },
                        principalSchema: "platform",
                        principalTable: "subscriptions",
                        principalColumns: new[] { "tenant_id", "id" },
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_subscription_invoices_tenants_tenant_id",
                        column: x => x.tenant_id,
                        principalSchema: "platform",
                        principalTable: "tenants",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "invoice_payment_allocations",
                schema: "tenant",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: false),
                    payment_attempt_id = table.Column<Guid>(type: "uuid", nullable: false),
                    gateway_account_id = table.Column<Guid>(type: "uuid", nullable: false),
                    gateway_transaction_id = table.Column<Guid>(type: "uuid", nullable: false),
                    amount_applied = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_invoice_payment_allocations", x => x.id);
                    table.CheckConstraint("ck_invoice_allocation_amount", "(amount_applied > 0)");
                    table.ForeignKey(
                        name: "FK_invoice_payment_allocations_invoice_payment_attempts_tenant~",
                        columns: x => new { x.tenant_id, x.gateway_account_id, x.payment_attempt_id },
                        principalSchema: "tenant",
                        principalTable: "invoice_payment_attempts",
                        principalColumns: new[] { "tenant_id", "gateway_account_id", "id" },
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_invoice_payment_allocations_payment_transactions_gateway_ac~",
                        columns: x => new { x.gateway_account_id, x.gateway_transaction_id },
                        principalSchema: "payment",
                        principalTable: "payment_transactions",
                        principalColumns: new[] { "provider_account_id", "id" },
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_invoice_payment_allocations_provider_accounts_gateway_accou~",
                        column: x => x.gateway_account_id,
                        principalSchema: "payment",
                        principalTable: "provider_accounts",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_invoice_payment_allocations_tenants_tenant_id",
                        column: x => x.tenant_id,
                        principalSchema: "platform",
                        principalTable: "tenants",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "subscription_payment_attempts",
                schema: "platform",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    subscription_invoice_id = table.Column<Guid>(type: "uuid", nullable: false),
                    gateway_owner_scope = table.Column<ProviderOwnerScope>(type: "payment.provider_owner_scope", nullable: false),
                    gateway_account_id = table.Column<Guid>(type: "uuid", nullable: false),
                    gateway_payment_request_id = table.Column<Guid>(type: "uuid", nullable: false),
                    requested_by_user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    status = table.Column<SubscriptionPaymentAttemptStatus>(type: "platform.subscription_payment_attempt_status", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_subscription_payment_attempts", x => x.id);
                    table.UniqueConstraint("AK_subscription_payment_attempts_gateway_account_id_id", x => new { x.gateway_account_id, x.id });
                    table.CheckConstraint("ck_subscription_attempt_scope", "(gateway_owner_scope = 'platform')");
                    table.ForeignKey(
                        name: "FK_subscription_payment_attempts_payment_requests_gateway_acco~",
                        columns: x => new { x.gateway_account_id, x.gateway_payment_request_id },
                        principalSchema: "payment",
                        principalTable: "payment_requests",
                        principalColumns: new[] { "provider_account_id", "id" },
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_subscription_payment_attempts_provider_accounts_gateway_own~",
                        columns: x => new { x.gateway_owner_scope, x.gateway_account_id },
                        principalSchema: "payment",
                        principalTable: "provider_accounts",
                        principalColumns: new[] { "owner_scope", "id" },
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_subscription_payment_attempts_subscription_invoices_subscri~",
                        column: x => x.subscription_invoice_id,
                        principalSchema: "platform",
                        principalTable: "subscription_invoices",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_subscription_payment_attempts_users_requested_by_user_id",
                        column: x => x.requested_by_user_id,
                        principalSchema: "identity",
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "subscription_payment_allocations",
                schema: "platform",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    payment_attempt_id = table.Column<Guid>(type: "uuid", nullable: false),
                    gateway_account_id = table.Column<Guid>(type: "uuid", nullable: false),
                    gateway_transaction_id = table.Column<Guid>(type: "uuid", nullable: false),
                    amount_applied = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_subscription_payment_allocations", x => x.id);
                    table.CheckConstraint("ck_subscription_allocation_amount", "(amount_applied > 0)");
                    table.ForeignKey(
                        name: "FK_subscription_payment_allocations_payment_transactions_gatew~",
                        columns: x => new { x.gateway_account_id, x.gateway_transaction_id },
                        principalSchema: "payment",
                        principalTable: "payment_transactions",
                        principalColumns: new[] { "provider_account_id", "id" },
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_subscription_payment_allocations_provider_accounts_gateway_~",
                        column: x => x.gateway_account_id,
                        principalSchema: "payment",
                        principalTable: "provider_accounts",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_subscription_payment_allocations_subscription_payment_attem~",
                        columns: x => new { x.gateway_account_id, x.payment_attempt_id },
                        principalSchema: "platform",
                        principalTable: "subscription_payment_attempts",
                        principalColumns: new[] { "gateway_account_id", "id" },
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_apartments_apartment_renter",
                schema: "tenant",
                table: "apartments",
                column: "apartment_renter");

            migrationBuilder.CreateIndex(
                name: "IX_apartments_tenant_id_apartment_no",
                schema: "tenant",
                table: "apartments",
                columns: new[] { "tenant_id", "apartment_no" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_complaints_created_by_resident",
                schema: "tenant",
                table: "complaints",
                column: "created_by_resident");

            migrationBuilder.CreateIndex(
                name: "IX_complaints_received_by_staff",
                schema: "tenant",
                table: "complaints",
                column: "received_by_staff");

            migrationBuilder.CreateIndex(
                name: "IX_complaints_tenant_id",
                schema: "tenant",
                table: "complaints",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "IX_contracts_imported_by",
                schema: "tenant",
                table: "contracts",
                column: "imported_by");

            migrationBuilder.CreateIndex(
                name: "IX_contracts_tenant_id_apartment_id",
                schema: "tenant",
                table: "contracts",
                columns: new[] { "tenant_id", "apartment_id" });

            migrationBuilder.CreateIndex(
                name: "IX_contracts_tenant_id_contract_no",
                schema: "tenant",
                table: "contracts",
                columns: new[] { "tenant_id", "contract_no" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_invitations_active",
                schema: "tenant",
                table: "invitations",
                columns: new[] { "tenant_id", "email", "role_id" },
                unique: true,
                filter: "status = 'sent' AND is_deleted = false");

            migrationBuilder.CreateIndex(
                name: "IX_invitations_created_by",
                schema: "tenant",
                table: "invitations",
                column: "created_by");

            migrationBuilder.CreateIndex(
                name: "ix_invitations_expiring",
                schema: "tenant",
                table: "invitations",
                columns: new[] { "status", "expires_at" },
                filter: "status = 'sent'");

            migrationBuilder.CreateIndex(
                name: "IX_invitations_role_id",
                schema: "tenant",
                table: "invitations",
                column: "role_id");

            migrationBuilder.CreateIndex(
                name: "IX_invitations_target_user_id",
                schema: "tenant",
                table: "invitations",
                column: "target_user_id");

            migrationBuilder.CreateIndex(
                name: "IX_invitations_tenant_id_email_token_hash",
                schema: "tenant",
                table: "invitations",
                columns: new[] { "tenant_id", "email", "token_hash" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_invitations_token_hash",
                schema: "tenant",
                table: "invitations",
                column: "token_hash",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_invoice_details_tenant_id_invoice_id",
                schema: "tenant",
                table: "invoice_details",
                columns: new[] { "tenant_id", "invoice_id" });

            migrationBuilder.CreateIndex(
                name: "IX_invoice_details_tenant_id_service_id",
                schema: "tenant",
                table: "invoice_details",
                columns: new[] { "tenant_id", "service_id" });

            migrationBuilder.CreateIndex(
                name: "ix_invoice_allocation_attempt",
                schema: "tenant",
                table: "invoice_payment_allocations",
                columns: new[] { "tenant_id", "payment_attempt_id" });

            migrationBuilder.CreateIndex(
                name: "IX_invoice_payment_allocations_gateway_account_id_gateway_tran~",
                schema: "tenant",
                table: "invoice_payment_allocations",
                columns: new[] { "gateway_account_id", "gateway_transaction_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_invoice_payment_allocations_tenant_id_gateway_account_id_pa~",
                schema: "tenant",
                table: "invoice_payment_allocations",
                columns: new[] { "tenant_id", "gateway_account_id", "payment_attempt_id" });

            migrationBuilder.CreateIndex(
                name: "ix_invoice_payment_attempt_invoice",
                schema: "tenant",
                table: "invoice_payment_attempts",
                columns: new[] { "tenant_id", "invoice_id", "created_at" },
                descending: new[] { false, false, true });

            migrationBuilder.CreateIndex(
                name: "ix_invoice_payment_attempt_open",
                schema: "tenant",
                table: "invoice_payment_attempts",
                columns: new[] { "tenant_id", "status", "created_at" },
                filter: "status IN ('pending', 'partially_paid')");

            migrationBuilder.CreateIndex(
                name: "IX_invoice_payment_attempts_gateway_account_id_gateway_payment~",
                schema: "tenant",
                table: "invoice_payment_attempts",
                columns: new[] { "gateway_account_id", "gateway_payment_request_id" });

            migrationBuilder.CreateIndex(
                name: "IX_invoice_payment_attempts_gateway_account_id_id",
                schema: "tenant",
                table: "invoice_payment_attempts",
                columns: new[] { "gateway_account_id", "id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_invoice_payment_attempts_gateway_payment_request_id",
                schema: "tenant",
                table: "invoice_payment_attempts",
                column: "gateway_payment_request_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_invoice_payment_attempts_requested_by_user_id",
                schema: "tenant",
                table: "invoice_payment_attempts",
                column: "requested_by_user_id");

            migrationBuilder.CreateIndex(
                name: "IX_invoice_payment_attempts_tenant_id_id",
                schema: "tenant",
                table: "invoice_payment_attempts",
                columns: new[] { "tenant_id", "id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_invoices_created_by_staff",
                schema: "tenant",
                table: "invoices",
                column: "created_by_staff");

            migrationBuilder.CreateIndex(
                name: "IX_invoices_deleted_by_staff",
                schema: "tenant",
                table: "invoices",
                column: "deleted_by_staff");

            migrationBuilder.CreateIndex(
                name: "IX_invoices_tenant_id_apartment_id",
                schema: "tenant",
                table: "invoices",
                columns: new[] { "tenant_id", "apartment_id" });

            migrationBuilder.CreateIndex(
                name: "IX_invoices_tenant_id_invoice_no",
                schema: "tenant",
                table: "invoices",
                columns: new[] { "tenant_id", "invoice_no" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_invoices_updated_by_staff",
                schema: "tenant",
                table: "invoices",
                column: "updated_by_staff");

            migrationBuilder.CreateIndex(
                name: "ix_payment_requests_created",
                schema: "payment",
                table: "payment_requests",
                columns: new[] { "provider_account_id", "created_at" },
                descending: new[] { false, true });

            migrationBuilder.CreateIndex(
                name: "IX_payment_requests_provider_account_id_order_code",
                schema: "payment",
                table: "payment_requests",
                columns: new[] { "provider_account_id", "order_code" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_payment_requests_request_open",
                schema: "payment",
                table: "payment_requests",
                columns: new[] { "provider_account_id", "status", "expires_at" },
                filter: "status IN ('pending', 'underpaid')");

            migrationBuilder.CreateIndex(
                name: "ux_payment_requests__provider_link",
                schema: "payment",
                table: "payment_requests",
                columns: new[] { "provider_account_id", "provider_payment_link_id" },
                unique: true,
                filter: "provider_payment_link_id IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_payment_transactions_provider_account_id_id",
                schema: "payment",
                table: "payment_transactions",
                columns: new[] { "provider_account_id", "id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_payment_transactions_provider_account_id_provider_transacti~",
                schema: "payment",
                table: "payment_transactions",
                columns: new[] { "provider_account_id", "provider_transaction_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_payment_transactions_provider_account_id_source_webhook_eve~",
                schema: "payment",
                table: "payment_transactions",
                columns: new[] { "provider_account_id", "source_webhook_event_id" });

            migrationBuilder.CreateIndex(
                name: "ix_payment_transactions_request",
                schema: "payment",
                table: "payment_transactions",
                columns: new[] { "provider_account_id", "payment_request_id", "occurred_at" });

            migrationBuilder.CreateIndex(
                name: "ix_payment_transactions_unresolved",
                schema: "payment",
                table: "payment_transactions",
                columns: new[] { "provider_account_id", "occurred_at" },
                filter: "payment_request_id IS NULL");

            migrationBuilder.CreateIndex(
                name: "IX_payout_batches_provider_account_id_id",
                schema: "payment",
                table: "payout_batches",
                columns: new[] { "provider_account_id", "id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_payout_batches_provider_account_id_idempotency_key",
                schema: "payment",
                table: "payout_batches",
                columns: new[] { "provider_account_id", "idempotency_key" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_payout_batches_provider_account_id_reference_id",
                schema: "payment",
                table: "payout_batches",
                columns: new[] { "provider_account_id", "reference_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_payout_batches_reconciliation",
                schema: "payment",
                table: "payout_batches",
                columns: new[] { "provider_account_id", "status", "last_synced_at" },
                filter: "status IN ('submitting', 'processing', 'partially_succeeded')");

            migrationBuilder.CreateIndex(
                name: "ux_payout_batches_provider_id",
                schema: "payment",
                table: "payout_batches",
                columns: new[] { "provider_account_id", "provider_payout_id" },
                unique: true,
                filter: "provider_payout_id IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "ix_payout_items_batch_status",
                schema: "payment",
                table: "payout_items",
                columns: new[] { "provider_account_id", "payout_batch_id", "status" });

            migrationBuilder.CreateIndex(
                name: "IX_payout_items_provider_account_id_id",
                schema: "payment",
                table: "payout_items",
                columns: new[] { "provider_account_id", "id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_payout_items_provider_account_id_payout_batch_id_reference_~",
                schema: "payment",
                table: "payout_items",
                columns: new[] { "provider_account_id", "payout_batch_id", "reference_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ux_payout_items_provider_transaction",
                schema: "payment",
                table: "payout_items",
                columns: new[] { "provider_account_id", "provider_transaction_id" },
                unique: true,
                filter: "provider_transaction_id IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_payroll_details_staff_receive",
                schema: "tenant",
                table: "payroll_details",
                column: "staff_receive");

            migrationBuilder.CreateIndex(
                name: "IX_payroll_details_tenant_id_payroll_id_staff_receive",
                schema: "tenant",
                table: "payroll_details",
                columns: new[] { "tenant_id", "payroll_id", "staff_receive" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_payroll_disbursement_items_staff_user_id",
                schema: "tenant",
                table: "payroll_disbursement_items",
                column: "staff_user_id");

            migrationBuilder.CreateIndex(
                name: "IX_payroll_disbursement_items_tenant_id_payroll_detail_id",
                schema: "tenant",
                table: "payroll_disbursement_items",
                columns: new[] { "tenant_id", "payroll_detail_id" });

            migrationBuilder.CreateIndex(
                name: "IX_payroll_disbursement_items_tenant_id_payroll_disbursement_i~",
                schema: "tenant",
                table: "payroll_disbursement_items",
                columns: new[] { "tenant_id", "payroll_disbursement_id", "payroll_detail_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_payroll_item_status",
                schema: "tenant",
                table: "payroll_disbursement_items",
                columns: new[] { "tenant_id", "payroll_disbursement_id", "status" });

            migrationBuilder.CreateIndex(
                name: "ux_payroll_item_gateway",
                schema: "tenant",
                table: "payroll_disbursement_items",
                columns: new[] { "gateway_account_id", "gateway_payout_item_id" },
                unique: true,
                filter: "gateway_payout_item_id IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "ix_payroll_disbursement_status",
                schema: "tenant",
                table: "payroll_disbursements",
                columns: new[] { "tenant_id", "status", "created_at" });

            migrationBuilder.CreateIndex(
                name: "IX_payroll_disbursements_approved_by_user_id",
                schema: "tenant",
                table: "payroll_disbursements",
                column: "approved_by_user_id");

            migrationBuilder.CreateIndex(
                name: "IX_payroll_disbursements_issued_by_user_id",
                schema: "tenant",
                table: "payroll_disbursements",
                column: "issued_by_user_id");

            migrationBuilder.CreateIndex(
                name: "IX_payroll_disbursements_tenant_id_id",
                schema: "tenant",
                table: "payroll_disbursements",
                columns: new[] { "tenant_id", "id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_payroll_disbursements_tenant_id_payroll_id_approved_payroll~",
                schema: "tenant",
                table: "payroll_disbursements",
                columns: new[] { "tenant_id", "payroll_id", "approved_payroll_version" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ux_payroll_disbursement_gateway_batch",
                schema: "tenant",
                table: "payroll_disbursements",
                columns: new[] { "gateway_account_id", "gateway_payout_batch_id" },
                unique: true,
                filter: "gateway_payout_batch_id IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_payrolls_created_by",
                schema: "tenant",
                table: "payrolls",
                column: "created_by");

            migrationBuilder.CreateIndex(
                name: "ix_provider_accounts_platform_active",
                schema: "payment",
                table: "provider_accounts",
                column: "is_active",
                filter: "owner_scope = 'platform'");

            migrationBuilder.CreateIndex(
                name: "ix_provider_accounts_tenant_active",
                schema: "payment",
                table: "provider_accounts",
                columns: new[] { "tenant_id", "is_active" },
                filter: "owner_scope = 'tenant'");

            migrationBuilder.CreateIndex(
                name: "IX_provider_accounts_webhook_key",
                schema: "payment",
                table: "provider_accounts",
                column: "webhook_key",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ux_provider_accounts_merchant",
                schema: "payment",
                table: "provider_accounts",
                columns: new[] { "provider", "merchant_code" },
                unique: true,
                filter: "merchant_code IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "ux_provider_accounts_platform_code",
                schema: "payment",
                table: "provider_accounts",
                column: "code",
                unique: true,
                filter: "owner_scope = 'platform'");

            migrationBuilder.CreateIndex(
                name: "ux_provider_accounts_tenant_code",
                schema: "payment",
                table: "provider_accounts",
                columns: new[] { "tenant_id", "code" },
                unique: true,
                filter: "owner_scope = 'tenant'");

            migrationBuilder.CreateIndex(
                name: "ix_role_permissions_permission",
                schema: "security",
                table: "role_permissions",
                columns: new[] { "scope", "permission_code", "role_id" });

            migrationBuilder.CreateIndex(
                name: "ux_roles_platform_code",
                schema: "security",
                table: "roles",
                column: "code",
                unique: true,
                filter: "scope = 'platform' AND is_deleted = false");

            migrationBuilder.CreateIndex(
                name: "ux_roles_tenant_code",
                schema: "security",
                table: "roles",
                columns: new[] { "tenant_id", "code" },
                unique: true,
                filter: "scope = 'tenant' AND is_deleted = false");

            migrationBuilder.CreateIndex(
                name: "IX_rules_imported_by",
                schema: "tenant",
                table: "rules",
                column: "imported_by");

            migrationBuilder.CreateIndex(
                name: "IX_rules_tenant_id",
                schema: "tenant",
                table: "rules",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "IX_services_created_by_manager",
                schema: "tenant",
                table: "services",
                column: "created_by_manager");

            migrationBuilder.CreateIndex(
                name: "IX_services_deleted_by_manager",
                schema: "tenant",
                table: "services",
                column: "deleted_by_manager");

            migrationBuilder.CreateIndex(
                name: "IX_services_tenant_id_code",
                schema: "tenant",
                table: "services",
                columns: new[] { "tenant_id", "code" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_services_updated_by_manager",
                schema: "tenant",
                table: "services",
                column: "updated_by_manager");

            migrationBuilder.CreateIndex(
                name: "IX_staff_time_sheets_created_by_manager",
                schema: "tenant",
                table: "staff_time_sheets",
                column: "created_by_manager");

            migrationBuilder.CreateIndex(
                name: "IX_staff_time_sheets_staff_id",
                schema: "tenant",
                table: "staff_time_sheets",
                column: "staff_id");

            migrationBuilder.CreateIndex(
                name: "IX_staff_time_sheets_tenant_id_payroll_period_staff_id",
                schema: "tenant",
                table: "staff_time_sheets",
                columns: new[] { "tenant_id", "payroll_period", "staff_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_subscription_invoices_due",
                schema: "platform",
                table: "subscription_invoices",
                columns: new[] { "tenant_id", "status", "due_at" },
                filter: "status IN ('issued', 'partially_paid', 'overdue')");

            migrationBuilder.CreateIndex(
                name: "IX_subscription_invoices_tenant_id_invoice_no",
                schema: "platform",
                table: "subscription_invoices",
                columns: new[] { "tenant_id", "invoice_no" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_subscription_invoices_tenant_id_subscription_id",
                schema: "platform",
                table: "subscription_invoices",
                columns: new[] { "tenant_id", "subscription_id" });

            migrationBuilder.CreateIndex(
                name: "IX_subscription_payment_allocations_gateway_account_id_gateway~",
                schema: "platform",
                table: "subscription_payment_allocations",
                columns: new[] { "gateway_account_id", "gateway_transaction_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_subscription_payment_allocations_gateway_account_id_payment~",
                schema: "platform",
                table: "subscription_payment_allocations",
                columns: new[] { "gateway_account_id", "payment_attempt_id" });

            migrationBuilder.CreateIndex(
                name: "ix_subscription_attempt_invoice",
                schema: "platform",
                table: "subscription_payment_attempts",
                columns: new[] { "subscription_invoice_id", "created_at" },
                descending: new[] { false, true });

            migrationBuilder.CreateIndex(
                name: "IX_subscription_payment_attempts_gateway_account_id_gateway_pa~",
                schema: "platform",
                table: "subscription_payment_attempts",
                columns: new[] { "gateway_account_id", "gateway_payment_request_id" });

            migrationBuilder.CreateIndex(
                name: "IX_subscription_payment_attempts_gateway_account_id_id",
                schema: "platform",
                table: "subscription_payment_attempts",
                columns: new[] { "gateway_account_id", "id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_subscription_payment_attempts_gateway_owner_scope_gateway_a~",
                schema: "platform",
                table: "subscription_payment_attempts",
                columns: new[] { "gateway_owner_scope", "gateway_account_id" });

            migrationBuilder.CreateIndex(
                name: "IX_subscription_payment_attempts_gateway_payment_request_id",
                schema: "platform",
                table: "subscription_payment_attempts",
                column: "gateway_payment_request_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_subscription_payment_attempts_requested_by_user_id",
                schema: "platform",
                table: "subscription_payment_attempts",
                column: "requested_by_user_id");

            migrationBuilder.CreateIndex(
                name: "IX_subscription_plans_created_by_admin",
                schema: "platform",
                table: "subscription_plans",
                column: "created_by_admin");

            migrationBuilder.CreateIndex(
                name: "IX_subscription_plans_deleted_by_admin",
                schema: "platform",
                table: "subscription_plans",
                column: "deleted_by_admin");

            migrationBuilder.CreateIndex(
                name: "IX_subscription_plans_updated_by_admin",
                schema: "platform",
                table: "subscription_plans",
                column: "updated_by_admin");

            migrationBuilder.CreateIndex(
                name: "IX_subscriptions_plan_id",
                schema: "platform",
                table: "subscriptions",
                column: "plan_id");

            migrationBuilder.CreateIndex(
                name: "ux_subscriptions_tenant_active",
                schema: "platform",
                table: "subscriptions",
                column: "tenant_id",
                unique: true,
                filter: "status IN ('trial', 'active', 'past_due')");

            migrationBuilder.CreateIndex(
                name: "IX_tenants_code",
                schema: "platform",
                table: "tenants",
                column: "code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_tenants_slug",
                schema: "platform",
                table: "tenants",
                column: "slug",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_tenants_tax_code",
                schema: "platform",
                table: "tenants",
                column: "tax_code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_user_claims_UserId",
                schema: "identity",
                table: "user_claims",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_user_logins_UserId",
                schema: "identity",
                table: "user_logins",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "ix_user_role_active",
                schema: "security",
                table: "user_roles",
                columns: new[] { "user_id", "role_id" },
                unique: true,
                filter: "revoked_at IS NULL");

            migrationBuilder.CreateIndex(
                name: "IX_user_roles_granted_by_user_id",
                schema: "security",
                table: "user_roles",
                column: "granted_by_user_id");

            migrationBuilder.CreateIndex(
                name: "IX_user_roles_scope_role_id",
                schema: "security",
                table: "user_roles",
                columns: new[] { "scope", "role_id" });

            migrationBuilder.CreateIndex(
                name: "ix_user_roles_tenant_user_active",
                schema: "security",
                table: "user_roles",
                columns: new[] { "tenant_id", "user_id" },
                filter: "scope = 'tenant' AND revoked_at IS NULL")
                .Annotation("Npgsql:IndexInclude", new[] { "role_id" });

            migrationBuilder.CreateIndex(
                name: "ux_user_roles_platform_user_active",
                schema: "security",
                table: "user_roles",
                column: "user_id",
                unique: true,
                filter: "scope = 'platform' AND revoked_at IS NULL");

            migrationBuilder.CreateIndex(
                name: "ix_users_tenant",
                schema: "identity",
                table: "users",
                column: "tenant_id",
                filter: "tenant_id IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "UserNameIndex",
                schema: "identity",
                table: "users",
                column: "normalized_user_name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ux_users_normalized_email",
                schema: "identity",
                table: "users",
                column: "normalized_email",
                unique: true,
                filter: "normalized_email IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "ux_users_phone",
                schema: "identity",
                table: "users",
                column: "phone_number",
                unique: true,
                filter: "phone_number IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "ix_webhook_events_order_code",
                schema: "payment",
                table: "webhook_events",
                columns: new[] { "provider_account_id", "order_code" });

            migrationBuilder.CreateIndex(
                name: "ix_webhook_events_pending",
                schema: "payment",
                table: "webhook_events",
                columns: new[] { "next_attempt_at", "received_at" },
                filter: "signature_valid = true AND processing_status IN ('received', 'failed') AND processed_at IS NULL");

            migrationBuilder.CreateIndex(
                name: "IX_webhook_events_provider_account_id_event_key",
                schema: "payment",
                table: "webhook_events",
                columns: new[] { "provider_account_id", "event_key" },
                unique: true,
                filter: "event_key IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_webhook_events_provider_account_id_id",
                schema: "payment",
                table: "webhook_events",
                columns: new[] { "provider_account_id", "id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_webhook_events_provider_account_id_payment_request_id",
                schema: "payment",
                table: "webhook_events",
                columns: new[] { "provider_account_id", "payment_request_id" });

            migrationBuilder.CreateIndex(
                name: "ix_webhook_events_provider_link",
                schema: "payment",
                table: "webhook_events",
                columns: new[] { "provider_account_id", "provider_payment_link_id" },
                filter: "provider_payment_link_id IS NOT NULL");

            migrationBuilder.Sql("""
                CREATE OR REPLACE FUNCTION security.assert_tenant_users()
                RETURNS trigger
                LANGUAGE plpgsql
                AS $$
                DECLARE
                    expected_tenant uuid;
                    referenced_user uuid;
                    actual_tenant uuid;
                    i integer;
                BEGIN
                    expected_tenant := (to_jsonb(NEW) ->> TG_ARGV[0])::uuid;

                    FOR i IN 1..(TG_NARGS - 1) LOOP
                        referenced_user := NULLIF(to_jsonb(NEW) ->> TG_ARGV[i], '')::uuid;
                        IF referenced_user IS NULL THEN
                            CONTINUE;
                        END IF;

                        SELECT tenant_id INTO actual_tenant
                        FROM identity.users
                        WHERE id = referenced_user;

                        IF NOT FOUND OR actual_tenant IS DISTINCT FROM expected_tenant THEN
                            RAISE EXCEPTION 'User % does not belong to tenant %', referenced_user, expected_tenant
                                USING ERRCODE = '23514';
                        END IF;
                    END LOOP;

                    RETURN NEW;
                END;
                $$;

                CREATE OR REPLACE FUNCTION security.assert_platform_users()
                RETURNS trigger
                LANGUAGE plpgsql
                AS $$
                DECLARE
                    referenced_user uuid;
                    actual_tenant uuid;
                    i integer;
                BEGIN
                    FOR i IN 0..(TG_NARGS - 1) LOOP
                        referenced_user := NULLIF(to_jsonb(NEW) ->> TG_ARGV[i], '')::uuid;
                        IF referenced_user IS NULL THEN
                            CONTINUE;
                        END IF;

                        SELECT tenant_id INTO actual_tenant
                        FROM identity.users
                        WHERE id = referenced_user;

                        IF NOT FOUND OR actual_tenant IS NOT NULL THEN
                            RAISE EXCEPTION 'User % is not a Platform user', referenced_user
                                USING ERRCODE = '23514';
                        END IF;
                    END LOOP;

                    RETURN NEW;
                END;
                $$;

                CREATE OR REPLACE FUNCTION security.prevent_user_tenant_change()
                RETURNS trigger
                LANGUAGE plpgsql
                AS $$
                BEGIN
                    IF NEW.tenant_id IS DISTINCT FROM OLD.tenant_id THEN
                        RAISE EXCEPTION 'User tenant ownership is immutable'
                            USING ERRCODE = '23514';
                    END IF;
                    RETURN NEW;
                END;
                $$;

                CREATE OR REPLACE FUNCTION security.prevent_role_owner_change()
                RETURNS trigger
                LANGUAGE plpgsql
                AS $$
                BEGIN
                    IF NEW.scope IS DISTINCT FROM OLD.scope
                       OR NEW.tenant_id IS DISTINCT FROM OLD.tenant_id THEN
                        RAISE EXCEPTION 'Role scope and tenant ownership are immutable'
                            USING ERRCODE = '23514';
                    END IF;
                    RETURN NEW;
                END;
                $$;

                CREATE OR REPLACE FUNCTION security.validate_user_role_assignment()
                RETURNS trigger
                LANGUAGE plpgsql
                AS $$
                DECLARE
                    role_tenant uuid;
                    role_code text;
                    user_tenant uuid;
                    grantor_tenant uuid;
                    has_tenant_admin boolean;
                    has_other_active boolean;
                BEGIN
                    PERFORM pg_advisory_xact_lock(hashtextextended(NEW.user_id::text, 0));

                    SELECT tenant_id, code INTO role_tenant, role_code
                    FROM security.roles
                    WHERE id = NEW.role_id
                      AND scope = NEW.scope
                      AND is_deleted = false;

                    IF NOT FOUND THEN
                        RAISE EXCEPTION 'Role % is missing, deleted, or belongs to another scope', NEW.role_id
                            USING ERRCODE = '23514';
                    END IF;

                    SELECT tenant_id INTO user_tenant
                    FROM identity.users
                    WHERE id = NEW.user_id
                      AND is_deleted = false;

                    IF NOT FOUND THEN
                        RAISE EXCEPTION 'User % is missing or deleted', NEW.user_id
                            USING ERRCODE = '23514';
                    END IF;

                    IF NEW.scope = 'platform' THEN
                        IF NEW.tenant_id IS NOT NULL
                           OR role_tenant IS NOT NULL
                           OR user_tenant IS NOT NULL
                           OR role_code <> 'PLATFORM_ADMIN' THEN
                            RAISE EXCEPTION 'Invalid Platform role assignment'
                                USING ERRCODE = '23514';
                        END IF;
                    ELSE
                        IF NEW.tenant_id IS NULL
                           OR role_tenant IS DISTINCT FROM NEW.tenant_id
                           OR user_tenant IS DISTINCT FROM NEW.tenant_id THEN
                            RAISE EXCEPTION 'User, role, and assignment must belong to the same tenant'
                                USING ERRCODE = '23514';
                        END IF;
                    END IF;

                    IF NEW.granted_by_user_id IS NOT NULL THEN
                        SELECT tenant_id INTO grantor_tenant
                        FROM identity.users
                        WHERE id = NEW.granted_by_user_id
                          AND is_deleted = false;

                        IF NOT FOUND
                           OR (NEW.scope = 'platform' AND grantor_tenant IS NOT NULL)
                           OR (NEW.scope = 'tenant' AND grantor_tenant IS DISTINCT FROM NEW.tenant_id) THEN
                            RAISE EXCEPTION 'Grantor does not belong to the assignment scope'
                                USING ERRCODE = '23514';
                        END IF;
                    END IF;

                    IF NEW.scope = 'tenant' AND NEW.revoked_at IS NULL THEN
                        SELECT EXISTS (
                            SELECT 1
                            FROM security.user_roles ur
                            JOIN security.roles r ON r.id = ur.role_id AND r.scope = ur.scope
                            WHERE ur.user_id = NEW.user_id
                              AND ur.tenant_id = NEW.tenant_id
                              AND ur.revoked_at IS NULL
                              AND ur.id <> NEW.id
                              AND r.code = 'TENANT_ADMIN'
                        ) INTO has_tenant_admin;

                        IF role_code = 'TENANT_ADMIN' OR has_tenant_admin THEN
                            IF role_code NOT IN ('TENANT_ADMIN', 'MANAGER', 'STAFF')
                               OR EXISTS (
                                    SELECT 1
                                    FROM security.user_roles ur
                                    JOIN security.roles r ON r.id = ur.role_id AND r.scope = ur.scope
                                    WHERE ur.user_id = NEW.user_id
                                      AND ur.tenant_id = NEW.tenant_id
                                      AND ur.revoked_at IS NULL
                                      AND ur.id <> NEW.id
                                      AND r.code NOT IN ('TENANT_ADMIN', 'MANAGER', 'STAFF')
                               ) THEN
                                RAISE EXCEPTION 'TENANT_ADMIN may only combine TENANT_ADMIN, MANAGER, and STAFF'
                                    USING ERRCODE = '23514';
                            END IF;
                        ELSE
                            SELECT EXISTS (
                                SELECT 1
                                FROM security.user_roles ur
                                WHERE ur.user_id = NEW.user_id
                                  AND ur.tenant_id = NEW.tenant_id
                                  AND ur.revoked_at IS NULL
                                  AND ur.id <> NEW.id
                            ) INTO has_other_active;

                            IF has_other_active THEN
                                RAISE EXCEPTION 'Only TENANT_ADMIN may have multiple active tenant roles'
                                    USING ERRCODE = '23514';
                            END IF;
                        END IF;
                    END IF;

                    RETURN NEW;
                END;
                $$;

                CREATE OR REPLACE FUNCTION tenant.validate_invitation_scope()
                RETURNS trigger
                LANGUAGE plpgsql
                AS $$
                DECLARE
                    role_tenant uuid;
                    role_scope security.role_scope_enum;
                    creator_tenant uuid;
                    target_tenant uuid;
                BEGIN
                    SELECT tenant_id, scope INTO role_tenant, role_scope
                    FROM security.roles
                    WHERE id = NEW.role_id
                      AND is_deleted = false;

                    IF NOT FOUND OR role_scope <> 'tenant' OR role_tenant IS DISTINCT FROM NEW.tenant_id THEN
                        RAISE EXCEPTION 'Invitation role does not belong to the invitation tenant'
                            USING ERRCODE = '23514';
                    END IF;

                    SELECT tenant_id INTO creator_tenant FROM identity.users WHERE id = NEW.created_by;
                    IF NOT FOUND OR creator_tenant IS DISTINCT FROM NEW.tenant_id THEN
                        RAISE EXCEPTION 'Invitation creator does not belong to the invitation tenant'
                            USING ERRCODE = '23514';
                    END IF;

                    IF NEW.target_user_id IS NOT NULL THEN
                        SELECT tenant_id INTO target_tenant FROM identity.users WHERE id = NEW.target_user_id;
                        IF NOT FOUND OR target_tenant IS DISTINCT FROM NEW.tenant_id THEN
                            RAISE EXCEPTION 'Invitation target does not belong to the invitation tenant'
                                USING ERRCODE = '23514';
                        END IF;
                    END IF;

                    RETURN NEW;
                END;
                $$;

                CREATE OR REPLACE FUNCTION payment.prevent_provider_owner_change()
                RETURNS trigger
                LANGUAGE plpgsql
                AS $$
                BEGIN
                    IF NEW.owner_scope IS DISTINCT FROM OLD.owner_scope
                       OR NEW.tenant_id IS DISTINCT FROM OLD.tenant_id THEN
                        RAISE EXCEPTION 'Provider account ownership is immutable'
                            USING ERRCODE = '23514';
                    END IF;
                    RETURN NEW;
                END;
                $$;

                CREATE OR REPLACE FUNCTION payment.assert_tenant_provider()
                RETURNS trigger
                LANGUAGE plpgsql
                AS $$
                DECLARE
                    expected_tenant uuid;
                    account_id uuid;
                    account_tenant uuid;
                    account_scope payment.provider_owner_scope;
                    account_active boolean;
                    requires_active boolean;
                BEGIN
                    expected_tenant := (to_jsonb(NEW) ->> TG_ARGV[0])::uuid;
                    account_id := (to_jsonb(NEW) ->> TG_ARGV[1])::uuid;

                    requires_active := TG_OP = 'INSERT';
                    IF TG_OP = 'UPDATE' THEN
                        requires_active := account_id IS DISTINCT FROM
                            NULLIF(to_jsonb(OLD) ->> TG_ARGV[1], '')::uuid;
                    END IF;

                    SELECT tenant_id, owner_scope, is_active
                    INTO account_tenant, account_scope, account_active
                    FROM payment.provider_accounts
                    WHERE id = account_id;

                    IF NOT FOUND
                       OR account_scope <> 'tenant'
                       OR account_tenant IS DISTINCT FROM expected_tenant
                       OR (requires_active AND NOT account_active) THEN
                        RAISE EXCEPTION 'Provider account does not belong to the tenant'
                            USING ERRCODE = '23514';
                    END IF;

                    RETURN NEW;
                END;
                $$;

                CREATE OR REPLACE FUNCTION payment.validate_allocation()
                RETURNS trigger
                LANGUAGE plpgsql
                AS $$
                DECLARE
                    transaction_amount numeric(18,2);
                BEGIN
                    PERFORM pg_advisory_xact_lock(
                        hashtextextended(NEW.gateway_account_id::text || NEW.gateway_transaction_id::text, 0));

                    SELECT amount INTO transaction_amount
                    FROM payment.payment_transactions
                    WHERE provider_account_id = NEW.gateway_account_id
                      AND id = NEW.gateway_transaction_id
                      AND status = 'verified';

                    IF NOT FOUND OR NEW.amount_applied > transaction_amount THEN
                        RAISE EXCEPTION 'Allocation requires a verified transaction and cannot exceed its amount'
                            USING ERRCODE = '23514';
                    END IF;

                    IF TG_TABLE_SCHEMA = 'tenant' AND EXISTS (
                        SELECT 1 FROM platform.subscription_payment_allocations
                        WHERE gateway_account_id = NEW.gateway_account_id
                          AND gateway_transaction_id = NEW.gateway_transaction_id
                    ) THEN
                        RAISE EXCEPTION 'Transaction is already allocated to a subscription payment'
                            USING ERRCODE = '23514';
                    END IF;

                    IF TG_TABLE_SCHEMA = 'platform' AND EXISTS (
                        SELECT 1 FROM tenant.invoice_payment_allocations
                        WHERE gateway_account_id = NEW.gateway_account_id
                          AND gateway_transaction_id = NEW.gateway_transaction_id
                    ) THEN
                        RAISE EXCEPTION 'Transaction is already allocated to an invoice payment'
                            USING ERRCODE = '23514';
                    END IF;

                    RETURN NEW;
                END;
                $$;

                CREATE TRIGGER tr_users_immutable_tenant
                BEFORE UPDATE OF tenant_id ON identity.users
                FOR EACH ROW EXECUTE FUNCTION security.prevent_user_tenant_change();

                CREATE TRIGGER tr_roles_immutable_owner
                BEFORE UPDATE OF scope, tenant_id ON security.roles
                FOR EACH ROW EXECUTE FUNCTION security.prevent_role_owner_change();

                CREATE TRIGGER tr_user_roles_validate
                BEFORE INSERT OR UPDATE ON security.user_roles
                FOR EACH ROW EXECUTE FUNCTION security.validate_user_role_assignment();

                CREATE TRIGGER tr_invitations_validate
                BEFORE INSERT OR UPDATE ON tenant.invitations
                FOR EACH ROW EXECUTE FUNCTION tenant.validate_invitation_scope();

                CREATE TRIGGER tr_provider_accounts_immutable_owner
                BEFORE UPDATE OF owner_scope, tenant_id ON payment.provider_accounts
                FOR EACH ROW EXECUTE FUNCTION payment.prevent_provider_owner_change();

                CREATE TRIGGER tr_staff_time_sheets_users
                BEFORE INSERT OR UPDATE ON tenant.staff_time_sheets
                FOR EACH ROW EXECUTE FUNCTION security.assert_tenant_users('tenant_id', 'staff_id', 'created_by_manager');
                CREATE TRIGGER tr_services_users
                BEFORE INSERT OR UPDATE ON tenant.services
                FOR EACH ROW EXECUTE FUNCTION security.assert_tenant_users('tenant_id', 'created_by_manager', 'updated_by_manager', 'deleted_by_manager');
                CREATE TRIGGER tr_rules_users
                BEFORE INSERT OR UPDATE ON tenant.rules
                FOR EACH ROW EXECUTE FUNCTION security.assert_tenant_users('tenant_id', 'imported_by');
                CREATE TRIGGER tr_payroll_details_users
                BEFORE INSERT OR UPDATE ON tenant.payroll_details
                FOR EACH ROW EXECUTE FUNCTION security.assert_tenant_users('tenant_id', 'staff_receive');
                CREATE TRIGGER tr_payrolls_users
                BEFORE INSERT OR UPDATE ON tenant.payrolls
                FOR EACH ROW EXECUTE FUNCTION security.assert_tenant_users('tenant_id', 'created_by');
                CREATE TRIGGER tr_invoice_attempts_users
                BEFORE INSERT OR UPDATE ON tenant.invoice_payment_attempts
                FOR EACH ROW EXECUTE FUNCTION security.assert_tenant_users('tenant_id', 'requested_by_user_id');
                CREATE TRIGGER tr_contracts_users
                BEFORE INSERT OR UPDATE ON tenant.contracts
                FOR EACH ROW EXECUTE FUNCTION security.assert_tenant_users('tenant_id', 'imported_by');
                CREATE TRIGGER tr_complaints_users
                BEFORE INSERT OR UPDATE ON tenant.complaints
                FOR EACH ROW EXECUTE FUNCTION security.assert_tenant_users('tenant_id', 'created_by_resident', 'received_by_staff');
                CREATE TRIGGER tr_apartments_users
                BEFORE INSERT OR UPDATE ON tenant.apartments
                FOR EACH ROW EXECUTE FUNCTION security.assert_tenant_users('tenant_id', 'apartment_renter');
                CREATE TRIGGER tr_invoices_users
                BEFORE INSERT OR UPDATE ON tenant.invoices
                FOR EACH ROW EXECUTE FUNCTION security.assert_tenant_users('tenant_id', 'created_by_staff', 'updated_by_staff', 'deleted_by_staff');
                CREATE TRIGGER tr_payroll_disbursements_users
                BEFORE INSERT OR UPDATE ON tenant.payroll_disbursements
                FOR EACH ROW EXECUTE FUNCTION security.assert_tenant_users('tenant_id', 'approved_by_user_id', 'issued_by_user_id');
                CREATE TRIGGER tr_payroll_disbursement_items_users
                BEFORE INSERT OR UPDATE ON tenant.payroll_disbursement_items
                FOR EACH ROW EXECUTE FUNCTION security.assert_tenant_users('tenant_id', 'staff_user_id');

                CREATE TRIGGER tr_subscription_plans_platform_users
                BEFORE INSERT OR UPDATE ON platform.subscription_plans
                FOR EACH ROW EXECUTE FUNCTION security.assert_platform_users('created_by_admin', 'updated_by_admin', 'deleted_by_admin');
                CREATE TRIGGER tr_subscription_attempts_platform_users
                BEFORE INSERT OR UPDATE ON platform.subscription_payment_attempts
                FOR EACH ROW EXECUTE FUNCTION security.assert_platform_users('requested_by_user_id');

                CREATE TRIGGER tr_invoice_attempts_provider
                BEFORE INSERT OR UPDATE ON tenant.invoice_payment_attempts
                FOR EACH ROW EXECUTE FUNCTION payment.assert_tenant_provider('tenant_id', 'gateway_account_id');
                CREATE TRIGGER tr_invoice_allocations_provider
                BEFORE INSERT OR UPDATE ON tenant.invoice_payment_allocations
                FOR EACH ROW EXECUTE FUNCTION payment.assert_tenant_provider('tenant_id', 'gateway_account_id');
                CREATE TRIGGER tr_payroll_disbursements_provider
                BEFORE INSERT OR UPDATE ON tenant.payroll_disbursements
                FOR EACH ROW EXECUTE FUNCTION payment.assert_tenant_provider('tenant_id', 'gateway_account_id');
                CREATE TRIGGER tr_payroll_disbursement_items_provider
                BEFORE INSERT OR UPDATE ON tenant.payroll_disbursement_items
                FOR EACH ROW EXECUTE FUNCTION payment.assert_tenant_provider('tenant_id', 'gateway_account_id');

                CREATE TRIGGER tr_invoice_allocations_validate
                BEFORE INSERT OR UPDATE ON tenant.invoice_payment_allocations
                FOR EACH ROW EXECUTE FUNCTION payment.validate_allocation();
                CREATE TRIGGER tr_subscription_allocations_validate
                BEFORE INSERT OR UPDATE ON platform.subscription_payment_allocations
                FOR EACH ROW EXECUTE FUNCTION payment.validate_allocation();
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                DROP FUNCTION IF EXISTS payment.validate_allocation() CASCADE;
                DROP FUNCTION IF EXISTS payment.assert_tenant_provider() CASCADE;
                DROP FUNCTION IF EXISTS payment.prevent_provider_owner_change() CASCADE;
                DROP FUNCTION IF EXISTS tenant.validate_invitation_scope() CASCADE;
                DROP FUNCTION IF EXISTS security.validate_user_role_assignment() CASCADE;
                DROP FUNCTION IF EXISTS security.prevent_role_owner_change() CASCADE;
                DROP FUNCTION IF EXISTS security.prevent_user_tenant_change() CASCADE;
                DROP FUNCTION IF EXISTS security.assert_platform_users() CASCADE;
                DROP FUNCTION IF EXISTS security.assert_tenant_users() CASCADE;
                """);

            migrationBuilder.DropTable(
                name: "complaints",
                schema: "tenant");

            migrationBuilder.DropTable(
                name: "contracts",
                schema: "tenant");

            migrationBuilder.DropTable(
                name: "invitations",
                schema: "tenant");

            migrationBuilder.DropTable(
                name: "invoice_details",
                schema: "tenant");

            migrationBuilder.DropTable(
                name: "invoice_payment_allocations",
                schema: "tenant");

            migrationBuilder.DropTable(
                name: "payroll_disbursement_items",
                schema: "tenant");

            migrationBuilder.DropTable(
                name: "role_permissions",
                schema: "security");

            migrationBuilder.DropTable(
                name: "rules",
                schema: "tenant");

            migrationBuilder.DropTable(
                name: "staff_time_sheets",
                schema: "tenant");

            migrationBuilder.DropTable(
                name: "subscription_payment_allocations",
                schema: "platform");

            migrationBuilder.DropTable(
                name: "user_claims",
                schema: "identity");

            migrationBuilder.DropTable(
                name: "user_logins",
                schema: "identity");

            migrationBuilder.DropTable(
                name: "user_roles",
                schema: "security");

            migrationBuilder.DropTable(
                name: "user_tokens",
                schema: "identity");

            migrationBuilder.DropTable(
                name: "services",
                schema: "tenant");

            migrationBuilder.DropTable(
                name: "invoice_payment_attempts",
                schema: "tenant");

            migrationBuilder.DropTable(
                name: "payout_items",
                schema: "payment");

            migrationBuilder.DropTable(
                name: "payroll_details",
                schema: "tenant");

            migrationBuilder.DropTable(
                name: "payroll_disbursements",
                schema: "tenant");

            migrationBuilder.DropTable(
                name: "permissions",
                schema: "security");

            migrationBuilder.DropTable(
                name: "payment_transactions",
                schema: "payment");

            migrationBuilder.DropTable(
                name: "subscription_payment_attempts",
                schema: "platform");

            migrationBuilder.DropTable(
                name: "roles",
                schema: "security");

            migrationBuilder.DropTable(
                name: "invoices",
                schema: "tenant");

            migrationBuilder.DropTable(
                name: "payout_batches",
                schema: "payment");

            migrationBuilder.DropTable(
                name: "payrolls",
                schema: "tenant");

            migrationBuilder.DropTable(
                name: "webhook_events",
                schema: "payment");

            migrationBuilder.DropTable(
                name: "subscription_invoices",
                schema: "platform");

            migrationBuilder.DropTable(
                name: "apartments",
                schema: "tenant");

            migrationBuilder.DropTable(
                name: "payment_requests",
                schema: "payment");

            migrationBuilder.DropTable(
                name: "subscriptions",
                schema: "platform");

            migrationBuilder.DropTable(
                name: "provider_accounts",
                schema: "payment");

            migrationBuilder.DropTable(
                name: "subscription_plans",
                schema: "platform");

            migrationBuilder.DropTable(
                name: "users",
                schema: "identity");

            migrationBuilder.DropTable(
                name: "tenants",
                schema: "platform");

            // Remove the now-empty canonical schemas (including PostgreSQL enum types)
            // and restore the pre-consolidation schemas when this migration is reverted.
            migrationBuilder.Sql("""
                DROP SCHEMA IF EXISTS security CASCADE;
                DROP SCHEMA IF EXISTS payment CASCADE;
                DROP SCHEMA IF EXISTS tenant CASCADE;
                DROP SCHEMA IF EXISTS platform CASCADE;
                DROP SCHEMA IF EXISTS identity CASCADE;

                DO $migration$
                DECLARE
                    source_schema text;
                    legacy_schema text;
                BEGIN
                    FOREACH source_schema IN ARRAY ARRAY['identity', 'platform', 'tenant']
                    LOOP
                        legacy_schema := 'legacy_' || source_schema || '_20261109';

                        IF EXISTS (
                            SELECT 1 FROM pg_namespace WHERE nspname = legacy_schema
                        ) THEN
                            EXECUTE format(
                                'ALTER SCHEMA %I RENAME TO %I',
                                legacy_schema,
                                source_schema);
                        END IF;
                    END LOOP;
                END
                $migration$;

                INSERT INTO public."__EFMigrationsHistory" ("MigrationId", "ProductVersion")
                SELECT "MigrationId", "ProductVersion"
                FROM public."__EFMigrationsHistoryLegacy_20261109"
                ON CONFLICT ("MigrationId") DO NOTHING;

                DROP TABLE IF EXISTS public."__EFMigrationsHistoryLegacy_20261109";
                """);
        }
    }
}
