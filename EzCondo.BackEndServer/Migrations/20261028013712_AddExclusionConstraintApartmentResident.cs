using EzCondo.BackEndServer.Data.Entities;
using EzCondo.BackEndServer.Data.Enums;
using Microsoft.EntityFrameworkCore.Migrations;
using NpgsqlTypes;
using System;

#nullable disable

namespace EzCondo.BackEndServer.Migrations
{
    /// <inheritdoc />
    public partial class AddExclusionConstraintApartmentResident : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "tenant");

            migrationBuilder.EnsureSchema(
                name: "platform");

            migrationBuilder.EnsureSchema(
                name: "identity");

            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:Enum:identity.user_status_enum", "active,locked,removed,pending,invited")
                .Annotation("Npgsql:Enum:platform.subscription_invoice_status", "draft,issued,partially_paid,paid,overdue,cancelled,void")
                .Annotation("Npgsql:Enum:platform.subscription_status_enum", "trial,active,past_due,suspended,cancelled,expired")
                .Annotation("Npgsql:Enum:platform.support_ticket_status", "open,assigned,in_progress,resolved,closed,cancelled")
                .Annotation("Npgsql:Enum:platform.system_incident_status", "reported,investigating,monitoring,resolved,closed")
                .Annotation("Npgsql:Enum:platform.tenant_status_enum", "trial,active,suspended,cancelled")
                .Annotation("Npgsql:Enum:tenant.ai_role_enum", "user,assistant,system")
                .Annotation("Npgsql:Enum:tenant.apartment_status_enum", "available,occupied,reserved,maintenance,locked")
                .Annotation("Npgsql:Enum:tenant.billing_period_status", "draft,collecting_readings,calculated,issued,closed,cancelled")
                .Annotation("Npgsql:Enum:tenant.document_status_enum", "draft,published,archived")
                .Annotation("Npgsql:Enum:tenant.document_type_enum", "regulation,rule,faq,service_guide,notice")
                .Annotation("Npgsql:Enum:tenant.payment_order_status", "pending,underpaid,paid,cancelled,expired,failed")
                .Annotation("Npgsql:Enum:tenant.payment_transaction_status", "received,verified,rejected,refunded")
                .Annotation("Npgsql:Enum:tenant.payroll_status_enum", "draft,pending_approval,processing,completed,approved,rejected,issued")
                .Annotation("Npgsql:Enum:tenant.resident_complaint_status", "assigned,in_progress,resolved,cancelled")
                .Annotation("Npgsql:Enum:tenant.service_billing_type", "fixed,metered,one_time")
                .Annotation("Npgsql:Enum:tenant.tenant_contract_status", "active,expired,terminated")
                .Annotation("Npgsql:Enum:tenant.tenant_invoice_status", "draft,issued,paid,overdue,cancelled,void")
                .Annotation("Npgsql:Enum:tenant.tenant_resident_status", "invited,active,suspended,left")
                .Annotation("Npgsql:Enum:tenant.time_sheet_status", "draft,comfirmed,included")
                .Annotation("Npgsql:PostgresExtension:btree_gist", ",,")
                .Annotation("Npgsql:PostgresExtension:citext", ",,")
                .Annotation("Npgsql:PostgresExtension:pg_trgm", ",,");

            migrationBuilder.CreateSequence(
                name: "payos_order_code_seq",
                schema: "tenant",
                startValue: 100000L);

            migrationBuilder.CreateSequence(
                name: "subscription_payment_order_code_seq",
                schema: "platform",
                startValue: 100000L);

            migrationBuilder.CreateTable(
                name: "IdentityUsers",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    UserName = table.Column<string>(type: "text", nullable: true),
                    NormalizedUserName = table.Column<string>(type: "text", nullable: true),
                    Email = table.Column<string>(type: "text", nullable: true),
                    NormalizedEmail = table.Column<string>(type: "text", nullable: true),
                    EmailConfirmed = table.Column<bool>(type: "boolean", nullable: false),
                    PasswordHash = table.Column<string>(type: "text", nullable: true),
                    SecurityStamp = table.Column<string>(type: "text", nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "text", nullable: true),
                    PhoneNumber = table.Column<string>(type: "text", nullable: true),
                    PhoneNumberConfirmed = table.Column<bool>(type: "boolean", nullable: false),
                    TwoFactorEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    LockoutEnd = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    LockoutEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    AccessFailedCount = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IdentityUsers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "platform_payment_provider_configs",
                schema: "platform",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    code = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    provider = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    merchant_code = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
                    secret_reference = table.Column<string>(type: "text", nullable: false),
                    settings = table.Column<string>(type: "jsonb", nullable: false, defaultValueSql: "'{}'::jsonb"),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_platform_payment_provider_configs", x => x.id);
                    table.CheckConstraint("CK_platform_provider_code", "btrim(code) <> ''");
                    table.CheckConstraint("CK_platform_provider_name", "btrim(provider) <> ''");
                    table.CheckConstraint("CK_platform_provider_secret", "btrim(secret_reference) <> ''");
                });

            migrationBuilder.CreateTable(
                name: "platform_permissions",
                schema: "platform",
                columns: table => new
                {
                    code = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    description = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_platform_permissions", x => x.code);
                });

            migrationBuilder.CreateTable(
                name: "platform_roles",
                schema: "platform",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    code = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    name = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    is_system = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_platform_roles", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "subscription_plans",
                schema: "platform",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    description = table.Column<string>(type: "text", nullable: true),
                    billing_cycle_months = table.Column<short>(type: "smallint", nullable: false, defaultValue: (short)6),
                    price = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    feature_limits = table.Column<string>(type: "jsonb", nullable: false, defaultValueSql: "'{}'::jsonb"),
                    is_available = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true, defaultValueSql: "now()"),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    deleted_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_subscription_plans", x => x.id);
                    table.CheckConstraint("CK_subscription_plans_billing_cycle_months", "billing_cycle_months IN (6, 12, 24)");
                    table.CheckConstraint("CK_subscription_plans_price", "price >= 0");
                });

            migrationBuilder.CreateTable(
                name: "tenant_permissions",
                schema: "tenant",
                columns: table => new
                {
                    code = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    description = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tenant_permissions", x => x.code);
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
                    tax_code = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    contact = table.Column<string>(type: "text", nullable: false),
                    status = table.Column<TenantStatusEnum>(type: "platform.tenant_status_enum", nullable: false, defaultValueSql: "'trial'::platform.tenant_status_enum"),
                    trial_started_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    trial_ends_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    activated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    suspended_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    suspension_reason = table.Column<string>(type: "text", nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tenants", x => x.id);
                    table.CheckConstraint("CK_tenants_slug_format", "slug ~ '^[a-z0-9]+(?:-[a-z0-9]+)*$'");
                    table.CheckConstraint("CK_tenants_trial_dates", "trial_ends_at IS NULL OR trial_started_at IS NULL OR trial_ends_at > trial_started_at");
                });

            migrationBuilder.CreateTable(
                name: "platform_role_permissions",
                schema: "platform",
                columns: table => new
                {
                    role_id = table.Column<Guid>(type: "uuid", nullable: false),
                    permission_code = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_platform_role_permissions", x => new { x.role_id, x.permission_code });
                    table.ForeignKey(
                        name: "FK_platform_role_permissions_platform_permissions_permission_c~",
                        column: x => x.permission_code,
                        principalSchema: "platform",
                        principalTable: "platform_permissions",
                        principalColumn: "code",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_platform_role_permissions_platform_roles_role_id",
                        column: x => x.role_id,
                        principalSchema: "platform",
                        principalTable: "platform_roles",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "subscriptions",
                schema: "platform",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    status = table.Column<SubscriptionStatusEnum>(type: "platform.subscription_status_enum", nullable: false, defaultValueSql: "'trial'::platform.subscription_status_enum"),
                    starts_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    ends_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    auto_renew = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true, defaultValueSql: "now()"),
                    cancelled_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: false),
                    plan_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_subscriptions", x => x.id);
                    table.CheckConstraint("CK_subscriptions_dates", "ends_at > starts_at");
                    table.ForeignKey(
                        name: "FK_subscriptions_subscription_plans_plan_id",
                        column: x => x.plan_id,
                        principalSchema: "platform",
                        principalTable: "subscription_plans",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_subscriptions_tenants_tenant_id",
                        column: x => x.tenant_id,
                        principalSchema: "platform",
                        principalTable: "tenants",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "tenant_buildings",
                schema: "tenant",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    name = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    address = table.Column<string>(type: "text", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tenant_buildings", x => x.id);
                    table.UniqueConstraint("AK_tenant_buildings_tenant_id_id", x => new { x.tenant_id, x.id });
                    table.ForeignKey(
                        name: "FK_tenant_buildings_tenants_tenant_id",
                        column: x => x.tenant_id,
                        principalSchema: "platform",
                        principalTable: "tenants",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "tenant_payment_provider_config",
                schema: "tenant",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    merchant_code = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
                    secret_reference = table.Column<string>(type: "text", nullable: false),
                    settings = table.Column<string>(type: "jsonb", nullable: false, defaultValueSql: "'{}'::jsonb"),
                    is_active = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true, defaultValueSql: "now()"),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tenant_payment_provider_config", x => x.id);
                    table.UniqueConstraint("AK_tenant_payment_provider_config_tenant_id_id", x => new { x.tenant_id, x.id });
                    table.ForeignKey(
                        name: "FK_tenant_payment_provider_config_tenants_tenant_id",
                        column: x => x.tenant_id,
                        principalSchema: "platform",
                        principalTable: "tenants",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "tenant_roles",
                schema: "tenant",
                columns: table => new
                {
                    role = table.Column<Guid>(type: "uuid", nullable: false),
                    code = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    name = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    hierarchy_level = table.Column<short>(type: "smallint", nullable: false),
                    is_system = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tenant_roles", x => x.role);
                    table.CheckConstraint("tenant_roles_hierarchy_check", "hierarchy_level BETWEEN 0 AND 100");
                    table.ForeignKey(
                        name: "FK_tenant_roles_tenants_tenant_id",
                        column: x => x.tenant_id,
                        principalSchema: "platform",
                        principalTable: "tenants",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "tenant_services",
                schema: "tenant",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    name = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    billing_type = table.Column<ServiceBillingType>(type: "tenant.service_billing_type", nullable: false),
                    unit = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    deleted_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true, defaultValueSql: "now()"),
                    deleted_by = table.Column<Guid>(type: "uuid", nullable: true),
                    updated_by = table.Column<Guid>(type: "uuid", nullable: true),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tenant_services", x => x.id);
                    table.UniqueConstraint("AK_tenant_services_tenant_id_id", x => new { x.tenant_id, x.id });
                    table.ForeignKey(
                        name: "FK_tenant_services_tenants_tenant_id",
                        column: x => x.tenant_id,
                        principalSchema: "platform",
                        principalTable: "tenants",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "users",
                schema: "identity",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    full_name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    avatar_url = table.Column<string>(type: "text", nullable: true),
                    status = table.Column<UserStatusEnum>(type: "identity.user_status_enum", nullable: false, defaultValueSql: "'pending'::identity.user_status_enum"),
                    last_log_in_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true, defaultValueSql: "now()"),
                    citizen_id = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_name = table.Column<string>(type: "text", nullable: true),
                    normalized_user_name = table.Column<string>(type: "text", nullable: true),
                    email = table.Column<string>(type: "citext", maxLength: 256, nullable: true),
                    normalized_email = table.Column<string>(type: "text", nullable: true),
                    email_confirmed = table.Column<bool>(type: "boolean", nullable: false),
                    password_hash = table.Column<string>(type: "text", nullable: true),
                    security_stamp = table.Column<string>(type: "text", nullable: true),
                    concurrency_stamp = table.Column<string>(type: "text", nullable: true),
                    phone_number = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    phone_number_confirmed = table.Column<bool>(type: "boolean", nullable: false),
                    two_factor_enabled = table.Column<bool>(type: "boolean", nullable: false),
                    lockout_end = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    lockout_enabled = table.Column<bool>(type: "boolean", nullable: false),
                    access_failed_count = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_users", x => x.id);
                    table.UniqueConstraint("AK_users_tenant_id_id", x => new { x.tenant_id, x.id });
                    table.CheckConstraint("CK_users_email_or_phone_number", "email IS NOT NULL OR phone_number IS NOT NULL");
                    table.ForeignKey(
                        name: "FK_users_tenants_tenant_id",
                        column: x => x.tenant_id,
                        principalSchema: "platform",
                        principalTable: "tenants",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "subscription_events",
                schema: "platform",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    event_type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    data = table.Column<string>(type: "jsonb", nullable: false, defaultValueSql: "'{}'::jsonb"),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true, defaultValueSql: "now()"),
                    subscription_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_subscription_events", x => x.id);
                    table.ForeignKey(
                        name: "FK_subscription_events_subscriptions_subscription_id",
                        column: x => x.subscription_id,
                        principalSchema: "platform",
                        principalTable: "subscriptions",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "subscription_invoices",
                schema: "platform",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    invoice_no = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    period = table.Column<NpgsqlRange<DateOnly>>(type: "daterange", nullable: false),
                    amount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    due_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true),
                    paid_amount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false, defaultValue: 0m),
                    status = table.Column<SubscriptionInvoiceStatus>(type: "platform.subscription_invoice_status", nullable: false, defaultValueSql: "'draft'::platform.subscription_invoice_status"),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    issued_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true),
                    subscription_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_subscription_invoices", x => x.id);
                    table.CheckConstraint("CK_subscription_invoice_paid_le_amount", "paid_amount <= amount");
                    table.CheckConstraint("CK_subscription_invoices_amount_positive", "amount >= 0");
                    table.CheckConstraint("CK_subscription_invoices_paid_amount_positive", "paid_amount >= 0");
                    table.CheckConstraint("CK_subscription_invoices_period_not_empty", "NOT isempty(period)");
                    table.ForeignKey(
                        name: "FK_subscription_invoices_subscriptions_subscription_id",
                        column: x => x.subscription_id,
                        principalSchema: "platform",
                        principalTable: "subscriptions",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "tenant_apartments",
                schema: "tenant",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    area_m2 = table.Column<decimal>(type: "numeric(10,2)", precision: 10, scale: 2, nullable: false),
                    apartment_no = table.Column<string>(type: "text", nullable: false),
                    bedroom = table.Column<short>(type: "smallint", nullable: false),
                    status = table.Column<ApartmentStatusEnum>(type: "tenant.apartment_status_enum", nullable: false, defaultValueSql: "'available'::tenant.apartment_status_enum"),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true, defaultValueSql: "now()"),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: false),
                    building_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tenant_apartments", x => x.id);
                    table.UniqueConstraint("AK_tenant_apartments_tenant_id_id", x => new { x.tenant_id, x.id });
                    table.CheckConstraint("CK_tenant_apartments_areaM2_positive", "area_m2 > 0");
                    table.CheckConstraint("CK_tenant_apartments_bedroom_positive", "bedroom >= 0");
                    table.ForeignKey(
                        name: "FK_tenant_apartments_tenant_buildings_tenant_id_building_id",
                        columns: x => new { x.tenant_id, x.building_id },
                        principalSchema: "tenant",
                        principalTable: "tenant_buildings",
                        principalColumns: new[] { "tenant_id", "id" },
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_tenant_apartments_tenants_tenant_id",
                        column: x => x.tenant_id,
                        principalSchema: "platform",
                        principalTable: "tenants",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "tenant_webhook_events",
                schema: "tenant",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    event_key = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    order_code = table.Column<int>(type: "integer", nullable: true),
                    is_signature_valid = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    payload = table.Column<string>(type: "jsonb", nullable: false),
                    received_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: false, defaultValueSql: "now()"),
                    processed_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true),
                    processing_error = table.Column<string>(type: "text", nullable: true),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: false),
                    provider_config_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tenant_webhook_events", x => x.id);
                    table.ForeignKey(
                        name: "FK_tenant_webhook_events_tenant_payment_provider_config_tenant~",
                        columns: x => new { x.tenant_id, x.provider_config_id },
                        principalSchema: "tenant",
                        principalTable: "tenant_payment_provider_config",
                        principalColumns: new[] { "tenant_id", "id" });
                    table.ForeignKey(
                        name: "FK_tenant_webhook_events_tenants_tenant_id",
                        column: x => x.tenant_id,
                        principalSchema: "platform",
                        principalTable: "tenants",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "tenant_role_permission",
                schema: "tenant",
                columns: table => new
                {
                    role_id = table.Column<Guid>(type: "uuid", nullable: false),
                    permission_code = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tenant_role_permission", x => new { x.role_id, x.permission_code });
                    table.ForeignKey(
                        name: "FK_tenant_role_permission_tenant_permissions_permission_code",
                        column: x => x.permission_code,
                        principalSchema: "tenant",
                        principalTable: "tenant_permissions",
                        principalColumn: "code",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_tenant_role_permission_tenant_roles_role_id",
                        column: x => x.role_id,
                        principalSchema: "tenant",
                        principalTable: "tenant_roles",
                        principalColumn: "role",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_tenant_role_permission_tenants_tenant_id",
                        column: x => x.tenant_id,
                        principalSchema: "platform",
                        principalTable: "tenants",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "knowledge_documents",
                schema: "tenant",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    title = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    type = table.Column<DocumentTypeEnum>(type: "tenant.document_type_enum", nullable: false),
                    status = table.Column<DocumentStatusEnum>(type: "tenant.document_status_enum", nullable: false, defaultValueSql: "'draft'::tenant.document_status_enum"),
                    effective_period = table.Column<NpgsqlRange<DateOnly>>(type: "daterange", nullable: true),
                    content = table.Column<string>(type: "text", nullable: false),
                    source_url = table.Column<string>(type: "text", nullable: true),
                    search_vector = table.Column<NpgsqlTsVector>(type: "tsvector", nullable: false, computedColumnSql: "to_tsvector('simple', coalesce(title, '') || ' ' || coalesce(content, ''))", stored: true),
                    published_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    deleted_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true, defaultValueSql: "now()"),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: false),
                    created_by = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_knowledge_documents", x => x.id);
                    table.CheckConstraint("CK_knowledge_documents_effective_period", "NOT isempty(effective_period)");
                    table.CheckConstraint("CK_knowledge_documents_published_at", "status <> 'published'::tenant.document_status_enum OR published_at IS NOT NULL");
                    table.ForeignKey(
                        name: "FK_knowledge_documents_tenants_tenant_id",
                        column: x => x.tenant_id,
                        principalSchema: "platform",
                        principalTable: "tenants",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_knowledge_documents_users_tenant_id_created_by",
                        columns: x => new { x.tenant_id, x.created_by },
                        principalSchema: "identity",
                        principalTable: "users",
                        principalColumns: new[] { "tenant_id", "id" });
                });

            migrationBuilder.CreateTable(
                name: "platform_invitations",
                schema: "platform",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    email = table.Column<string>(type: "citext", nullable: false),
                    token_hash = table.Column<string>(type: "text", nullable: false),
                    expires_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: false),
                    accepted_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true),
                    revoked_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true, defaultValueSql: "now()"),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    deleted_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    platform_role_id = table.Column<Guid>(type: "uuid", nullable: false),
                    invited_by = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_platform_invitations", x => x.id);
                    table.ForeignKey(
                        name: "FK_platform_invitations_platform_roles_platform_role_id",
                        column: x => x.platform_role_id,
                        principalSchema: "platform",
                        principalTable: "platform_roles",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_platform_invitations_users_invited_by",
                        column: x => x.invited_by,
                        principalSchema: "identity",
                        principalTable: "users",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "platform_user_role",
                schema: "platform",
                columns: table => new
                {
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    role_id = table.Column<Guid>(type: "uuid", nullable: false),
                    granted_by = table.Column<Guid>(type: "uuid", nullable: true),
                    granted_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_platform_user_role", x => new { x.user_id, x.role_id });
                    table.ForeignKey(
                        name: "FK_platform_user_role_platform_roles_role_id",
                        column: x => x.role_id,
                        principalSchema: "platform",
                        principalTable: "platform_roles",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_platform_user_role_users_granted_by",
                        column: x => x.granted_by,
                        principalSchema: "identity",
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_platform_user_role_users_user_id",
                        column: x => x.user_id,
                        principalSchema: "identity",
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "support_tickets",
                schema: "platform",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    subject = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    status = table.Column<SupportTicketStatus>(type: "platform.support_ticket_status", nullable: false, defaultValueSql: "'open'::platform.support_ticket_status"),
                    resolved_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    message = table.Column<string>(type: "text", nullable: false),
                    due_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    deleted_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true, defaultValueSql: "now()"),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: false),
                    requester_id = table.Column<Guid>(type: "uuid", nullable: false),
                    receiver_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_support_tickets", x => x.id);
                    table.ForeignKey(
                        name: "FK_support_tickets_tenants_tenant_id",
                        column: x => x.tenant_id,
                        principalSchema: "platform",
                        principalTable: "tenants",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_support_tickets_users_tenant_id_receiver_id",
                        columns: x => new { x.tenant_id, x.receiver_id },
                        principalSchema: "identity",
                        principalTable: "users",
                        principalColumns: new[] { "tenant_id", "id" });
                    table.ForeignKey(
                        name: "FK_support_tickets_users_tenant_id_requester_id",
                        columns: x => new { x.tenant_id, x.requester_id },
                        principalSchema: "identity",
                        principalTable: "users",
                        principalColumns: new[] { "tenant_id", "id" });
                });

            migrationBuilder.CreateTable(
                name: "system_incidents",
                schema: "platform",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    code = table.Column<string>(type: "text", nullable: false),
                    title = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    description = table.Column<string>(type: "text", nullable: true),
                    severity = table.Column<short>(type: "smallint", nullable: false),
                    status = table.Column<SystemIncidentStatus>(type: "platform.system_incident_status", nullable: false, defaultValueSql: "'investigating'::platform.system_incident_status"),
                    resolved_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true, defaultValueSql: "now()"),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    deleted_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    created_by = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_system_incidents", x => x.id);
                    table.ForeignKey(
                        name: "FK_system_incidents_users_created_by",
                        column: x => x.created_by,
                        principalSchema: "identity",
                        principalTable: "users",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "tenant_ai_chat_sessions",
                schema: "tenant",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    last_message_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    identity_user_id = table.Column<Guid>(type: "uuid", nullable: true),
                    tenant_id1 = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tenant_ai_chat_sessions", x => x.id);
                    table.UniqueConstraint("AK_tenant_ai_chat_sessions_tenant_id_id", x => new { x.tenant_id, x.id });
                    table.ForeignKey(
                        name: "FK_tenant_ai_chat_sessions_tenants_tenant_id",
                        column: x => x.tenant_id,
                        principalSchema: "platform",
                        principalTable: "tenants",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_tenant_ai_chat_sessions_tenants_tenant_id1",
                        column: x => x.tenant_id1,
                        principalSchema: "platform",
                        principalTable: "tenants",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_tenant_ai_chat_sessions_users_identity_user_id",
                        column: x => x.identity_user_id,
                        principalSchema: "identity",
                        principalTable: "users",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_tenant_ai_chat_sessions_users_user_id",
                        column: x => x.user_id,
                        principalSchema: "identity",
                        principalTable: "users",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "tenant_billing_periods",
                schema: "tenant",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    period = table.Column<NpgsqlRange<DateOnly>>(type: "daterange", nullable: false),
                    due_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    status = table.Column<BillingPeriodStatus>(type: "tenant.billing_period_status", nullable: false, defaultValueSql: "'draft'::tenant.billing_period_status"),
                    closed_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true, defaultValueSql: "now()"),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: false),
                    created_by = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tenant_billing_periods", x => x.id);
                    table.UniqueConstraint("AK_tenant_billing_periods_tenant_id_id", x => new { x.tenant_id, x.id });
                    table.CheckConstraint("CK_billing_periods_period_not_empty", "NOT isempty(period)");
                    table.ForeignKey(
                        name: "FK_tenant_billing_periods_tenants_tenant_id",
                        column: x => x.tenant_id,
                        principalSchema: "platform",
                        principalTable: "tenants",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_tenant_billing_periods_users_tenant_id_created_by",
                        columns: x => new { x.tenant_id, x.created_by },
                        principalSchema: "identity",
                        principalTable: "users",
                        principalColumns: new[] { "tenant_id", "id" });
                });

            migrationBuilder.CreateTable(
                name: "tenant_invitations",
                schema: "tenant",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    email = table.Column<string>(type: "citext", nullable: false),
                    token_hash = table.Column<string>(type: "text", nullable: false),
                    expires_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    accepted_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    revoked_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true, defaultValueSql: "now()"),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    deleted_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: false),
                    tenant_role_id = table.Column<Guid>(type: "uuid", nullable: false),
                    invited_by = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tenant_invitations", x => x.id);
                    table.UniqueConstraint("AK_tenant_invitations_tenant_id_id", x => new { x.tenant_id, x.id });
                    table.CheckConstraint("CK_tenant_invitations_expires_at_le_created_at", "expires_at > created_at");
                    table.ForeignKey(
                        name: "FK_tenant_invitations_tenant_roles_tenant_role_id",
                        column: x => x.tenant_role_id,
                        principalSchema: "tenant",
                        principalTable: "tenant_roles",
                        principalColumn: "role");
                    table.ForeignKey(
                        name: "FK_tenant_invitations_tenants_tenant_id",
                        column: x => x.tenant_id,
                        principalSchema: "platform",
                        principalTable: "tenants",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_tenant_invitations_users_tenant_id_invited_by",
                        columns: x => new { x.tenant_id, x.invited_by },
                        principalSchema: "identity",
                        principalTable: "users",
                        principalColumns: new[] { "tenant_id", "id" });
                });

            migrationBuilder.CreateTable(
                name: "tenant_payrolls",
                schema: "tenant",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    total_amount = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    status = table.Column<PayrollStatusEnum>(type: "tenant.payroll_status_enum", nullable: false, defaultValueSql: "'draft'::tenant.payroll_status_enum"),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    deleted_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true, defaultValueSql: "now()"),
                    submitted_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    approved_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    rejected_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    issued_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: false),
                    created_by = table.Column<Guid>(type: "uuid", nullable: false),
                    submitted_by = table.Column<Guid>(type: "uuid", nullable: true),
                    approved_by = table.Column<Guid>(type: "uuid", nullable: true),
                    rejected_by = table.Column<Guid>(type: "uuid", nullable: true),
                    issued_by = table.Column<Guid>(type: "uuid", nullable: true),
                    receive_by = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tenant_payrolls", x => x.id);
                    table.UniqueConstraint("AK_tenant_payrolls_tenant_id_id", x => new { x.tenant_id, x.id });
                    table.CheckConstraint("CK_tenant_payrolls_total_amount_positive", "total_amount >= 0");
                    table.ForeignKey(
                        name: "FK_tenant_payrolls_tenants_tenant_id",
                        column: x => x.tenant_id,
                        principalSchema: "platform",
                        principalTable: "tenants",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_tenant_payrolls_users_tenant_id_approved_by",
                        columns: x => new { x.tenant_id, x.approved_by },
                        principalSchema: "identity",
                        principalTable: "users",
                        principalColumns: new[] { "tenant_id", "id" });
                    table.ForeignKey(
                        name: "FK_tenant_payrolls_users_tenant_id_created_by",
                        columns: x => new { x.tenant_id, x.created_by },
                        principalSchema: "identity",
                        principalTable: "users",
                        principalColumns: new[] { "tenant_id", "id" });
                    table.ForeignKey(
                        name: "FK_tenant_payrolls_users_tenant_id_issued_by",
                        columns: x => new { x.tenant_id, x.issued_by },
                        principalSchema: "identity",
                        principalTable: "users",
                        principalColumns: new[] { "tenant_id", "id" });
                    table.ForeignKey(
                        name: "FK_tenant_payrolls_users_tenant_id_rejected_by",
                        columns: x => new { x.tenant_id, x.rejected_by },
                        principalSchema: "identity",
                        principalTable: "users",
                        principalColumns: new[] { "tenant_id", "id" });
                    table.ForeignKey(
                        name: "FK_tenant_payrolls_users_tenant_id_submitted_by",
                        columns: x => new { x.tenant_id, x.submitted_by },
                        principalSchema: "identity",
                        principalTable: "users",
                        principalColumns: new[] { "tenant_id", "id" });
                });

            migrationBuilder.CreateTable(
                name: "tenant_residents",
                schema: "tenant",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    full_name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    email = table.Column<string>(type: "citext", nullable: true),
                    phone = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    date_of_birth = table.Column<DateOnly>(type: "date", nullable: false),
                    citizen_id = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    status = table.Column<TenantResidentStatus>(type: "tenant.tenant_resident_status", nullable: false, defaultValueSql: "'invited'::tenant.tenant_resident_status"),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    deleted_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true, defaultValueSql: "now()"),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tenant_residents", x => x.id);
                    table.UniqueConstraint("AK_tenant_residents_tenant_id_id", x => new { x.tenant_id, x.id });
                    table.ForeignKey(
                        name: "FK_tenant_residents_tenants_tenant_id",
                        column: x => x.tenant_id,
                        principalSchema: "platform",
                        principalTable: "tenants",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_tenant_residents_users_tenant_id_user_id",
                        columns: x => new { x.tenant_id, x.user_id },
                        principalSchema: "identity",
                        principalTable: "users",
                        principalColumns: new[] { "tenant_id", "id" });
                });

            migrationBuilder.CreateTable(
                name: "tenant_staff_profiles",
                schema: "tenant",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    date_of_birth = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    bank_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    bank_account_number = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    deleted_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true, defaultValueSql: "now()"),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tenant_staff_profiles", x => x.id);
                    table.UniqueConstraint("AK_tenant_staff_profiles_tenant_id_id", x => new { x.tenant_id, x.id });
                    table.ForeignKey(
                        name: "FK_tenant_staff_profiles_tenants_tenant_id",
                        column: x => x.tenant_id,
                        principalSchema: "platform",
                        principalTable: "tenants",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_tenant_staff_profiles_users_tenant_id_user_id",
                        columns: x => new { x.tenant_id, x.user_id },
                        principalSchema: "identity",
                        principalTable: "users",
                        principalColumns: new[] { "tenant_id", "id" },
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "tenant_status_histories",
                schema: "tenant",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    old_status = table.Column<TenantStatusEnum>(type: "platform.tenant_status_enum", nullable: true),
                    current_status = table.Column<TenantStatusEnum>(type: "platform.tenant_status_enum", nullable: false),
                    reason = table.Column<string>(type: "text", nullable: true),
                    changed_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: false, defaultValueSql: "now()"),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: false),
                    changed_by = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tenant_status_histories", x => x.id);
                    table.ForeignKey(
                        name: "FK_tenant_status_histories_tenants_tenant_id",
                        column: x => x.tenant_id,
                        principalSchema: "platform",
                        principalTable: "tenants",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_tenant_status_histories_users_tenant_id_changed_by",
                        columns: x => new { x.tenant_id, x.changed_by },
                        principalSchema: "identity",
                        principalTable: "users",
                        principalColumns: new[] { "tenant_id", "id" });
                });

            migrationBuilder.CreateTable(
                name: "tenant_user_roles",
                schema: "tenant",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    granted_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    revoked_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    granted_by = table.Column<Guid>(type: "uuid", nullable: true),
                    role_id = table.Column<Guid>(type: "uuid", nullable: false),
                    tenant_role_role = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tenant_user_roles", x => x.id);
                    table.ForeignKey(
                        name: "FK_tenant_user_roles_tenant_roles_role_id",
                        column: x => x.role_id,
                        principalSchema: "tenant",
                        principalTable: "tenant_roles",
                        principalColumn: "role",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_tenant_user_roles_tenant_roles_tenant_role_role",
                        column: x => x.tenant_role_role,
                        principalSchema: "tenant",
                        principalTable: "tenant_roles",
                        principalColumn: "role");
                    table.ForeignKey(
                        name: "FK_tenant_user_roles_users_granted_by",
                        column: x => x.granted_by,
                        principalSchema: "identity",
                        principalTable: "users",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_tenant_user_roles_users_user_id",
                        column: x => x.user_id,
                        principalSchema: "identity",
                        principalTable: "users",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "subscription_payment_orders",
                schema: "platform",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    order_code = table.Column<long>(type: "bigint", nullable: false, defaultValueSql: "nextval('platform.subscription_payment_order_code_seq')"),
                    provider_payment_link_id = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false, defaultValue: "Pending"),
                    amount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    amount_paid = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false, defaultValue: 0m),
                    currency = table.Column<string>(type: "character(3)", nullable: false, defaultValue: "VND"),
                    checkout_url = table.Column<string>(type: "text", nullable: true),
                    qr_code = table.Column<string>(type: "text", nullable: true),
                    return_url = table.Column<string>(type: "text", nullable: true),
                    cancel_url = table.Column<string>(type: "text", nullable: false),
                    expires_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: false),
                    cancelled_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: false),
                    cancellation_reason = table.Column<string>(type: "text", nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true, defaultValueSql: "now()"),
                    subscription_invoice_id = table.Column<Guid>(type: "uuid", nullable: false),
                    provider_config_id = table.Column<Guid>(type: "uuid", nullable: false),
                    requested_by = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_subscription_payment_orders", x => x.id);
                    table.UniqueConstraint("AK_subscription_payment_orders_provider_config_id_id", x => new { x.provider_config_id, x.id });
                    table.CheckConstraint("CK_subscription_order_amount", "amount > 0");
                    table.CheckConstraint("CK_subscription_order_code", "order_code > 0");
                    table.CheckConstraint("CK_subscription_order_currency", "currency = 'VND'");
                    table.CheckConstraint("CK_subscription_order_link", "provider_payment_link_id IS NULL OR btrim(provider_payment_link_id) <> ''");
                    table.CheckConstraint("CK_subscription_order_paid", "amount_paid >= 0");
                    table.CheckConstraint("CK_subscription_order_status", "status IN ('Pending', 'Underpaid', 'Paid', 'Cancelled', 'Expired', 'Failed')");
                    table.ForeignKey(
                        name: "FK_subscription_payment_orders_platform_payment_provider_confi~",
                        column: x => x.provider_config_id,
                        principalSchema: "platform",
                        principalTable: "platform_payment_provider_configs",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_subscription_payment_orders_subscription_invoices_subscript~",
                        column: x => x.subscription_invoice_id,
                        principalSchema: "platform",
                        principalTable: "subscription_invoices",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_subscription_payment_orders_users_requested_by",
                        column: x => x.requested_by,
                        principalSchema: "identity",
                        principalTable: "users",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "incident_affected_tenants",
                schema: "platform",
                columns: table => new
                {
                    incident_id = table.Column<Guid>(type: "uuid", nullable: false),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: false),
                    notified_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_incident_affected_tenants", x => new { x.incident_id, x.tenant_id });
                    table.ForeignKey(
                        name: "FK_incident_affected_tenants_system_incidents_incident_id",
                        column: x => x.incident_id,
                        principalSchema: "platform",
                        principalTable: "system_incidents",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_incident_affected_tenants_tenants_tenant_id",
                        column: x => x.tenant_id,
                        principalSchema: "platform",
                        principalTable: "tenants",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "incident_updates",
                schema: "platform",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    status = table.Column<SystemIncidentStatus>(type: "platform.system_incident_status", nullable: false),
                    message = table.Column<string>(type: "text", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    incident_id = table.Column<Guid>(type: "uuid", nullable: false),
                    author_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_incident_updates", x => x.id);
                    table.ForeignKey(
                        name: "FK_incident_updates_system_incidents_incident_id",
                        column: x => x.incident_id,
                        principalSchema: "platform",
                        principalTable: "system_incidents",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_incident_updates_users_author_id",
                        column: x => x.author_id,
                        principalSchema: "identity",
                        principalTable: "users",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "tenant_ai_chat_messages",
                schema: "tenant",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    role = table.Column<AiRoleEnum>(type: "tenant.ai_role_enum", nullable: false),
                    content = table.Column<string>(type: "text", nullable: false),
                    citations = table.Column<string>(type: "jsonb", nullable: false, defaultValueSql: "'{}'::jsonb"),
                    model = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    session_id = table.Column<Guid>(type: "uuid", nullable: false),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tenant_ai_chat_messages", x => x.id);
                    table.ForeignKey(
                        name: "FK_tenant_ai_chat_messages_tenant_ai_chat_sessions_session_id",
                        column: x => x.session_id,
                        principalSchema: "tenant",
                        principalTable: "tenant_ai_chat_sessions",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_tenant_ai_chat_messages_tenants_tenant_id",
                        column: x => x.tenant_id,
                        principalSchema: "platform",
                        principalTable: "tenants",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "apartment_contracts",
                schema: "tenant",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    contract_no = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    contract_period = table.Column<NpgsqlRange<DateOnly>>(type: "daterange", nullable: false),
                    signed_at = table.Column<DateOnly>(type: "date", nullable: true),
                    status = table.Column<TenantContractStatus>(type: "tenant.tenant_contract_status", nullable: false, defaultValueSql: "'active'::tenant.tenant_contract_status"),
                    document_url = table.Column<string>(type: "text", nullable: true),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    deleted_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true, defaultValueSql: "now()"),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: false),
                    apartment_id = table.Column<Guid>(type: "uuid", nullable: false),
                    resident_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_apartment_contracts", x => x.id);
                    table.CheckConstraint("CK_apartment_contracts_contract_period", "NOT isempty(contract_period)");
                    table.ForeignKey(
                        name: "FK_apartment_contracts_tenant_apartments_tenant_id_apartment_id",
                        columns: x => new { x.tenant_id, x.apartment_id },
                        principalSchema: "tenant",
                        principalTable: "tenant_apartments",
                        principalColumns: new[] { "tenant_id", "id" });
                    table.ForeignKey(
                        name: "FK_apartment_contracts_tenant_residents_tenant_id_resident_id",
                        columns: x => new { x.tenant_id, x.resident_id },
                        principalSchema: "tenant",
                        principalTable: "tenant_residents",
                        principalColumns: new[] { "tenant_id", "id" });
                    table.ForeignKey(
                        name: "FK_apartment_contracts_tenants_tenant_id",
                        column: x => x.tenant_id,
                        principalSchema: "platform",
                        principalTable: "tenants",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "apartment_residents",
                schema: "tenant",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    occupancy_period = table.Column<NpgsqlRange<DateOnly>>(type: "daterange", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: false),
                    apartment_id = table.Column<Guid>(type: "uuid", nullable: false),
                    resident_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_apartment_residents", x => x.id);
                    table.UniqueConstraint("AK_apartment_residents_tenant_id_id", x => new { x.tenant_id, x.id });
                    table.CheckConstraint("apartment_residents_period_check", "NOT isempty(occupancy_period)");
                    table.ForeignKey(
                        name: "FK_apartment_residents_tenant_apartments_tenant_id_apartment_id",
                        columns: x => new { x.tenant_id, x.apartment_id },
                        principalSchema: "tenant",
                        principalTable: "tenant_apartments",
                        principalColumns: new[] { "tenant_id", "id" },
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_apartment_residents_tenant_residents_tenant_id_resident_id",
                        columns: x => new { x.tenant_id, x.resident_id },
                        principalSchema: "tenant",
                        principalTable: "tenant_residents",
                        principalColumns: new[] { "tenant_id", "id" },
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_apartment_residents_tenants_tenant_id",
                        column: x => x.tenant_id,
                        principalSchema: "platform",
                        principalTable: "tenants",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "tenant_invoices",
                schema: "tenant",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    invoice_no = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    status = table.Column<TenantInvoiceStatus>(type: "tenant.tenant_invoice_status", nullable: false, defaultValueSql: "'draft'::tenant.tenant_invoice_status"),
                    currency = table.Column<string>(type: "char(3)", nullable: false, defaultValue: "VND"),
                    subtotal_amount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false, defaultValue: 0m),
                    adjustment_amount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false, defaultValue: 0m),
                    total_amount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false, computedColumnSql: "subtotal_amount + adjustment_amount", stored: true),
                    paid_amount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false, defaultValue: 0m),
                    issued_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    due_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    note = table.Column<string>(type: "text", nullable: true),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    deleted_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true, defaultValueSql: "now()"),
                    deleted_by = table.Column<Guid>(type: "uuid", nullable: true),
                    updated_by = table.Column<Guid>(type: "uuid", nullable: true),
                    created_by = table.Column<Guid>(type: "uuid", nullable: false),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: false),
                    billing_period_id = table.Column<Guid>(type: "uuid", nullable: false),
                    apartment_id = table.Column<Guid>(type: "uuid", nullable: false),
                    billing_contact_id = table.Column<Guid>(type: "uuid", nullable: false),
                    identity_user_id = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tenant_invoices", x => x.id);
                    table.UniqueConstraint("AK_tenant_invoices_tenant_id_id", x => new { x.tenant_id, x.id });
                    table.CheckConstraint("CK_tenant_invoices_paid_amount_positive", "paid_amount >= 0");
                    table.CheckConstraint("CK_tenant_invoices_paid_le_total", "paid_amount <= total_amount");
                    table.CheckConstraint("CK_tenant_invoices_subtotal_amount_positive", "subtotal_amount >= 0");
                    table.ForeignKey(
                        name: "FK_tenant_invoices_tenant_apartments_tenant_id_apartment_id",
                        columns: x => new { x.tenant_id, x.apartment_id },
                        principalSchema: "tenant",
                        principalTable: "tenant_apartments",
                        principalColumns: new[] { "tenant_id", "id" });
                    table.ForeignKey(
                        name: "FK_tenant_invoices_tenant_billing_periods_tenant_id_billing_pe~",
                        columns: x => new { x.tenant_id, x.billing_period_id },
                        principalSchema: "tenant",
                        principalTable: "tenant_billing_periods",
                        principalColumns: new[] { "tenant_id", "id" },
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_tenant_invoices_tenant_residents_tenant_id_billing_contact_~",
                        columns: x => new { x.tenant_id, x.billing_contact_id },
                        principalSchema: "tenant",
                        principalTable: "tenant_residents",
                        principalColumns: new[] { "tenant_id", "id" });
                    table.ForeignKey(
                        name: "FK_tenant_invoices_tenants_tenant_id",
                        column: x => x.tenant_id,
                        principalSchema: "platform",
                        principalTable: "tenants",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_tenant_invoices_users_identity_user_id",
                        column: x => x.identity_user_id,
                        principalSchema: "identity",
                        principalTable: "users",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "tenant_payroll_details",
                schema: "tenant",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    base_salary = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    allowance = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    deduction = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    net_pay = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: false),
                    recipient_id = table.Column<Guid>(type: "uuid", nullable: false),
                    payroll_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tenant_payroll_details", x => x.id);
                    table.CheckConstraint("CK_tenant_payroll_details_allowance_positive", "allowance >= 0");
                    table.CheckConstraint("CK_tenant_payroll_details_base_salary_positive", "base_salary > 0");
                    table.CheckConstraint("CK_tenant_payroll_details_deduction_positive", "deduction >= 0");
                    table.CheckConstraint("CK_tenant_payroll_details_net_pay", "net_pay >= 0");
                    table.ForeignKey(
                        name: "FK_tenant_payroll_details_tenant_payrolls_tenant_id_payroll_id",
                        columns: x => new { x.tenant_id, x.payroll_id },
                        principalSchema: "tenant",
                        principalTable: "tenant_payrolls",
                        principalColumns: new[] { "tenant_id", "id" });
                    table.ForeignKey(
                        name: "FK_tenant_payroll_details_tenant_staff_profiles_tenant_id_reci~",
                        columns: x => new { x.tenant_id, x.recipient_id },
                        principalSchema: "tenant",
                        principalTable: "tenant_staff_profiles",
                        principalColumns: new[] { "tenant_id", "id" });
                    table.ForeignKey(
                        name: "FK_tenant_payroll_details_tenants_tenant_id",
                        column: x => x.tenant_id,
                        principalSchema: "platform",
                        principalTable: "tenants",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "tenant_staff_time_sheets",
                schema: "tenant",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    date_month = table.Column<short>(type: "smallint", nullable: false),
                    date_year = table.Column<short>(type: "smallint", nullable: false),
                    standard_working_day = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    actual_working_day = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    paid_leave_day = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    unpaid_leave_day = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    overtime_hour_normal = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    overtime_hour_weekend = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    overtime_hour_holiday = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    night_shifts_count = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    bonus_amount = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    penalty_amount = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    note = table.Column<string>(type: "text", nullable: true),
                    status = table.Column<TimeSheetStatus>(type: "tenant.time_sheet_status", nullable: false, defaultValueSql: "'draft'::tenant.time_sheet_status"),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    deleted_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true, defaultValueSql: "now()"),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: false),
                    staff_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tenant_staff_time_sheets", x => x.id);
                    table.UniqueConstraint("AK_tenant_staff_time_sheets_tenant_id_id", x => new { x.tenant_id, x.id });
                    table.CheckConstraint("CK_tenant_staff_time_sheets_actual_working_day_positive", "actual_working_day >= 0 AND actual_working_day <= 30");
                    table.CheckConstraint("CK_tenant_staff_time_sheets_bonus_amount_positive", "bonus_amount >= 0");
                    table.CheckConstraint("CK_tenant_staff_time_sheets_night_shifts_count_positive", "night_shifts_count >= 0 AND night_shifts_count <= 30");
                    table.CheckConstraint("CK_tenant_staff_time_sheets_overtime_hour_holiday_positive", "overtime_hour_holiday >= 0");
                    table.CheckConstraint("CK_tenant_staff_time_sheets_overtime_hour_normal_positive", "overtime_hour_normal >= 0");
                    table.CheckConstraint("CK_tenant_staff_time_sheets_overtime_hour_weekend_positive", "overtime_hour_weekend >= 0");
                    table.CheckConstraint("CK_tenant_staff_time_sheets_paid_leave_day_positive", "paid_leave_day >= 0");
                    table.CheckConstraint("CK_tenant_staff_time_sheets_penalty_amount_positive", "penalty_amount >= 0");
                    table.CheckConstraint("CK_tenant_staff_time_sheets_standard_working_day_positive", "standard_working_day >= 0 AND standard_working_day <= 30");
                    table.CheckConstraint("CK_tenant_staff_time_sheets_unpaid_leave_day_positive", "unpaid_leave_day >= 0");
                    table.ForeignKey(
                        name: "FK_tenant_staff_time_sheets_tenant_staff_profiles_tenant_id_st~",
                        columns: x => new { x.tenant_id, x.staff_id },
                        principalSchema: "tenant",
                        principalTable: "tenant_staff_profiles",
                        principalColumns: new[] { "tenant_id", "id" },
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_tenant_staff_time_sheets_tenants_tenant_id",
                        column: x => x.tenant_id,
                        principalSchema: "platform",
                        principalTable: "tenants",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "subscription_payment_transaction_configs",
                schema: "platform",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    provider_transaction_id = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    provider_reference = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false, defaultValue: "Received"),
                    amount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    occurred_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: false),
                    payer_masked = table.Column<string>(type: "jsonb", nullable: false, defaultValueSql: "'{}'::jsonb"),
                    provider_payload = table.Column<string>(type: "jsonb", nullable: false, defaultValueSql: "'{}'::jsonb"),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    provider_config_id = table.Column<Guid>(type: "uuid", nullable: false),
                    payment_order_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_subscription_payment_transaction_configs", x => x.id);
                    table.CheckConstraint("CK_subscription_transaction_amount", "amount > 0");
                    table.CheckConstraint("CK_subscription_transaction_provider_id", "btrim(provider_transaction_id) <> ''");
                    table.CheckConstraint("CK_subscription_transaction_status", "status IN ('Received', 'Verified', 'Rejected', 'Refunded')");
                    table.ForeignKey(
                        name: "FK_subscription_payment_transaction_configs_platform_payment_p~",
                        column: x => x.provider_config_id,
                        principalSchema: "platform",
                        principalTable: "platform_payment_provider_configs",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_subscription_payment_transaction_configs_subscription_payme~",
                        columns: x => new { x.provider_config_id, x.payment_order_id },
                        principalSchema: "platform",
                        principalTable: "subscription_payment_orders",
                        principalColumns: new[] { "provider_config_id", "id" });
                });

            migrationBuilder.CreateTable(
                name: "subscription_payment_webhook_events",
                schema: "platform",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    event_key = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    order_code = table.Column<long>(type: "bigint", nullable: true),
                    is_signature_valid = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    payload = table.Column<string>(type: "text", nullable: false),
                    received_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: false, defaultValueSql: "now()"),
                    processed_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true),
                    processing_attempts = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    last_attempt_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true),
                    processing_error = table.Column<string>(type: "text", nullable: true),
                    provider_config_id = table.Column<Guid>(type: "uuid", nullable: false),
                    payment_order_id = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_subscription_payment_webhook_events", x => x.id);
                    table.CheckConstraint("CK_subscription_webhook_attempts", "processing_attempts >= 0");
                    table.CheckConstraint("CK_subscription_webhook_event_key", "event_key IS NULL OR (is_signature_valid = TRUE AND btrim(event_key) <> '')");
                    table.CheckConstraint("CK_subscription_webhook_order_verified", "payment_order_id IS NULL OR is_signature_valid = TRUE");
                    table.CheckConstraint("CK_subscription_webhook_processed", "processed_at IS NULL OR (is_signature_valid = TRUE AND payment_order_id IS NOT NULL AND processing_error IS NULL)");
                    table.ForeignKey(
                        name: "FK_subscription_payment_webhook_events_platform_payment_provid~",
                        column: x => x.provider_config_id,
                        principalSchema: "platform",
                        principalTable: "platform_payment_provider_configs",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_subscription_payment_webhook_events_subscription_payment_or~",
                        columns: x => new { x.provider_config_id, x.payment_order_id },
                        principalSchema: "platform",
                        principalTable: "subscription_payment_orders",
                        principalColumns: new[] { "provider_config_id", "id" });
                });

            migrationBuilder.CreateTable(
                name: "resident_complaints",
                schema: "tenant",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    message = table.Column<string>(type: "text", nullable: false),
                    status = table.Column<ResidentComplaintStatus>(type: "tenant.resident_complaint_status", nullable: false, defaultValueSql: "'assigned'::tenant.resident_complaint_status"),
                    resolved_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true, defaultValueSql: "now()"),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    deleted_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: false),
                    apartment_id = table.Column<Guid>(type: "uuid", nullable: false),
                    created_by = table.Column<Guid>(type: "uuid", nullable: false),
                    received_by = table.Column<Guid>(type: "uuid", nullable: false),
                    apartment_resident_id = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_resident_complaints", x => x.id);
                    table.UniqueConstraint("AK_resident_complaints_tenant_id_id", x => new { x.tenant_id, x.id });
                    table.ForeignKey(
                        name: "FK_resident_complaints_apartment_residents_apartment_resident_~",
                        column: x => x.apartment_resident_id,
                        principalSchema: "tenant",
                        principalTable: "apartment_residents",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_resident_complaints_apartment_residents_tenant_id_apartment~",
                        columns: x => new { x.tenant_id, x.apartment_id },
                        principalSchema: "tenant",
                        principalTable: "apartment_residents",
                        principalColumns: new[] { "tenant_id", "id" });
                    table.ForeignKey(
                        name: "FK_resident_complaints_tenants_tenant_id",
                        column: x => x.tenant_id,
                        principalSchema: "platform",
                        principalTable: "tenants",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_resident_complaints_users_tenant_id_created_by",
                        columns: x => new { x.tenant_id, x.created_by },
                        principalSchema: "identity",
                        principalTable: "users",
                        principalColumns: new[] { "tenant_id", "id" },
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_resident_complaints_users_tenant_id_received_by",
                        columns: x => new { x.tenant_id, x.received_by },
                        principalSchema: "identity",
                        principalTable: "users",
                        principalColumns: new[] { "tenant_id", "id" });
                });

            migrationBuilder.CreateTable(
                name: "tenant_invoice_lines",
                schema: "tenant",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    quantity = table.Column<decimal>(type: "numeric(14,3)", nullable: false, defaultValue: 1m),
                    unit = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    unit_price = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    line_amount = table.Column<decimal>(type: "numeric(18,2)", nullable: false, computedColumnSql: "round(quantity * unit_price, 2)", stored: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    invoice_id = table.Column<Guid>(type: "uuid", nullable: false),
                    service_id = table.Column<Guid>(type: "uuid", nullable: false),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tenant_invoice_lines", x => x.id);
                    table.CheckConstraint("CK_tenant_invoice_lines_quantity_positive", "quantity >= 0");
                    table.CheckConstraint("CK_tenant_invoice_lines_unit_price_positive", "unit_price >= 0");
                    table.ForeignKey(
                        name: "FK_tenant_invoice_lines_tenant_invoices_tenant_id_invoice_id",
                        columns: x => new { x.tenant_id, x.invoice_id },
                        principalSchema: "tenant",
                        principalTable: "tenant_invoices",
                        principalColumns: new[] { "tenant_id", "id" },
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_tenant_invoice_lines_tenant_services_tenant_id_service_id",
                        columns: x => new { x.tenant_id, x.service_id },
                        principalSchema: "tenant",
                        principalTable: "tenant_services",
                        principalColumns: new[] { "tenant_id", "id" });
                    table.ForeignKey(
                        name: "FK_tenant_invoice_lines_tenants_tenant_id",
                        column: x => x.tenant_id,
                        principalSchema: "platform",
                        principalTable: "tenants",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "tenant_payment_orders",
                schema: "tenant",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    order_code = table.Column<long>(type: "bigint", nullable: false, defaultValueSql: "nextval('tenant.payos_order_code_seq')"),
                    provider_payment_link_id = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    status = table.Column<PaymentOrderStatus>(type: "tenant.payment_order_status", nullable: false, defaultValueSql: "'pending'::tenant.payment_order_status"),
                    amount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    amount_paid = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false, defaultValue: 0m),
                    currency = table.Column<string>(type: "char(3)", nullable: false, defaultValue: "VND"),
                    checkout_url = table.Column<string>(type: "text", nullable: true),
                    qr_code = table.Column<string>(type: "text", nullable: true),
                    return_url = table.Column<string>(type: "text", nullable: true),
                    cancel_url = table.Column<string>(type: "text", nullable: true),
                    expires_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    cancelled_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    cancellation_reason = table.Column<string>(type: "text", nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true, defaultValueSql: "now()"),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: false),
                    request_by = table.Column<Guid>(type: "uuid", nullable: false),
                    provider_config_id = table.Column<Guid>(type: "uuid", nullable: false),
                    tenant_invoice_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tenant_payment_orders", x => x.id);
                    table.UniqueConstraint("AK_tenant_payment_orders_tenant_id_id", x => new { x.tenant_id, x.id });
                    table.CheckConstraint("CK_payment_orders_amount_paid_nonnegative", "amount_paid >= 0");
                    table.CheckConstraint("CK_payment_orders_amount_paid_not_exceed_amount", "amount_paid <= amount");
                    table.CheckConstraint("CK_payment_orders_amount_positive", "amount > 0");
                    table.ForeignKey(
                        name: "FK_tenant_payment_orders_tenant_invoices_tenant_id_tenant_invo~",
                        columns: x => new { x.tenant_id, x.tenant_invoice_id },
                        principalSchema: "tenant",
                        principalTable: "tenant_invoices",
                        principalColumns: new[] { "tenant_id", "id" });
                    table.ForeignKey(
                        name: "FK_tenant_payment_orders_tenant_payment_provider_config_tenant~",
                        columns: x => new { x.tenant_id, x.provider_config_id },
                        principalSchema: "tenant",
                        principalTable: "tenant_payment_provider_config",
                        principalColumns: new[] { "tenant_id", "id" });
                    table.ForeignKey(
                        name: "FK_tenant_payment_orders_tenants_tenant_id",
                        column: x => x.tenant_id,
                        principalSchema: "platform",
                        principalTable: "tenants",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_tenant_payment_orders_users_tenant_id_request_by",
                        columns: x => new { x.tenant_id, x.request_by },
                        principalSchema: "identity",
                        principalTable: "users",
                        principalColumns: new[] { "tenant_id", "id" });
                });

            migrationBuilder.CreateTable(
                name: "tenant_payment_transactions",
                schema: "tenant",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    provider_reference = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    provider_transaction_id = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    status = table.Column<PaymentTransactionStatus>(type: "tenant.payment_transaction_status", nullable: false, defaultValueSql: "'received'::tenant.payment_transaction_status"),
                    amount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    occurred_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    payer_masked = table.Column<string>(type: "jsonb", nullable: false, defaultValueSql: "'{}'::jsonb"),
                    provider_payload = table.Column<string>(type: "jsonb", nullable: false, defaultValueSql: "'{}'::jsonb"),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: false),
                    payment_order_id = table.Column<Guid>(type: "uuid", nullable: false),
                    tenant_id1 = table.Column<Guid>(type: "uuid", nullable: true),
                    tenant_payment_order_id = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tenant_payment_transactions", x => x.id);
                    table.UniqueConstraint("AK_tenant_payment_transactions_tenant_id_id", x => new { x.tenant_id, x.id });
                    table.CheckConstraint("CK_payment_transactions_amount", "amount > 0");
                    table.ForeignKey(
                        name: "FK_tenant_payment_transactions_tenant_payment_orders_tenant_id~",
                        columns: x => new { x.tenant_id, x.payment_order_id },
                        principalSchema: "tenant",
                        principalTable: "tenant_payment_orders",
                        principalColumns: new[] { "tenant_id", "id" });
                    table.ForeignKey(
                        name: "FK_tenant_payment_transactions_tenant_payment_orders_tenant_pa~",
                        column: x => x.tenant_payment_order_id,
                        principalSchema: "tenant",
                        principalTable: "tenant_payment_orders",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_tenant_payment_transactions_tenants_tenant_id",
                        column: x => x.tenant_id,
                        principalSchema: "platform",
                        principalTable: "tenants",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_tenant_payment_transactions_tenants_tenant_id1",
                        column: x => x.tenant_id1,
                        principalSchema: "platform",
                        principalTable: "tenants",
                        principalColumn: "id");
                });

            migrationBuilder.CreateIndex(
                name: "apartment_contracts_status_idx",
                schema: "tenant",
                table: "apartment_contracts",
                column: "apartment_id",
                unique: true,
                filter: "\"status\" = 'active'::tenant.tenant_contract_status AND \"is_deleted\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_apartment_contracts_contract_period",
                schema: "tenant",
                table: "apartment_contracts",
                column: "contract_period")
                .Annotation("Npgsql:IndexMethod", "gist");

            migrationBuilder.CreateIndex(
                name: "IX_apartment_contracts_tenant_id_apartment_id",
                schema: "tenant",
                table: "apartment_contracts",
                columns: new[] { "tenant_id", "apartment_id" });

            migrationBuilder.CreateIndex(
                name: "IX_apartment_contracts_tenant_id_contract_no",
                schema: "tenant",
                table: "apartment_contracts",
                columns: new[] { "tenant_id", "contract_no" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_apartment_contracts_tenant_id_id",
                schema: "tenant",
                table: "apartment_contracts",
                columns: new[] { "tenant_id", "id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_apartment_contracts_tenant_id_resident_id",
                schema: "tenant",
                table: "apartment_contracts",
                columns: new[] { "tenant_id", "resident_id" });

            migrationBuilder.CreateIndex(
                name: "IX_apartment_residents_tenant_id_apartment_id",
                schema: "tenant",
                table: "apartment_residents",
                columns: new[] { "tenant_id", "apartment_id" });

            migrationBuilder.CreateIndex(
                name: "IX_apartment_residents_tenant_id_resident_id",
                schema: "tenant",
                table: "apartment_residents",
                columns: new[] { "tenant_id", "resident_id" });

            migrationBuilder.CreateIndex(
                name: "IX_incident_affected_tenants_tenant_id",
                schema: "platform",
                table: "incident_affected_tenants",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "IX_incident_updates_author_id",
                schema: "platform",
                table: "incident_updates",
                column: "author_id");

            migrationBuilder.CreateIndex(
                name: "IX_incident_updates_incident_id",
                schema: "platform",
                table: "incident_updates",
                column: "incident_id");

            migrationBuilder.CreateIndex(
                name: "IX_knowledge_documents_effective_period",
                schema: "tenant",
                table: "knowledge_documents",
                column: "effective_period")
                .Annotation("Npgsql:IndexMethod", "gist");

            migrationBuilder.CreateIndex(
                name: "IX_knowledge_documents_search_vector",
                schema: "tenant",
                table: "knowledge_documents",
                column: "search_vector")
                .Annotation("Npgsql:IndexMethod", "gin");

            migrationBuilder.CreateIndex(
                name: "IX_knowledge_documents_tenant_id_created_by",
                schema: "tenant",
                table: "knowledge_documents",
                columns: new[] { "tenant_id", "created_by" });

            migrationBuilder.CreateIndex(
                name: "IX_knowledge_documents_tenant_id_id",
                schema: "tenant",
                table: "knowledge_documents",
                columns: new[] { "tenant_id", "id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "knowledga_documents_idx",
                schema: "tenant",
                table: "knowledge_documents",
                columns: new[] { "tenant_id", "published_at" },
                filter: "\"status\" = 'published'::tenant.document_status_enum AND \"is_deleted\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_platform_invitations_invited_by",
                schema: "platform",
                table: "platform_invitations",
                column: "invited_by");

            migrationBuilder.CreateIndex(
                name: "IX_platform_invitations_platform_role_id",
                schema: "platform",
                table: "platform_invitations",
                column: "platform_role_id");

            migrationBuilder.CreateIndex(
                name: "IX_platform_invitations_token_hash",
                schema: "platform",
                table: "platform_invitations",
                column: "token_hash",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "platform_invitations_idx",
                schema: "platform",
                table: "platform_invitations",
                columns: new[] { "email", "platform_role_id" },
                unique: true,
                filter: "accepted_at IS NULL AND revoked_at IS NULL");

            migrationBuilder.CreateIndex(
                name: "IX_platform_payment_provider_configs_code",
                schema: "platform",
                table: "platform_payment_provider_configs",
                column: "code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_platform_role_permissions_permission_code",
                schema: "platform",
                table: "platform_role_permissions",
                column: "permission_code");

            migrationBuilder.CreateIndex(
                name: "IX_platform_roles_code",
                schema: "platform",
                table: "platform_roles",
                column: "code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_platform_user_role_granted_by",
                schema: "platform",
                table: "platform_user_role",
                column: "granted_by");

            migrationBuilder.CreateIndex(
                name: "IX_platform_user_role_role_id",
                schema: "platform",
                table: "platform_user_role",
                column: "role_id");

            migrationBuilder.CreateIndex(
                name: "IX_resident_complaints_apartment_resident_id",
                schema: "tenant",
                table: "resident_complaints",
                column: "apartment_resident_id");

            migrationBuilder.CreateIndex(
                name: "IX_resident_complaints_id_tenant_id",
                schema: "tenant",
                table: "resident_complaints",
                columns: new[] { "id", "tenant_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_resident_complaints_tenant_id_apartment_id",
                schema: "tenant",
                table: "resident_complaints",
                columns: new[] { "tenant_id", "apartment_id" });

            migrationBuilder.CreateIndex(
                name: "IX_resident_complaints_tenant_id_created_by",
                schema: "tenant",
                table: "resident_complaints",
                columns: new[] { "tenant_id", "created_by" });

            migrationBuilder.CreateIndex(
                name: "IX_resident_complaints_tenant_id_received_by",
                schema: "tenant",
                table: "resident_complaints",
                columns: new[] { "tenant_id", "received_by" });

            migrationBuilder.CreateIndex(
                name: "IX_subscription_events_subscription_id",
                schema: "platform",
                table: "subscription_events",
                column: "subscription_id");

            migrationBuilder.CreateIndex(
                name: "IX_subscription_invoices_invoice_no",
                schema: "platform",
                table: "subscription_invoices",
                column: "invoice_no",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_subscription_invoices_subscription_id",
                schema: "platform",
                table: "subscription_invoices",
                column: "subscription_id");

            migrationBuilder.CreateIndex(
                name: "IX_subscription_payment_orders_order_code",
                schema: "platform",
                table: "subscription_payment_orders",
                column: "order_code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_subscription_payment_orders_provider_config_id_provider_pay~",
                schema: "platform",
                table: "subscription_payment_orders",
                columns: new[] { "provider_config_id", "provider_payment_link_id" },
                unique: true,
                filter: "provider_payment_link_id IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_subscription_payment_orders_requested_by",
                schema: "platform",
                table: "subscription_payment_orders",
                column: "requested_by");

            migrationBuilder.CreateIndex(
                name: "IX_subscription_payment_orders_status_expires_at",
                schema: "platform",
                table: "subscription_payment_orders",
                columns: new[] { "status", "expires_at" },
                filter: "status IN ('Pending', 'Underpaid')");

            migrationBuilder.CreateIndex(
                name: "IX_subscription_payment_orders_subscription_invoice_id_created~",
                schema: "platform",
                table: "subscription_payment_orders",
                columns: new[] { "subscription_invoice_id", "created_at" });

            migrationBuilder.CreateIndex(
                name: "IX_subscription_payment_transaction_configs_payment_order_id_o~",
                schema: "platform",
                table: "subscription_payment_transaction_configs",
                columns: new[] { "payment_order_id", "occurred_at" });

            migrationBuilder.CreateIndex(
                name: "IX_subscription_payment_transaction_configs_provider_config_i~1",
                schema: "platform",
                table: "subscription_payment_transaction_configs",
                columns: new[] { "provider_config_id", "provider_transaction_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_subscription_payment_transaction_configs_provider_config_id~",
                schema: "platform",
                table: "subscription_payment_transaction_configs",
                columns: new[] { "provider_config_id", "payment_order_id" });

            migrationBuilder.CreateIndex(
                name: "IX_subscription_payment_webhook_events_provider_config_id_even~",
                schema: "platform",
                table: "subscription_payment_webhook_events",
                columns: new[] { "provider_config_id", "event_key" },
                unique: true,
                filter: "event_key IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_subscription_payment_webhook_events_provider_config_id_orde~",
                schema: "platform",
                table: "subscription_payment_webhook_events",
                columns: new[] { "provider_config_id", "order_code" });

            migrationBuilder.CreateIndex(
                name: "IX_subscription_payment_webhook_events_provider_config_id_paym~",
                schema: "platform",
                table: "subscription_payment_webhook_events",
                columns: new[] { "provider_config_id", "payment_order_id" });

            migrationBuilder.CreateIndex(
                name: "IX_subscription_payment_webhook_events_received_at",
                schema: "platform",
                table: "subscription_payment_webhook_events",
                column: "received_at",
                filter: "is_signature_valid = TRUE AND processed_at IS NULL");

            migrationBuilder.CreateIndex(
                name: "IX_subscriptions_plan_id",
                schema: "platform",
                table: "subscriptions",
                column: "plan_id");

            migrationBuilder.CreateIndex(
                name: "IX_subscriptions_tenant_id_id",
                schema: "platform",
                table: "subscriptions",
                columns: new[] { "tenant_id", "id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "subscriptions_current_idx",
                schema: "platform",
                table: "subscriptions",
                columns: new[] { "tenant_id", "status", "ends_at" },
                descending: new[] { false, false, true });

            migrationBuilder.CreateIndex(
                name: "IX_support_tickets_id_tenant_id",
                schema: "platform",
                table: "support_tickets",
                columns: new[] { "id", "tenant_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_support_tickets_tenant_id_receiver_id",
                schema: "platform",
                table: "support_tickets",
                columns: new[] { "tenant_id", "receiver_id" });

            migrationBuilder.CreateIndex(
                name: "IX_support_tickets_tenant_id_requester_id",
                schema: "platform",
                table: "support_tickets",
                columns: new[] { "tenant_id", "requester_id" });

            migrationBuilder.CreateIndex(
                name: "IX_system_incidents_created_by",
                schema: "platform",
                table: "system_incidents",
                column: "created_by");

            migrationBuilder.CreateIndex(
                name: "IX_tenant_ai_chat_messages_session_id",
                schema: "tenant",
                table: "tenant_ai_chat_messages",
                column: "session_id");

            migrationBuilder.CreateIndex(
                name: "IX_tenant_ai_chat_messages_tenant_id",
                schema: "tenant",
                table: "tenant_ai_chat_messages",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "IX_tenant_ai_chat_sessions_identity_user_id",
                schema: "tenant",
                table: "tenant_ai_chat_sessions",
                column: "identity_user_id");

            migrationBuilder.CreateIndex(
                name: "IX_tenant_ai_chat_sessions_tenant_id1",
                schema: "tenant",
                table: "tenant_ai_chat_sessions",
                column: "tenant_id1");

            migrationBuilder.CreateIndex(
                name: "IX_tenant_ai_chat_sessions_user_id",
                schema: "tenant",
                table: "tenant_ai_chat_sessions",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_tenant_apartments_tenant_id_apartment_no_building_id",
                schema: "tenant",
                table: "tenant_apartments",
                columns: new[] { "tenant_id", "apartment_no", "building_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_tenant_apartments_tenant_id_building_id",
                schema: "tenant",
                table: "tenant_apartments",
                columns: new[] { "tenant_id", "building_id" });

            migrationBuilder.CreateIndex(
                name: "IX_tenant_apartments_tenant_id_id",
                schema: "tenant",
                table: "tenant_apartments",
                columns: new[] { "tenant_id", "id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_tenant_billing_periods_tenant_id_created_by",
                schema: "tenant",
                table: "tenant_billing_periods",
                columns: new[] { "tenant_id", "created_by" });

            migrationBuilder.CreateIndex(
                name: "IX_tenant_billing_periods_tenant_id_id",
                schema: "tenant",
                table: "tenant_billing_periods",
                columns: new[] { "tenant_id", "id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_tenant_buildings_tenant_id_id",
                schema: "tenant",
                table: "tenant_buildings",
                columns: new[] { "tenant_id", "id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_tenant_invitations_tenant_id_email",
                schema: "tenant",
                table: "tenant_invitations",
                columns: new[] { "tenant_id", "email" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_tenant_invitations_tenant_id_id",
                schema: "tenant",
                table: "tenant_invitations",
                columns: new[] { "tenant_id", "id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_tenant_invitations_tenant_id_invited_by",
                schema: "tenant",
                table: "tenant_invitations",
                columns: new[] { "tenant_id", "invited_by" });

            migrationBuilder.CreateIndex(
                name: "IX_tenant_invitations_tenant_role_id",
                schema: "tenant",
                table: "tenant_invitations",
                column: "tenant_role_id");

            migrationBuilder.CreateIndex(
                name: "IX_tenant_invitations_token_hash",
                schema: "tenant",
                table: "tenant_invitations",
                column: "token_hash",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_tenant_invoice_lines_tenant_id_invoice_id",
                schema: "tenant",
                table: "tenant_invoice_lines",
                columns: new[] { "tenant_id", "invoice_id" });

            migrationBuilder.CreateIndex(
                name: "IX_tenant_invoice_lines_tenant_id_service_id",
                schema: "tenant",
                table: "tenant_invoice_lines",
                columns: new[] { "tenant_id", "service_id" });

            migrationBuilder.CreateIndex(
                name: "IX_tenant_invoices_billing_period_id_apartment_id",
                schema: "tenant",
                table: "tenant_invoices",
                columns: new[] { "billing_period_id", "apartment_id" },
                unique: true)
                .Annotation("Npgsql:NullsDistinct", false);

            migrationBuilder.CreateIndex(
                name: "IX_tenant_invoices_identity_user_id",
                schema: "tenant",
                table: "tenant_invoices",
                column: "identity_user_id");

            migrationBuilder.CreateIndex(
                name: "IX_tenant_invoices_tenant_id_apartment_id",
                schema: "tenant",
                table: "tenant_invoices",
                columns: new[] { "tenant_id", "apartment_id" });

            migrationBuilder.CreateIndex(
                name: "IX_tenant_invoices_tenant_id_billing_contact_id",
                schema: "tenant",
                table: "tenant_invoices",
                columns: new[] { "tenant_id", "billing_contact_id" });

            migrationBuilder.CreateIndex(
                name: "IX_tenant_invoices_tenant_id_billing_period_id",
                schema: "tenant",
                table: "tenant_invoices",
                columns: new[] { "tenant_id", "billing_period_id" });

            migrationBuilder.CreateIndex(
                name: "IX_tenant_invoices_tenant_id_id",
                schema: "tenant",
                table: "tenant_invoices",
                columns: new[] { "tenant_id", "id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_tenant_invoices_tenant_id_invoice_no",
                schema: "tenant",
                table: "tenant_invoices",
                columns: new[] { "tenant_id", "invoice_no" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_tenant_payment_orders_order_code",
                schema: "tenant",
                table: "tenant_payment_orders",
                column: "order_code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_tenant_payment_orders_provider_payment_link_id",
                schema: "tenant",
                table: "tenant_payment_orders",
                column: "provider_payment_link_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_tenant_payment_orders_tenant_id_provider_config_id",
                schema: "tenant",
                table: "tenant_payment_orders",
                columns: new[] { "tenant_id", "provider_config_id" });

            migrationBuilder.CreateIndex(
                name: "IX_tenant_payment_orders_tenant_id_request_by",
                schema: "tenant",
                table: "tenant_payment_orders",
                columns: new[] { "tenant_id", "request_by" });

            migrationBuilder.CreateIndex(
                name: "IX_tenant_payment_orders_tenant_id_tenant_invoice_id",
                schema: "tenant",
                table: "tenant_payment_orders",
                columns: new[] { "tenant_id", "tenant_invoice_id" });

            migrationBuilder.CreateIndex(
                name: "payment_orders_idx",
                schema: "tenant",
                table: "tenant_payment_orders",
                columns: new[] { "tenant_id", "status", "expires_at" },
                filter: "\"status\" IN ('pending'::tenant.payment_order_status, 'underpaid'::tenant.payment_order_status)");

            migrationBuilder.CreateIndex(
                name: "IX_tenant_payment_transactions_tenant_id_payment_order_id",
                schema: "tenant",
                table: "tenant_payment_transactions",
                columns: new[] { "tenant_id", "payment_order_id" });

            migrationBuilder.CreateIndex(
                name: "IX_tenant_payment_transactions_tenant_id1",
                schema: "tenant",
                table: "tenant_payment_transactions",
                column: "tenant_id1");

            migrationBuilder.CreateIndex(
                name: "IX_tenant_payment_transactions_tenant_payment_order_id",
                schema: "tenant",
                table: "tenant_payment_transactions",
                column: "tenant_payment_order_id");

            migrationBuilder.CreateIndex(
                name: "IX_tenant_payroll_details_tenant_id_id",
                schema: "tenant",
                table: "tenant_payroll_details",
                columns: new[] { "tenant_id", "id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_tenant_payroll_details_tenant_id_payroll_id_recipient_id",
                schema: "tenant",
                table: "tenant_payroll_details",
                columns: new[] { "tenant_id", "payroll_id", "recipient_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_tenant_payroll_details_tenant_id_recipient_id",
                schema: "tenant",
                table: "tenant_payroll_details",
                columns: new[] { "tenant_id", "recipient_id" });

            migrationBuilder.CreateIndex(
                name: "IX_tenant_payrolls_tenant_id_approved_by",
                schema: "tenant",
                table: "tenant_payrolls",
                columns: new[] { "tenant_id", "approved_by" });

            migrationBuilder.CreateIndex(
                name: "IX_tenant_payrolls_tenant_id_created_by",
                schema: "tenant",
                table: "tenant_payrolls",
                columns: new[] { "tenant_id", "created_by" });

            migrationBuilder.CreateIndex(
                name: "IX_tenant_payrolls_tenant_id_id",
                schema: "tenant",
                table: "tenant_payrolls",
                columns: new[] { "tenant_id", "id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_tenant_payrolls_tenant_id_issued_by",
                schema: "tenant",
                table: "tenant_payrolls",
                columns: new[] { "tenant_id", "issued_by" });

            migrationBuilder.CreateIndex(
                name: "IX_tenant_payrolls_tenant_id_rejected_by",
                schema: "tenant",
                table: "tenant_payrolls",
                columns: new[] { "tenant_id", "rejected_by" });

            migrationBuilder.CreateIndex(
                name: "IX_tenant_payrolls_tenant_id_submitted_by",
                schema: "tenant",
                table: "tenant_payrolls",
                columns: new[] { "tenant_id", "submitted_by" });

            migrationBuilder.CreateIndex(
                name: "IX_tenant_residents_tenant_id_citizen_id",
                schema: "tenant",
                table: "tenant_residents",
                columns: new[] { "tenant_id", "citizen_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_tenant_residents_tenant_id_id",
                schema: "tenant",
                table: "tenant_residents",
                columns: new[] { "tenant_id", "id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_tenant_residents_tenant_id_user_id",
                schema: "tenant",
                table: "tenant_residents",
                columns: new[] { "tenant_id", "user_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_tenant_role_permission_permission_code",
                schema: "tenant",
                table: "tenant_role_permission",
                column: "permission_code");

            migrationBuilder.CreateIndex(
                name: "IX_tenant_role_permission_tenant_id",
                schema: "tenant",
                table: "tenant_role_permission",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "IX_tenant_roles_code_tenant_id",
                schema: "tenant",
                table: "tenant_roles",
                columns: new[] { "code", "tenant_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_tenant_roles_tenant_id",
                schema: "tenant",
                table: "tenant_roles",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "IX_tenant_services_tenant_id_code",
                schema: "tenant",
                table: "tenant_services",
                columns: new[] { "tenant_id", "code" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_tenant_services_tenant_id_id",
                schema: "tenant",
                table: "tenant_services",
                columns: new[] { "tenant_id", "id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_tenant_staff_profiles_bank_account_number",
                schema: "tenant",
                table: "tenant_staff_profiles",
                column: "bank_account_number",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_tenant_staff_profiles_tenant_id_id",
                schema: "tenant",
                table: "tenant_staff_profiles",
                columns: new[] { "tenant_id", "id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_tenant_staff_profiles_tenant_id_user_id",
                schema: "tenant",
                table: "tenant_staff_profiles",
                columns: new[] { "tenant_id", "user_id" });

            migrationBuilder.CreateIndex(
                name: "IX_tenant_staff_time_sheets_tenant_id_id",
                schema: "tenant",
                table: "tenant_staff_time_sheets",
                columns: new[] { "tenant_id", "id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_tenant_staff_time_sheets_tenant_id_staff_id_date_month_date~",
                schema: "tenant",
                table: "tenant_staff_time_sheets",
                columns: new[] { "tenant_id", "staff_id", "date_month", "date_year" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_tenant_status_histories_tenant_id_changed_by",
                schema: "tenant",
                table: "tenant_status_histories",
                columns: new[] { "tenant_id", "changed_by" });

            migrationBuilder.CreateIndex(
                name: "IX_tenant_status_histories_tenant_id_id",
                schema: "tenant",
                table: "tenant_status_histories",
                columns: new[] { "tenant_id", "id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_tenant_user_roles_granted_by",
                schema: "tenant",
                table: "tenant_user_roles",
                column: "granted_by");

            migrationBuilder.CreateIndex(
                name: "IX_tenant_user_roles_role_id",
                schema: "tenant",
                table: "tenant_user_roles",
                column: "role_id");

            migrationBuilder.CreateIndex(
                name: "IX_tenant_user_roles_tenant_role_role",
                schema: "tenant",
                table: "tenant_user_roles",
                column: "tenant_role_role");

            migrationBuilder.CreateIndex(
                name: "IX_tenant_user_roles_user_id",
                schema: "tenant",
                table: "tenant_user_roles",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_tenant_webhook_events_tenant_id_provider_config_id",
                schema: "tenant",
                table: "tenant_webhook_events",
                columns: new[] { "tenant_id", "provider_config_id" });

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
                name: "IX_users_email",
                schema: "identity",
                table: "users",
                column: "email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_users_phone_number",
                schema: "identity",
                table: "users",
                column: "phone_number",
                unique: true);

			//mapping Exclusion Contraint
			//1. ApartmentResident - Chống chồng chất thời gian
			migrationBuilder.Sql(@"
                ALTER TABLE tenant.apartment_residents
                ADD CONSTRAINT ex_apartment_resident_no_overlap
                EXCLUDE USING gist
                (
                    tenant_id WITH =,
                    apartment_id WITH =,
                    resident_id WITH=,
                    occupancy_period WITH &&
                );
            ");

            //2. TenantBillingPeriod - cho phép tạo bill mới ghi đè lên bull cũ đã hủy
            migrationBuilder.Sql(@"
                ALTER TABLE tenant.tenant_billing_periods
                ADD CONSTRAINT ex_tenant_billing_period_no_overlap
                EXCLUSION USING gist
                (
                    tenant_id WITH =,
                    period WITH =&&
                )
                WHERE (
                    status <> 'cancelled'::tenant.billing_period_status
                );
            ");

            //3. ApartmentContract
            migrationBuilder.Sql(@"
                ALTER TABLE tenant.apartment_contracts
                ADD CONSTRAINT ex_apartment_contracts_no_overlap
                EXCLUSION USING gist
                (
                    tenant_id WITH=,
                    apartment_id WITH,
                    contract_period WITH &&
                )
                WHERE (
                    status = 'active'::tenant.tenant_contract_status
                    AND is_deleted = false
                );
            ");

		}

		/// <inheritdoc />
		protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "apartment_contracts",
                schema: "tenant");

            migrationBuilder.DropTable(
                name: "IdentityUsers");

            migrationBuilder.DropTable(
                name: "incident_affected_tenants",
                schema: "platform");

            migrationBuilder.DropTable(
                name: "incident_updates",
                schema: "platform");

            migrationBuilder.DropTable(
                name: "knowledge_documents",
                schema: "tenant");

            migrationBuilder.DropTable(
                name: "platform_invitations",
                schema: "platform");

            migrationBuilder.DropTable(
                name: "platform_role_permissions",
                schema: "platform");

            migrationBuilder.DropTable(
                name: "platform_user_role",
                schema: "platform");

            migrationBuilder.DropTable(
                name: "resident_complaints",
                schema: "tenant");

            migrationBuilder.DropTable(
                name: "subscription_events",
                schema: "platform");

            migrationBuilder.DropTable(
                name: "subscription_payment_transaction_configs",
                schema: "platform");

            migrationBuilder.DropTable(
                name: "subscription_payment_webhook_events",
                schema: "platform");

            migrationBuilder.DropTable(
                name: "support_tickets",
                schema: "platform");

            migrationBuilder.DropTable(
                name: "tenant_ai_chat_messages",
                schema: "tenant");

            migrationBuilder.DropTable(
                name: "tenant_invitations",
                schema: "tenant");

            migrationBuilder.DropTable(
                name: "tenant_invoice_lines",
                schema: "tenant");

            migrationBuilder.DropTable(
                name: "tenant_payment_transactions",
                schema: "tenant");

            migrationBuilder.DropTable(
                name: "tenant_payroll_details",
                schema: "tenant");

            migrationBuilder.DropTable(
                name: "tenant_role_permission",
                schema: "tenant");

            migrationBuilder.DropTable(
                name: "tenant_staff_time_sheets",
                schema: "tenant");

            migrationBuilder.DropTable(
                name: "tenant_status_histories",
                schema: "tenant");

            migrationBuilder.DropTable(
                name: "tenant_user_roles",
                schema: "tenant");

            migrationBuilder.DropTable(
                name: "tenant_webhook_events",
                schema: "tenant");

            migrationBuilder.DropTable(
                name: "system_incidents",
                schema: "platform");

            migrationBuilder.DropTable(
                name: "platform_permissions",
                schema: "platform");

            migrationBuilder.DropTable(
                name: "platform_roles",
                schema: "platform");

            migrationBuilder.DropTable(
                name: "apartment_residents",
                schema: "tenant");

            migrationBuilder.DropTable(
                name: "subscription_payment_orders",
                schema: "platform");

            migrationBuilder.DropTable(
                name: "tenant_ai_chat_sessions",
                schema: "tenant");

            migrationBuilder.DropTable(
                name: "tenant_services",
                schema: "tenant");

            migrationBuilder.DropTable(
                name: "tenant_payment_orders",
                schema: "tenant");

            migrationBuilder.DropTable(
                name: "tenant_payrolls",
                schema: "tenant");

            migrationBuilder.DropTable(
                name: "tenant_permissions",
                schema: "tenant");

            migrationBuilder.DropTable(
                name: "tenant_staff_profiles",
                schema: "tenant");

            migrationBuilder.DropTable(
                name: "tenant_roles",
                schema: "tenant");

            migrationBuilder.DropTable(
                name: "platform_payment_provider_configs",
                schema: "platform");

            migrationBuilder.DropTable(
                name: "subscription_invoices",
                schema: "platform");

            migrationBuilder.DropTable(
                name: "tenant_invoices",
                schema: "tenant");

            migrationBuilder.DropTable(
                name: "tenant_payment_provider_config",
                schema: "tenant");

            migrationBuilder.DropTable(
                name: "subscriptions",
                schema: "platform");

            migrationBuilder.DropTable(
                name: "tenant_apartments",
                schema: "tenant");

            migrationBuilder.DropTable(
                name: "tenant_billing_periods",
                schema: "tenant");

            migrationBuilder.DropTable(
                name: "tenant_residents",
                schema: "tenant");

            migrationBuilder.DropTable(
                name: "subscription_plans",
                schema: "platform");

            migrationBuilder.DropTable(
                name: "tenant_buildings",
                schema: "tenant");

            migrationBuilder.DropTable(
                name: "users",
                schema: "identity");

            migrationBuilder.DropTable(
                name: "tenants",
                schema: "platform");

            migrationBuilder.DropSequence(
                name: "payos_order_code_seq",
                schema: "tenant");

            migrationBuilder.DropSequence(
                name: "subscription_payment_order_code_seq",
                schema: "platform");

            //mapping Exclusion Contraint
            //1. ApartmentResident - Chống chồng chất thời gian
            migrationBuilder.Sql(@"
                ALTER TABLE tenant.apartment_residents
                DROP CONSTRAINT IF EXISTS ex_apartment_residents_no_overlap;
            ");

			//2. TenantBillingPeriod - cho phép tạo bill mới ghi đè lên bull cũ đã hủy
			migrationBuilder.Sql(@"
                ALTER TABLE tenant_tenant_billing_periods
                DROP CONSTRAINT IF EXISTS ex_billing_periods_no_overlap;
            ");

			//3. ApartmentContract
			migrationBuilder.Sql(@"
                ALTER TABLE tenant_apartment_contracts
                DROP CONSTRAINT IF EXISTS ex_apartment_contracts_no_overlap;
            ");

		}
	}
}
