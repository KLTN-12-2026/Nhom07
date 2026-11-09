CREATE TABLE IF NOT EXISTS "__EFMigrationsHistory" (
    "MigrationId" character varying(150) NOT NULL,
    "ProductVersion" character varying(32) NOT NULL,
    CONSTRAINT "PK___EFMigrationsHistory" PRIMARY KEY ("MigrationId")
);

START TRANSACTION;


DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261109115802_InitialCreate') THEN
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
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261109115802_InitialCreate') THEN
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
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261109115802_InitialCreate') THEN
        IF NOT EXISTS(SELECT 1 FROM pg_namespace WHERE nspname = 'tenant') THEN
            CREATE SCHEMA tenant;
        END IF;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261109115802_InitialCreate') THEN
        IF NOT EXISTS(SELECT 1 FROM pg_namespace WHERE nspname = 'payment') THEN
            CREATE SCHEMA payment;
        END IF;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261109115802_InitialCreate') THEN
        IF NOT EXISTS(SELECT 1 FROM pg_namespace WHERE nspname = 'security') THEN
            CREATE SCHEMA security;
        END IF;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261109115802_InitialCreate') THEN
        IF NOT EXISTS(SELECT 1 FROM pg_namespace WHERE nspname = 'platform') THEN
            CREATE SCHEMA platform;
        END IF;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261109115802_InitialCreate') THEN
        IF NOT EXISTS(SELECT 1 FROM pg_namespace WHERE nspname = 'identity') THEN
            CREATE SCHEMA identity;
        END IF;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261109115802_InitialCreate') THEN
        IF NOT EXISTS(SELECT 1 FROM pg_namespace WHERE nspname = 'identity') THEN
            CREATE SCHEMA identity;
        END IF;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261109115802_InitialCreate') THEN
    CREATE TYPE identity.user_status_enum AS ENUM ('active', 'locked', 'inviting');
        IF NOT EXISTS(SELECT 1 FROM pg_namespace WHERE nspname = 'payment') THEN
            CREATE SCHEMA payment;
        END IF;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261109115802_InitialCreate') THEN
    CREATE TYPE payment.payment_request_status AS ENUM ('pending', 'underpaid', 'paid', 'cancelled', 'expired', 'failed');
        IF NOT EXISTS(SELECT 1 FROM pg_namespace WHERE nspname = 'payment') THEN
            CREATE SCHEMA payment;
        END IF;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261109115802_InitialCreate') THEN
    CREATE TYPE payment.payment_transaction_status AS ENUM ('received', 'verified', 'rejected');
        IF NOT EXISTS(SELECT 1 FROM pg_namespace WHERE nspname = 'payment') THEN
            CREATE SCHEMA payment;
        END IF;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261109115802_InitialCreate') THEN
    CREATE TYPE payment.payout_batch_status AS ENUM ('created', 'submitting', 'processing', 'partially_succeeded', 'succeeded', 'failed', 'cancelled');
        IF NOT EXISTS(SELECT 1 FROM pg_namespace WHERE nspname = 'payment') THEN
            CREATE SCHEMA payment;
        END IF;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261109115802_InitialCreate') THEN
    CREATE TYPE payment.payout_item_status_enum AS ENUM ('created', 'processing', 'succeeded', 'failed', 'cancelled');
        IF NOT EXISTS(SELECT 1 FROM pg_namespace WHERE nspname = 'payment') THEN
            CREATE SCHEMA payment;
        END IF;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261109115802_InitialCreate') THEN
    CREATE TYPE payment.processing_status_enum AS ENUM ('received', 'processing', 'succeeded', 'failed');
        IF NOT EXISTS(SELECT 1 FROM pg_namespace WHERE nspname = 'payment') THEN
            CREATE SCHEMA payment;
        END IF;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261109115802_InitialCreate') THEN
    CREATE TYPE payment.provider_owner_scope AS ENUM ('platform', 'tenant');
        IF NOT EXISTS(SELECT 1 FROM pg_namespace WHERE nspname = 'platform') THEN
            CREATE SCHEMA platform;
        END IF;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261109115802_InitialCreate') THEN
    CREATE TYPE platform.subscription_invoice_status AS ENUM ('issued', 'partially_paid', 'paid', 'overdue', 'cancelled');
        IF NOT EXISTS(SELECT 1 FROM pg_namespace WHERE nspname = 'platform') THEN
            CREATE SCHEMA platform;
        END IF;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261109115802_InitialCreate') THEN
    CREATE TYPE platform.subscription_payment_attempt_status AS ENUM ('pending', 'partially_paid', 'paid', 'cancelled', 'failed');
        IF NOT EXISTS(SELECT 1 FROM pg_namespace WHERE nspname = 'platform') THEN
            CREATE SCHEMA platform;
        END IF;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261109115802_InitialCreate') THEN
    CREATE TYPE platform.subscription_status_enum AS ENUM ('trial', 'active', 'past_due', 'cancelled', 'expired');
        IF NOT EXISTS(SELECT 1 FROM pg_namespace WHERE nspname = 'platform') THEN
            CREATE SCHEMA platform;
        END IF;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261109115802_InitialCreate') THEN
    CREATE TYPE platform.tenant_status_enum AS ENUM ('trial', 'active', 'suspended', 'closed');
        IF NOT EXISTS(SELECT 1 FROM pg_namespace WHERE nspname = 'security') THEN
            CREATE SCHEMA security;
        END IF;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261109115802_InitialCreate') THEN
    CREATE TYPE security.role_scope_enum AS ENUM ('platform', 'tenant');
        IF NOT EXISTS(SELECT 1 FROM pg_namespace WHERE nspname = 'tenant') THEN
            CREATE SCHEMA tenant;
        END IF;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261109115802_InitialCreate') THEN
    CREATE TYPE tenant.invitation_status_enum AS ENUM ('accepted', 'sent', 'revoked', 'expired');
        IF NOT EXISTS(SELECT 1 FROM pg_namespace WHERE nspname = 'tenant') THEN
            CREATE SCHEMA tenant;
        END IF;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261109115802_InitialCreate') THEN
    CREATE TYPE tenant.invoice_payment_attempt_status AS ENUM ('pending', 'partially_paid', 'paid', 'cancelled', 'failed');
        IF NOT EXISTS(SELECT 1 FROM pg_namespace WHERE nspname = 'tenant') THEN
            CREATE SCHEMA tenant;
        END IF;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261109115802_InitialCreate') THEN
    CREATE TYPE tenant.payroll_disbursement_item_status AS ENUM ('ready', 'processing', 'succeeded', 'failed');
        IF NOT EXISTS(SELECT 1 FROM pg_namespace WHERE nspname = 'tenant') THEN
            CREATE SCHEMA tenant;
        END IF;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261109115802_InitialCreate') THEN
    CREATE TYPE tenant.payroll_disbursement_status AS ENUM ('ready', 'issuing', 'processing', 'partially_succeeded', 'succeeded', 'failed');
        IF NOT EXISTS(SELECT 1 FROM pg_namespace WHERE nspname = 'tenant') THEN
            CREATE SCHEMA tenant;
        END IF;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261109115802_InitialCreate') THEN
    CREATE TYPE tenant.payroll_status_enum AS ENUM ('draft', 'pending_approval', 'approved', 'rejected', 'published');
        IF NOT EXISTS(SELECT 1 FROM pg_namespace WHERE nspname = 'tenant') THEN
            CREATE SCHEMA tenant;
        END IF;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261109115802_InitialCreate') THEN
    CREATE TYPE tenant.tenant_invoice_status AS ENUM ('draft', 'pending_approval', 'approved', 'rejected', 'published', 'paid', 'unpaid');
    CREATE EXTENSION IF NOT EXISTS citext;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261109115802_InitialCreate') THEN
    CREATE TABLE security.permissions (
        scope security.role_scope_enum NOT NULL,
        code character varying(120) NOT NULL,
        description text NOT NULL,
        CONSTRAINT "PK_permissions" PRIMARY KEY (scope, code),
        CONSTRAINT ck_permissions_code_scope CHECK ((scope = 'platform' AND code LIKE 'platform.%') OR (scope = 'tenant' AND code LIKE 'tenant.%'))
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261109115802_InitialCreate') THEN
    CREATE TABLE platform.tenants (
        id uuid NOT NULL DEFAULT (gen_random_uuid()),
        code character varying(30) NOT NULL,
        slug citext NOT NULL,
        legal_name character varying(255) NOT NULL,
        tax_code character varying(30) NOT NULL,
        address text NOT NULL,
        status platform.tenant_status_enum NOT NULL,
        trial_ends_at timestamptz,
        created_at timestamp with time zone NOT NULL DEFAULT (now()),
        updated_at timestamp with time zone DEFAULT (now()),
        CONSTRAINT "PK_tenants" PRIMARY KEY (id),
        CONSTRAINT "CK_tenants_slug_format" CHECK (slug ~ '^[a-z0-9]+(?:-[a-z0-9]+)*$'),
        CONSTRAINT ck_tenants_trial CHECK (status <> 'trial' OR trial_ends_at IS NOT NULL)
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261109115802_InitialCreate') THEN
    CREATE TABLE payment.provider_accounts (
        id uuid NOT NULL DEFAULT (gen_random_uuid()),
        tenant_id uuid,
        webhook_key uuid NOT NULL,
        code character varying(80) NOT NULL,
        provider character varying(30) NOT NULL DEFAULT 'payos',
        owner_scope payment.provider_owner_scope NOT NULL,
        merchant_code character varying(150),
        payment_credentials_ref text,
        payout_credentials_ref text,
        supports_collection boolean NOT NULL DEFAULT FALSE,
        supports_payout boolean NOT NULL DEFAULT FALSE,
        settings jsonb NOT NULL DEFAULT ('{}'::jsonb),
        is_active boolean NOT NULL DEFAULT TRUE,
        created_at timestamp with time zone NOT NULL DEFAULT (now()),
        updated_at timestamp with time zone DEFAULT (now()),
        CONSTRAINT "PK_provider_accounts" PRIMARY KEY (id),
        CONSTRAINT "AK_provider_accounts_owner_scope_id" UNIQUE (owner_scope, id),
        CONSTRAINT ck_provider_accounts_capability CHECK ((supports_collection OR supports_payout)),
        CONSTRAINT ck_provider_accounts_owner CHECK (((owner_scope = 'platform' AND tenant_id IS NULL) OR (owner_scope = 'tenant' AND tenant_id IS NOT NULL))),
        CONSTRAINT ck_provider_accounts_payout_secret CHECK ((NOT supports_payout OR payout_credentials_ref IS NOT NULL)),
        CONSTRAINT ck_provider_accounts_provider CHECK ((provider = 'payos')),
        CONSTRAINT ck_provider_accounts_settings CHECK ((jsonb_typeof(settings) = 'object')),
        CONSTRAINT ck_provider_accounts_supports_collection_secret CHECK ((NOT supports_collection OR payment_credentials_ref IS NOT NULL)),
        CONSTRAINT "FK_provider_accounts_tenants_tenant_id" FOREIGN KEY (tenant_id) REFERENCES platform.tenants (id) ON DELETE RESTRICT
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261109115802_InitialCreate') THEN
    CREATE TABLE security.roles (
        id uuid NOT NULL DEFAULT (gen_random_uuid()),
        scope security.role_scope_enum NOT NULL,
        tenant_id uuid,
        code character varying(80) NOT NULL,
        name character varying(150) NOT NULL,
        is_deleted boolean NOT NULL DEFAULT FALSE,
        is_system boolean NOT NULL DEFAULT FALSE,
        CONSTRAINT "PK_roles" PRIMARY KEY (id),
        CONSTRAINT "AK_roles_scope_id" UNIQUE (scope, id),
        CONSTRAINT ck_roles_owner_scope CHECK ((scope = 'platform' AND tenant_id IS NULL) OR (scope = 'tenant' AND tenant_id IS NOT NULL)),
        CONSTRAINT ck_roles_platform_code CHECK (scope <> 'platform' OR code = 'PLATFORM_ADMIN'),
        CONSTRAINT "FK_roles_tenants_tenant_id" FOREIGN KEY (tenant_id) REFERENCES platform.tenants (id) ON DELETE RESTRICT
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261109115802_InitialCreate') THEN
    CREATE TABLE identity.users (
        id uuid NOT NULL DEFAULT (gen_random_uuid()),
        full_name character varying(200) NOT NULL,
        avatar_url text,
        status identity.user_status_enum NOT NULL,
        citizen_id character varying(50),
        employeement_contract text,
        date_of_birth date,
        bank_name character varying(100),
        bank_account_number character varying(50),
        is_deleted boolean NOT NULL DEFAULT FALSE,
        deleted_at timestamp with time zone,
        created_at timestamp with time zone NOT NULL DEFAULT (now()),
        updated_at timestamp with time zone DEFAULT (now()),
        tenant_id uuid,
        user_name character varying(256),
        normalized_user_name character varying(256),
        email character varying(256),
        normalized_email character varying(256),
        email_confirmed boolean NOT NULL,
        password_hash text,
        security_stamp text,
        concurrency_stamp text,
        phone_number text,
        phone_number_confirmed boolean NOT NULL,
        two_factor_enabled boolean NOT NULL,
        lockout_end timestamp with time zone,
        lockout_enabled boolean NOT NULL,
        access_failed_count integer NOT NULL,
        CONSTRAINT "PK_users" PRIMARY KEY (id),
        CONSTRAINT "CK_users_bank_name_and_bank_account_name" CHECK ((bank_name IS NULL AND bank_account_number IS NULL) OR (bank_name IS NOT NULL AND bank_account_number IS NOT NULL)),
        CONSTRAINT "CK_users_email_or_phone_number_not_null" CHECK (email IS NOT NULL OR phone_number IS NOT NULL),
        CONSTRAINT "CK_users_is_deleted_and_deleted_at" CHECK ((is_deleted = false AND deleted_at IS NULL) OR (is_deleted = true AND deleted_at IS NOT NULL)),
        CONSTRAINT "FK_users_tenants_tenant_id" FOREIGN KEY (tenant_id) REFERENCES platform.tenants (id) ON DELETE RESTRICT
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261109115802_InitialCreate') THEN
    CREATE TABLE payment.payment_requests (
        id uuid NOT NULL DEFAULT (gen_random_uuid()),
        provider_account_id uuid NOT NULL,
        order_code bigint NOT NULL,
        provider_payment_link_id character varying(255),
        status payment.payment_request_status NOT NULL,
        amount numeric(18,2) NOT NULL,
        amount_received numeric(18,2) NOT NULL DEFAULT 0.0,
        currency char(3) NOT NULL DEFAULT 'VND',
        checkout_url text,
        qr_code text,
        return_url text,
        cancel_url text,
        expires_at timestamptz,
        cancelled_at timestamptz,
        cancellation_reason text,
        created_at timestamp with time zone NOT NULL DEFAULT (now()),
        updated_at timestamp with time zone DEFAULT (now()),
        CONSTRAINT "PK_payment_requests" PRIMARY KEY (id),
        CONSTRAINT "AK_payment_requests_provider_account_id_id" UNIQUE (provider_account_id, id),
        CONSTRAINT ck_payment_requests_amount CHECK ((amount > 0)),
        CONSTRAINT ck_payment_requests_amount_received CHECK ((amount_received >= 0)),
        CONSTRAINT ck_payment_requests_integer_vnd CHECK ((amount = trunc(amount) AND amount_received = trunc(amount_received))),
        CONSTRAINT ck_payment_requests_order_code CHECK ((order_code > 0)),
        CONSTRAINT "FK_payment_requests_provider_accounts_provider_account_id" FOREIGN KEY (provider_account_id) REFERENCES payment.provider_accounts (id) ON DELETE RESTRICT
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261109115802_InitialCreate') THEN
    CREATE TABLE payment.payout_batches (
        id uuid NOT NULL DEFAULT (gen_random_uuid()),
        provider_account_id uuid NOT NULL,
        reference_id character varying(150) NOT NULL,
        idempotency_key character varying(150) NOT NULL,
        provider_payout_id character varying(255),
        status payment.payout_batch_status NOT NULL,
        provider_status character varying(80),
        total_amount numeric(18,2) NOT NULL,
        estimated_fee numeric(18,2),
        currency char(3) NOT NULL DEFAULT 'VND',
        last_synced_at timestamptz,
        provider_payload jsonb NOT NULL DEFAULT ('{}'::jsonb),
        created_at timestamp with time zone NOT NULL DEFAULT (now()),
        updated_at timestamp with time zone DEFAULT (now()),
        CONSTRAINT "PK_payout_batches" PRIMARY KEY (id),
        CONSTRAINT "AK_payout_batches_provider_account_id_id" UNIQUE (provider_account_id, id),
        CONSTRAINT "FK_payout_batches_provider_accounts_provider_account_id" FOREIGN KEY (provider_account_id) REFERENCES payment.provider_accounts (id) ON DELETE RESTRICT
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261109115802_InitialCreate') THEN
    CREATE TABLE security.role_permissions (
        scope security.role_scope_enum NOT NULL,
        role_id uuid NOT NULL,
        permission_code character varying(120) NOT NULL,
        CONSTRAINT "PK_role_permissions" PRIMARY KEY (scope, role_id, permission_code),
        CONSTRAINT "FK_role_permissions_permissions_scope_permission_code" FOREIGN KEY (scope, permission_code) REFERENCES security.permissions (scope, code) ON DELETE RESTRICT,
        CONSTRAINT "FK_role_permissions_roles_scope_role_id" FOREIGN KEY (scope, role_id) REFERENCES security.roles (scope, id) ON DELETE RESTRICT
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261109115802_InitialCreate') THEN
    CREATE TABLE tenant.apartments (
        id uuid NOT NULL DEFAULT (gen_random_uuid()),
        area_m2 numeric(10,2) NOT NULL,
        apartment_no text NOT NULL,
        bedroom smallint NOT NULL,
        created_at timestamp with time zone NOT NULL DEFAULT (now()),
        updated_at timestamp with time zone DEFAULT (now()),
        is_deleted boolean NOT NULL DEFAULT FALSE,
        deleted_at timestamp with time zone,
        tenant_id uuid NOT NULL,
        apartment_renter uuid NOT NULL,
        CONSTRAINT "PK_apartments" PRIMARY KEY (id),
        CONSTRAINT "AK_apartments_tenant_id_id" UNIQUE (tenant_id, id),
        CONSTRAINT "CK_tenant_apartments_areaM2_positive" CHECK (area_m2 > 0),
        CONSTRAINT "CK_tenant_apartments_bedroom_positive" CHECK (bedroom >= 0),
        CONSTRAINT "FK_apartments_tenants_tenant_id" FOREIGN KEY (tenant_id) REFERENCES platform.tenants (id) ON DELETE RESTRICT,
        CONSTRAINT "FK_apartments_users_apartment_renter" FOREIGN KEY (apartment_renter) REFERENCES identity.users (id)
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261109115802_InitialCreate') THEN
    CREATE TABLE tenant.complaints (
        id uuid NOT NULL DEFAULT (gen_random_uuid()),
        title character varying(200) NOT NULL,
        message text NOT NULL,
        resolved_at timestamptz,
        created_at timestamp with time zone NOT NULL DEFAULT (now()),
        updated_at timestamp with time zone DEFAULT (now()),
        is_deleted boolean NOT NULL DEFAULT FALSE,
        deleted_at timestamp with time zone,
        tenant_id uuid NOT NULL,
        created_by_resident uuid NOT NULL,
        received_by_staff uuid,
        CONSTRAINT "PK_complaints" PRIMARY KEY (id),
        CONSTRAINT "FK_complaints_tenants_tenant_id" FOREIGN KEY (tenant_id) REFERENCES platform.tenants (id) ON DELETE RESTRICT,
        CONSTRAINT "FK_complaints_users_created_by_resident" FOREIGN KEY (created_by_resident) REFERENCES identity.users (id),
        CONSTRAINT "FK_complaints_users_received_by_staff" FOREIGN KEY (received_by_staff) REFERENCES identity.users (id)
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261109115802_InitialCreate') THEN
    CREATE TABLE tenant.invitations (
        id uuid NOT NULL DEFAULT (gen_random_uuid()),
        email citext NOT NULL,
        token_hash text NOT NULL,
        accepted_at timestamptz,
        expires_at timestamptz NOT NULL,
        revoked_at timestamptz,
        created_by uuid NOT NULL,
        role_id uuid NOT NULL,
        tenant_id uuid NOT NULL,
        target_user_id uuid,
        status tenant.invitation_status_enum NOT NULL,
        created_at timestamp with time zone NOT NULL DEFAULT (now()),
        is_deleted boolean NOT NULL DEFAULT FALSE,
        deleted_at timestamp with time zone,
        CONSTRAINT "PK_invitations" PRIMARY KEY (id),
        CONSTRAINT ck_invitations_accepted CHECK (status <> 'accepted' OR accepted_at IS NOT NULL),
        CONSTRAINT ck_invitations_expiry CHECK (expires_at > created_at),
        CONSTRAINT ck_invitations_revoked CHECK (status <> 'revoked' OR revoked_at IS NOT NULL),
        CONSTRAINT "FK_invitations_roles_role_id" FOREIGN KEY (role_id) REFERENCES security.roles (id),
        CONSTRAINT "FK_invitations_tenants_tenant_id" FOREIGN KEY (tenant_id) REFERENCES platform.tenants (id) ON DELETE RESTRICT,
        CONSTRAINT "FK_invitations_users_created_by" FOREIGN KEY (created_by) REFERENCES identity.users (id),
        CONSTRAINT "FK_invitations_users_target_user_id" FOREIGN KEY (target_user_id) REFERENCES identity.users (id)
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261109115802_InitialCreate') THEN
    CREATE TABLE tenant.payrolls (
        id uuid NOT NULL DEFAULT (gen_random_uuid()),
        title character varying(200) NOT NULL,
        status tenant.payroll_status_enum NOT NULL,
        version integer NOT NULL DEFAULT 1,
        netpay numeric(18,2) NOT NULL,
        created_by uuid NOT NULL,
        tenant_id uuid NOT NULL,
        is_deleted boolean NOT NULL DEFAULT FALSE,
        deleted_at timestamp with time zone,
        created_at timestamp with time zone NOT NULL DEFAULT (now()),
        updated_at timestamp with time zone DEFAULT (now()),
        published_at timestamptz,
        CONSTRAINT "PK_payrolls" PRIMARY KEY (id),
        CONSTRAINT "AK_payrolls_tenant_id_id" UNIQUE (tenant_id, id),
        CONSTRAINT ck_payrolls_version CHECK (version > 0),
        CONSTRAINT "CK_tenant_payrolls_netpay_positive" CHECK (netpay >= 0),
        CONSTRAINT "FK_payrolls_tenants_tenant_id" FOREIGN KEY (tenant_id) REFERENCES platform.tenants (id) ON DELETE RESTRICT,
        CONSTRAINT "FK_payrolls_users_created_by" FOREIGN KEY (created_by) REFERENCES identity.users (id)
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261109115802_InitialCreate') THEN
    CREATE TABLE tenant.rules (
        id uuid NOT NULL DEFAULT (gen_random_uuid()),
        title character varying(500) NOT NULL,
        source_url text NOT NULL,
        tenant_id uuid NOT NULL,
        imported_by uuid NOT NULL,
        CONSTRAINT "PK_rules" PRIMARY KEY (id),
        CONSTRAINT "FK_rules_tenants_tenant_id" FOREIGN KEY (tenant_id) REFERENCES platform.tenants (id) ON DELETE RESTRICT,
        CONSTRAINT "FK_rules_users_imported_by" FOREIGN KEY (imported_by) REFERENCES identity.users (id)
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261109115802_InitialCreate') THEN
    CREATE TABLE tenant.services (
        id uuid NOT NULL DEFAULT (gen_random_uuid()),
        code character varying(50) NOT NULL,
        name character varying(150) NOT NULL,
        price numeric(18,2) NOT NULL,
        created_by_manager uuid NOT NULL,
        deleted_by_manager uuid,
        updated_by_manager uuid,
        tenant_id uuid NOT NULL,
        is_deleted boolean NOT NULL DEFAULT FALSE,
        deleted_at timestamp with time zone,
        created_at timestamp with time zone NOT NULL DEFAULT (now()),
        updated_at timestamp with time zone DEFAULT (now()),
        CONSTRAINT "PK_services" PRIMARY KEY (id),
        CONSTRAINT "AK_services_tenant_id_id" UNIQUE (tenant_id, id),
        CONSTRAINT "FK_services_tenants_tenant_id" FOREIGN KEY (tenant_id) REFERENCES platform.tenants (id) ON DELETE RESTRICT,
        CONSTRAINT "FK_services_users_created_by_manager" FOREIGN KEY (created_by_manager) REFERENCES identity.users (id),
        CONSTRAINT "FK_services_users_deleted_by_manager" FOREIGN KEY (deleted_by_manager) REFERENCES identity.users (id),
        CONSTRAINT "FK_services_users_updated_by_manager" FOREIGN KEY (updated_by_manager) REFERENCES identity.users (id)
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261109115802_InitialCreate') THEN
    CREATE TABLE tenant.staff_time_sheets (
        id uuid NOT NULL DEFAULT (gen_random_uuid()),
        payroll_period timestamp with time zone NOT NULL,
        standard_working_day numeric(18,2) NOT NULL,
        paid_leave_day numeric(18,2) NOT NULL,
        unpaid_leave_day numeric(18,2) NOT NULL,
        overtime_hour_normal numeric(18,2) NOT NULL,
        overtime_hour_weekend numeric(18,2) NOT NULL,
        overtime_hour_holiday numeric(18,2) NOT NULL,
        bonus_amount numeric(18,2) NOT NULL,
        penalty_amount numeric(18,2) NOT NULL,
        note text,
        is_deleted boolean NOT NULL DEFAULT FALSE,
        deleted_at timestamp with time zone,
        created_at timestamp with time zone NOT NULL DEFAULT (now()),
        updated_at timestamp with time zone DEFAULT (now()),
        created_by_manager uuid NOT NULL,
        staff_id uuid NOT NULL,
        tenant_id uuid NOT NULL,
        CONSTRAINT "PK_staff_time_sheets" PRIMARY KEY (id),
        CONSTRAINT "CK_tenant_staff_time_sheets_bonus_amount_positive" CHECK (bonus_amount >= 0),
        CONSTRAINT "CK_tenant_staff_time_sheets_overtime_hour_holiday_positive" CHECK (overtime_hour_holiday >= 0),
        CONSTRAINT "CK_tenant_staff_time_sheets_overtime_hour_normal_positive" CHECK (overtime_hour_normal >= 0),
        CONSTRAINT "CK_tenant_staff_time_sheets_overtime_hour_weekend_positive" CHECK (overtime_hour_weekend >= 0),
        CONSTRAINT "CK_tenant_staff_time_sheets_paid_leave_day_positive" CHECK (paid_leave_day >= 0),
        CONSTRAINT "CK_tenant_staff_time_sheets_penalty_amount_positive" CHECK (penalty_amount >= 0),
        CONSTRAINT "CK_tenant_staff_time_sheets_standard_working_day_positive" CHECK (standard_working_day >= 0 AND standard_working_day <= 30),
        CONSTRAINT "CK_tenant_staff_time_sheets_unpaid_leave_day_positive" CHECK (unpaid_leave_day >= 0),
        CONSTRAINT "FK_staff_time_sheets_tenants_tenant_id" FOREIGN KEY (tenant_id) REFERENCES platform.tenants (id) ON DELETE RESTRICT,
        CONSTRAINT "FK_staff_time_sheets_users_created_by_manager" FOREIGN KEY (created_by_manager) REFERENCES identity.users (id),
        CONSTRAINT "FK_staff_time_sheets_users_staff_id" FOREIGN KEY (staff_id) REFERENCES identity.users (id)
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261109115802_InitialCreate') THEN
    CREATE TABLE platform.subscription_plans (
        id uuid NOT NULL DEFAULT (gen_random_uuid()),
        name character varying(50) NOT NULL,
        description text,
        billing_cycle_months smallint NOT NULL,
        price numeric(18,2) NOT NULL,
        feature_limits jsonb NOT NULL DEFAULT ('{}'::jsonb),
        is_available boolean NOT NULL DEFAULT TRUE,
        created_by_admin uuid NOT NULL,
        updated_by_admin uuid,
        deleted_by_admin uuid,
        created_at timestamp with time zone NOT NULL DEFAULT (now()),
        updated_at timestamp with time zone DEFAULT (now()),
        is_deleted boolean NOT NULL DEFAULT FALSE,
        deleted_at timestamp with time zone,
        CONSTRAINT "PK_subscription_plans" PRIMARY KEY (id),
        CONSTRAINT ck_subscription_plans_billing_cycle CHECK (billing_cycle_months IN (6, 12, 24)),
        CONSTRAINT "CK_subscription_plans_price" CHECK (price >= 0),
        CONSTRAINT "FK_subscription_plans_users_created_by_admin" FOREIGN KEY (created_by_admin) REFERENCES identity.users (id),
        CONSTRAINT "FK_subscription_plans_users_deleted_by_admin" FOREIGN KEY (deleted_by_admin) REFERENCES identity.users (id),
        CONSTRAINT "FK_subscription_plans_users_updated_by_admin" FOREIGN KEY (updated_by_admin) REFERENCES identity.users (id)
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261109115802_InitialCreate') THEN
    CREATE TABLE identity.user_claims (
        "Id" integer GENERATED BY DEFAULT AS IDENTITY,
        "UserId" uuid NOT NULL,
        "ClaimType" text,
        "ClaimValue" text,
        CONSTRAINT "PK_user_claims" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_user_claims_users_UserId" FOREIGN KEY ("UserId") REFERENCES identity.users (id) ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261109115802_InitialCreate') THEN
    CREATE TABLE identity.user_logins (
        "LoginProvider" text NOT NULL,
        "ProviderKey" text NOT NULL,
        "ProviderDisplayName" text,
        "UserId" uuid NOT NULL,
        CONSTRAINT "PK_user_logins" PRIMARY KEY ("LoginProvider", "ProviderKey"),
        CONSTRAINT "FK_user_logins_users_UserId" FOREIGN KEY ("UserId") REFERENCES identity.users (id) ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261109115802_InitialCreate') THEN
    CREATE TABLE security.user_roles (
        id uuid NOT NULL DEFAULT (gen_random_uuid()),
        scope security.role_scope_enum NOT NULL,
        tenant_id uuid,
        user_id uuid NOT NULL,
        role_id uuid NOT NULL,
        granted_by_user_id uuid,
        granted_at timestamptz NOT NULL,
        revoked_at timestamptz,
        CONSTRAINT "PK_user_roles" PRIMARY KEY (id),
        CONSTRAINT ck_user_roles_revoked_at CHECK (revoked_at IS NULL OR revoked_at >= granted_at),
        CONSTRAINT ck_user_roles_scope_tenant CHECK ((scope = 'platform' AND tenant_id IS NULL) OR (scope = 'tenant' AND tenant_id IS NOT NULL)),
        CONSTRAINT "FK_user_roles_roles_scope_role_id" FOREIGN KEY (scope, role_id) REFERENCES security.roles (scope, id) ON DELETE RESTRICT,
        CONSTRAINT "FK_user_roles_tenants_tenant_id" FOREIGN KEY (tenant_id) REFERENCES platform.tenants (id) ON DELETE RESTRICT,
        CONSTRAINT "FK_user_roles_users_granted_by_user_id" FOREIGN KEY (granted_by_user_id) REFERENCES identity.users (id) ON DELETE RESTRICT,
        CONSTRAINT "FK_user_roles_users_user_id" FOREIGN KEY (user_id) REFERENCES identity.users (id) ON DELETE RESTRICT
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261109115802_InitialCreate') THEN
    CREATE TABLE identity.user_tokens (
        "UserId" uuid NOT NULL,
        "LoginProvider" text NOT NULL,
        "Name" text NOT NULL,
        "Value" text,
        CONSTRAINT "PK_user_tokens" PRIMARY KEY ("UserId", "LoginProvider", "Name"),
        CONSTRAINT "FK_user_tokens_users_UserId" FOREIGN KEY ("UserId") REFERENCES identity.users (id) ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261109115802_InitialCreate') THEN
    CREATE TABLE payment.webhook_events (
        id uuid NOT NULL DEFAULT (gen_random_uuid()),
        provider_account_id uuid NOT NULL,
        payment_request_id uuid,
        event_key character varying(255),
        order_code bigint,
        provider_payment_link_id character varying(255),
        provider_reference character varying(255),
        signature_valid boolean NOT NULL DEFAULT FALSE,
        raw_payload text NOT NULL,
        processing_status payment.processing_status_enum NOT NULL,
        processing_attempts integer NOT NULL DEFAULT 0,
        last_attempt_at timestamptz,
        next_attempt_at timestamptz,
        received_at timestamptz NOT NULL DEFAULT (now()),
        processed_at timestamptz,
        processing_error text,
        CONSTRAINT "PK_webhook_events" PRIMARY KEY (id),
        CONSTRAINT "AK_webhook_events_provider_account_id_id" UNIQUE (provider_account_id, id),
        CONSTRAINT ck_webhook_event_attempt CHECK (processing_attempts >= 0),
        CONSTRAINT ck_webhook_event_event_key CHECK ((event_key IS NULL OR (signature_valid = true AND btrim(event_key) <> ''))),
        CONSTRAINT ck_webhook_event_processed CHECK ((processed_at IS NULL OR (signature_valid = true AND processing_status = 'succeeded' AND payment_request_id IS NOT NULL AND processing_error IS NULL))),
        CONSTRAINT ck_webhook_event_resolved_request CHECK ((payment_request_id IS NULL OR signature_valid = true)),
        CONSTRAINT "FK_webhook_events_payment_requests_provider_account_id_payment~" FOREIGN KEY (provider_account_id, payment_request_id) REFERENCES payment.payment_requests (provider_account_id, id) ON DELETE RESTRICT,
        CONSTRAINT "FK_webhook_events_provider_accounts_provider_account_id" FOREIGN KEY (provider_account_id) REFERENCES payment.provider_accounts (id) ON DELETE RESTRICT
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261109115802_InitialCreate') THEN
    CREATE TABLE payment.payout_items (
        id uuid NOT NULL DEFAULT (gen_random_uuid()),
        provider_account_id uuid NOT NULL,
        payout_batch_id uuid NOT NULL,
        reference_id character varying(150) NOT NULL,
        provider_transaction_id character varying(255),
        amount numeric(18,2) NOT NULL,
        currency char(3) NOT NULL DEFAULT 'VND',
        description character varying(255) NOT NULL,
        destination_bin character varying(12) NOT NULL,
        destination_account_ciphertext bytea NOT NULL,
        destination_account_last4 character varying(4) NOT NULL,
        destination_account_name character varying(255) NOT NULL,
        status payment.payout_item_status_enum NOT NULL,
        provider_status character varying(80),
        failure_code character varying(80),
        failure_message text,
        created_at timestamp with time zone NOT NULL DEFAULT (now()),
        updated_at timestamp with time zone DEFAULT (now()),
        CONSTRAINT "PK_payout_items" PRIMARY KEY (id),
        CONSTRAINT "AK_payout_items_provider_account_id_id" UNIQUE (provider_account_id, id),
        CONSTRAINT ck_payout_items_amount CHECK ((amount > 0)),
        CONSTRAINT ck_payout_items_currency CHECK ((currency = 'VND')),
        CONSTRAINT ck_payout_items_failure CHECK (status <> 'failed' OR failure_message IS NOT NULL OR failure_code IS NOT NULL),
        CONSTRAINT ck_payout_items_last4 CHECK ((destination_account_last4 ~ '^[0-9]{4}$')),
        CONSTRAINT "FK_payout_items_payout_batches_provider_account_id_payout_batc~" FOREIGN KEY (provider_account_id, payout_batch_id) REFERENCES payment.payout_batches (provider_account_id, id) ON DELETE RESTRICT,
        CONSTRAINT "FK_payout_items_provider_accounts_provider_account_id" FOREIGN KEY (provider_account_id) REFERENCES payment.provider_accounts (id) ON DELETE RESTRICT
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261109115802_InitialCreate') THEN
    CREATE TABLE tenant.contracts (
        id uuid NOT NULL DEFAULT (gen_random_uuid()),
        contract_no character varying(80) NOT NULL,
        contract_url text,
        starts_at timestamptz NOT NULL,
        ends_at timestamptz NOT NULL,
        tenant_id uuid NOT NULL,
        imported_by uuid NOT NULL,
        apartment_id uuid NOT NULL,
        created_at timestamp with time zone NOT NULL DEFAULT (now()),
        updated_at timestamp with time zone DEFAULT (now()),
        is_deleted boolean NOT NULL DEFAULT FALSE,
        deleted_at timestamp with time zone,
        CONSTRAINT "PK_contracts" PRIMARY KEY (id),
        CONSTRAINT ck_contracts_period CHECK (ends_at > starts_at),
        CONSTRAINT "FK_contracts_apartments_tenant_id_apartment_id" FOREIGN KEY (tenant_id, apartment_id) REFERENCES tenant.apartments (tenant_id, id),
        CONSTRAINT "FK_contracts_tenants_tenant_id" FOREIGN KEY (tenant_id) REFERENCES platform.tenants (id) ON DELETE RESTRICT,
        CONSTRAINT "FK_contracts_users_imported_by" FOREIGN KEY (imported_by) REFERENCES identity.users (id)
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261109115802_InitialCreate') THEN
    CREATE TABLE tenant.invoices (
        id uuid NOT NULL DEFAULT (gen_random_uuid()),
        invoice_no character varying(80) NOT NULL,
        status tenant.tenant_invoice_status NOT NULL,
        published_at timestamptz,
        billing_period timestamptz NOT NULL,
        payment_deadline_at timestamptz NOT NULL,
        total_amount numeric(18,2) NOT NULL,
        note text,
        deleted_by_staff uuid,
        updated_by_staff uuid,
        created_by_staff uuid NOT NULL,
        tenant_id uuid NOT NULL,
        apartment_id uuid NOT NULL,
        is_deleted boolean NOT NULL DEFAULT FALSE,
        deleted_at timestamp with time zone,
        created_at timestamp with time zone NOT NULL DEFAULT (now()),
        updated_at timestamp with time zone DEFAULT (now()),
        CONSTRAINT "PK_invoices" PRIMARY KEY (id),
        CONSTRAINT "AK_invoices_tenant_id_id" UNIQUE (tenant_id, id),
        CONSTRAINT "FK_invoices_apartments_tenant_id_apartment_id" FOREIGN KEY (tenant_id, apartment_id) REFERENCES tenant.apartments (tenant_id, id),
        CONSTRAINT "FK_invoices_tenants_tenant_id" FOREIGN KEY (tenant_id) REFERENCES platform.tenants (id) ON DELETE RESTRICT,
        CONSTRAINT "FK_invoices_users_created_by_staff" FOREIGN KEY (created_by_staff) REFERENCES identity.users (id),
        CONSTRAINT "FK_invoices_users_deleted_by_staff" FOREIGN KEY (deleted_by_staff) REFERENCES identity.users (id),
        CONSTRAINT "FK_invoices_users_updated_by_staff" FOREIGN KEY (updated_by_staff) REFERENCES identity.users (id)
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261109115802_InitialCreate') THEN
    CREATE TABLE tenant.payroll_details (
        id uuid NOT NULL DEFAULT (gen_random_uuid()),
        base_salary numeric(18,2) NOT NULL,
        allowance numeric(18,2) NOT NULL,
        deduction numeric(18,2) NOT NULL,
        staff_receive uuid NOT NULL,
        payroll_id uuid NOT NULL,
        tenant_id uuid NOT NULL,
        CONSTRAINT "PK_payroll_details" PRIMARY KEY (id),
        CONSTRAINT "AK_payroll_details_tenant_id_id" UNIQUE (tenant_id, id),
        CONSTRAINT "CK_tenant_payroll_details_allowance_positive" CHECK (allowance >= 0),
        CONSTRAINT "CK_tenant_payroll_details_base_salary_positive" CHECK (base_salary > 0),
        CONSTRAINT "CK_tenant_payroll_details_deduction_positive" CHECK (deduction >= 0),
        CONSTRAINT "FK_payroll_details_payrolls_tenant_id_payroll_id" FOREIGN KEY (tenant_id, payroll_id) REFERENCES tenant.payrolls (tenant_id, id),
        CONSTRAINT "FK_payroll_details_tenants_tenant_id" FOREIGN KEY (tenant_id) REFERENCES platform.tenants (id) ON DELETE RESTRICT,
        CONSTRAINT "FK_payroll_details_users_staff_receive" FOREIGN KEY (staff_receive) REFERENCES identity.users (id)
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261109115802_InitialCreate') THEN
    CREATE TABLE tenant.payroll_disbursements (
        id uuid NOT NULL DEFAULT (gen_random_uuid()),
        tenant_id uuid NOT NULL,
        payroll_id uuid NOT NULL,
        gateway_account_id uuid NOT NULL,
        gateway_payout_batch_id uuid,
        approved_payroll_version integer NOT NULL,
        approved_content_hash bytea NOT NULL,
        approved_by_user_id uuid NOT NULL,
        approved_at timestamptz NOT NULL,
        issued_by_user_id uuid,
        issued_at timestamptz,
        status tenant.payroll_disbursement_status NOT NULL,
        total_amount numeric(18,2) NOT NULL,
        created_at timestamp with time zone NOT NULL DEFAULT (now()),
        updated_at timestamp with time zone DEFAULT (now()),
        CONSTRAINT "PK_payroll_disbursements" PRIMARY KEY (id),
        CONSTRAINT "AK_payroll_disbursements_tenant_id_id" UNIQUE (tenant_id, id),
        CONSTRAINT ck_payroll_disbursement_amount CHECK ((total_amount > 0)),
        CONSTRAINT ck_payroll_disbursement_hash CHECK ((octet_length(approved_content_hash) = 32)),
        CONSTRAINT ck_payroll_disbursement_issued CHECK (((issued_at IS NULL AND issued_by_user_id IS NULL) OR (issued_at IS NOT NULL AND issued_by_user_id IS NOT NULL))),
        CONSTRAINT ck_payroll_disbursement_status_issued CHECK (status = 'ready' OR issued_at IS NOT NULL),
        CONSTRAINT ck_payroll_disbursement_version CHECK ((approved_payroll_version > 0)),
        CONSTRAINT "FK_payroll_disbursements_payout_batches_gateway_account_id_gat~" FOREIGN KEY (gateway_account_id, gateway_payout_batch_id) REFERENCES payment.payout_batches (provider_account_id, id) ON DELETE RESTRICT,
        CONSTRAINT "FK_payroll_disbursements_payrolls_tenant_id_payroll_id" FOREIGN KEY (tenant_id, payroll_id) REFERENCES tenant.payrolls (tenant_id, id) ON DELETE RESTRICT,
        CONSTRAINT "FK_payroll_disbursements_provider_accounts_gateway_account_id" FOREIGN KEY (gateway_account_id) REFERENCES payment.provider_accounts (id) ON DELETE RESTRICT,
        CONSTRAINT "FK_payroll_disbursements_tenants_tenant_id" FOREIGN KEY (tenant_id) REFERENCES platform.tenants (id) ON DELETE CASCADE,
        CONSTRAINT "FK_payroll_disbursements_users_approved_by_user_id" FOREIGN KEY (approved_by_user_id) REFERENCES identity.users (id) ON DELETE RESTRICT,
        CONSTRAINT "FK_payroll_disbursements_users_issued_by_user_id" FOREIGN KEY (issued_by_user_id) REFERENCES identity.users (id) ON DELETE RESTRICT
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261109115802_InitialCreate') THEN
    CREATE TABLE platform.subscriptions (
        id uuid NOT NULL DEFAULT (gen_random_uuid()),
        tenant_id uuid NOT NULL,
        plan_id uuid NOT NULL,
        billing_cycle_months_snapshot smallint NOT NULL,
        status platform.subscription_status_enum NOT NULL,
        starts_at date NOT NULL,
        ends_at date NOT NULL,
        cancelled_at timestamptz,
        CONSTRAINT "PK_subscriptions" PRIMARY KEY (id),
        CONSTRAINT "AK_subscriptions_tenant_id_id" UNIQUE (tenant_id, id),
        CONSTRAINT ck_subscriptions_billing_cycle CHECK (billing_cycle_months_snapshot > 0),
        CONSTRAINT ck_subscriptions_cancelled CHECK (status <> 'cancelled' OR cancelled_at IS NOT NULL),
        CONSTRAINT ck_subscriptions_period CHECK (ends_at > starts_at),
        CONSTRAINT "FK_subscriptions_subscription_plans_plan_id" FOREIGN KEY (plan_id) REFERENCES platform.subscription_plans (id) ON DELETE RESTRICT,
        CONSTRAINT "FK_subscriptions_tenants_tenant_id" FOREIGN KEY (tenant_id) REFERENCES platform.tenants (id) ON DELETE RESTRICT
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261109115802_InitialCreate') THEN
    CREATE TABLE payment.payment_transactions (
        id uuid NOT NULL DEFAULT (gen_random_uuid()),
        provider_account_id uuid NOT NULL,
        payment_request_id uuid,
        source_webhook_event_id uuid,
        provider_reference character varying(255),
        provider_transaction_id character varying(255) NOT NULL,
        amount numeric(18,2) NOT NULL,
        occurred_at timestamptz NOT NULL,
        payer_masked jsonb NOT NULL DEFAULT ('{}'::jsonb),
        provider_payload jsonb NOT NULL DEFAULT ('{}'::jsonb),
        created_at timestamp with time zone NOT NULL DEFAULT (now()),
        currency char(3) NOT NULL DEFAULT 'VND',
        status payment.payment_transaction_status NOT NULL,
        CONSTRAINT "PK_payment_transactions" PRIMARY KEY (id),
        CONSTRAINT "AK_payment_transactions_provider_account_id_id" UNIQUE (provider_account_id, id),
        CONSTRAINT ck_payment_transaction_amount CHECK ((amount > 0)),
        CONSTRAINT ck_payment_transaction_verified CHECK (status <> 'verified' OR payment_request_id IS NOT NULL),
        CONSTRAINT "FK_payment_transactions_payment_requests_provider_account_id_p~" FOREIGN KEY (provider_account_id, payment_request_id) REFERENCES payment.payment_requests (provider_account_id, id) ON DELETE RESTRICT,
        CONSTRAINT "FK_payment_transactions_provider_accounts_provider_account_id" FOREIGN KEY (provider_account_id) REFERENCES payment.provider_accounts (id) ON DELETE RESTRICT,
        CONSTRAINT "FK_payment_transactions_webhook_events_provider_account_id_sou~" FOREIGN KEY (provider_account_id, source_webhook_event_id) REFERENCES payment.webhook_events (provider_account_id, id) ON DELETE RESTRICT
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261109115802_InitialCreate') THEN
    CREATE TABLE tenant.invoice_details (
        id uuid NOT NULL DEFAULT (gen_random_uuid()),
        quantity integer NOT NULL,
        unit_price numeric(18,2) NOT NULL,
        amount numeric(18,2) NOT NULL,
        description text,
        invoice_id uuid NOT NULL,
        service_id uuid NOT NULL,
        tenant_id uuid NOT NULL,
        CONSTRAINT "PK_invoice_details" PRIMARY KEY (id),
        CONSTRAINT ck_invoice_details_amount CHECK (amount >= 0 AND amount = quantity * unit_price),
        CONSTRAINT ck_invoice_details_quantity CHECK (quantity > 0),
        CONSTRAINT ck_invoice_details_unit_price CHECK (unit_price >= 0),
        CONSTRAINT "FK_invoice_details_invoices_tenant_id_invoice_id" FOREIGN KEY (tenant_id, invoice_id) REFERENCES tenant.invoices (tenant_id, id) ON DELETE RESTRICT,
        CONSTRAINT "FK_invoice_details_services_tenant_id_service_id" FOREIGN KEY (tenant_id, service_id) REFERENCES tenant.services (tenant_id, id),
        CONSTRAINT "FK_invoice_details_tenants_tenant_id" FOREIGN KEY (tenant_id) REFERENCES platform.tenants (id) ON DELETE RESTRICT
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261109115802_InitialCreate') THEN
    CREATE TABLE tenant.invoice_payment_attempts (
        id uuid NOT NULL DEFAULT (gen_random_uuid()),
        tenant_id uuid NOT NULL,
        invoice_id uuid NOT NULL,
        gateway_account_id uuid NOT NULL,
        gateway_payment_request_id uuid NOT NULL,
        requested_by_user_id uuid NOT NULL,
        status tenant.invoice_payment_attempt_status NOT NULL,
        created_at timestamp with time zone NOT NULL DEFAULT (now()),
        CONSTRAINT "PK_invoice_payment_attempts" PRIMARY KEY (id),
        CONSTRAINT "AK_invoice_payment_attempts_tenant_id_gateway_account_id_id" UNIQUE (tenant_id, gateway_account_id, id),
        CONSTRAINT "FK_invoice_payment_attempts_invoices_tenant_id_invoice_id" FOREIGN KEY (tenant_id, invoice_id) REFERENCES tenant.invoices (tenant_id, id) ON DELETE RESTRICT,
        CONSTRAINT "FK_invoice_payment_attempts_payment_requests_gateway_account_i~" FOREIGN KEY (gateway_account_id, gateway_payment_request_id) REFERENCES payment.payment_requests (provider_account_id, id) ON DELETE RESTRICT,
        CONSTRAINT "FK_invoice_payment_attempts_provider_accounts_gateway_account_~" FOREIGN KEY (gateway_account_id) REFERENCES payment.provider_accounts (id) ON DELETE RESTRICT,
        CONSTRAINT "FK_invoice_payment_attempts_tenants_tenant_id" FOREIGN KEY (tenant_id) REFERENCES platform.tenants (id) ON DELETE CASCADE,
        CONSTRAINT "FK_invoice_payment_attempts_users_requested_by_user_id" FOREIGN KEY (requested_by_user_id) REFERENCES identity.users (id) ON DELETE RESTRICT
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261109115802_InitialCreate') THEN
    CREATE TABLE tenant.payroll_disbursement_items (
        id uuid NOT NULL DEFAULT (gen_random_uuid()),
        tenant_id uuid NOT NULL,
        payroll_disbursement_id uuid NOT NULL,
        payroll_detail_id uuid NOT NULL,
        staff_user_id uuid NOT NULL,
        gateway_account_id uuid NOT NULL,
        gateway_payout_item_id uuid,
        amount numeric(18,2) NOT NULL,
        destination_bin character varying(12) NOT NULL,
        destination_account_ciphertext bytea NOT NULL,
        destination_account_last4 character varying(4) NOT NULL,
        recipient_name_snapshot character varying(255) NOT NULL,
        status tenant.payroll_disbursement_item_status NOT NULL,
        created_at timestamp with time zone NOT NULL DEFAULT (now()),
        updated_at timestamp with time zone DEFAULT (now()),
        CONSTRAINT "PK_payroll_disbursement_items" PRIMARY KEY (id),
        CONSTRAINT ck_payroll_item_amount CHECK ((amount > 0)),
        CONSTRAINT "FK_payroll_disbursement_items_payout_items_gateway_account_id_~" FOREIGN KEY (gateway_account_id, gateway_payout_item_id) REFERENCES payment.payout_items (provider_account_id, id) ON DELETE RESTRICT,
        CONSTRAINT "FK_payroll_disbursement_items_payroll_details_tenant_id_payrol~" FOREIGN KEY (tenant_id, payroll_detail_id) REFERENCES tenant.payroll_details (tenant_id, id) ON DELETE RESTRICT,
        CONSTRAINT "FK_payroll_disbursement_items_payroll_disbursements_tenant_id_~" FOREIGN KEY (tenant_id, payroll_disbursement_id) REFERENCES tenant.payroll_disbursements (tenant_id, id) ON DELETE RESTRICT,
        CONSTRAINT "FK_payroll_disbursement_items_provider_accounts_gateway_accoun~" FOREIGN KEY (gateway_account_id) REFERENCES payment.provider_accounts (id) ON DELETE RESTRICT,
        CONSTRAINT "FK_payroll_disbursement_items_tenants_tenant_id" FOREIGN KEY (tenant_id) REFERENCES platform.tenants (id) ON DELETE CASCADE,
        CONSTRAINT "FK_payroll_disbursement_items_users_staff_user_id" FOREIGN KEY (staff_user_id) REFERENCES identity.users (id) ON DELETE RESTRICT
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261109115802_InitialCreate') THEN
    CREATE TABLE platform.subscription_invoices (
        id uuid NOT NULL DEFAULT (gen_random_uuid()),
        tenant_id uuid NOT NULL,
        subscription_id uuid NOT NULL,
        invoice_no character varying(80) NOT NULL,
        billing_period_from date,
        billing_period_to date,
        due_at timestamptz NOT NULL,
        status platform.subscription_invoice_status NOT NULL,
        total_amount numeric(18,2) NOT NULL,
        paid_amount numeric(18,2) NOT NULL DEFAULT 0.0,
        currency char(3) NOT NULL DEFAULT 'VND',
        created_at timestamp with time zone NOT NULL DEFAULT (now()),
        updated_at timestamp with time zone DEFAULT (now()),
        CONSTRAINT "PK_subscription_invoices" PRIMARY KEY (id),
        CONSTRAINT ck_subscription_invoices_amount CHECK ((total_amount > 0 AND paid_amount >= 0 AND paid_amount <= total_amount)),
        CONSTRAINT ck_subscription_invoices_billing_period CHECK ((billing_period_from IS NULL AND billing_period_to IS NULL) OR (billing_period_from IS NOT NULL AND billing_period_to IS NOT NULL AND billing_period_to > billing_period_from)),
        CONSTRAINT "FK_subscription_invoices_subscriptions_tenant_id_subscription_~" FOREIGN KEY (tenant_id, subscription_id) REFERENCES platform.subscriptions (tenant_id, id) ON DELETE RESTRICT,
        CONSTRAINT "FK_subscription_invoices_tenants_tenant_id" FOREIGN KEY (tenant_id) REFERENCES platform.tenants (id) ON DELETE RESTRICT
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261109115802_InitialCreate') THEN
    CREATE TABLE tenant.invoice_payment_allocations (
        id uuid NOT NULL DEFAULT (gen_random_uuid()),
        tenant_id uuid NOT NULL,
        payment_attempt_id uuid NOT NULL,
        gateway_account_id uuid NOT NULL,
        gateway_transaction_id uuid NOT NULL,
        amount_applied numeric(18,2) NOT NULL,
        created_at timestamp with time zone NOT NULL DEFAULT (now()),
        CONSTRAINT "PK_invoice_payment_allocations" PRIMARY KEY (id),
        CONSTRAINT ck_invoice_allocation_amount CHECK ((amount_applied > 0)),
        CONSTRAINT "FK_invoice_payment_allocations_invoice_payment_attempts_tenant~" FOREIGN KEY (tenant_id, gateway_account_id, payment_attempt_id) REFERENCES tenant.invoice_payment_attempts (tenant_id, gateway_account_id, id) ON DELETE RESTRICT,
        CONSTRAINT "FK_invoice_payment_allocations_payment_transactions_gateway_ac~" FOREIGN KEY (gateway_account_id, gateway_transaction_id) REFERENCES payment.payment_transactions (provider_account_id, id) ON DELETE RESTRICT,
        CONSTRAINT "FK_invoice_payment_allocations_provider_accounts_gateway_accou~" FOREIGN KEY (gateway_account_id) REFERENCES payment.provider_accounts (id) ON DELETE RESTRICT,
        CONSTRAINT "FK_invoice_payment_allocations_tenants_tenant_id" FOREIGN KEY (tenant_id) REFERENCES platform.tenants (id) ON DELETE RESTRICT
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261109115802_InitialCreate') THEN
    CREATE TABLE platform.subscription_payment_attempts (
        id uuid NOT NULL DEFAULT (gen_random_uuid()),
        subscription_invoice_id uuid NOT NULL,
        gateway_owner_scope payment.provider_owner_scope NOT NULL,
        gateway_account_id uuid NOT NULL,
        gateway_payment_request_id uuid NOT NULL,
        requested_by_user_id uuid NOT NULL,
        status platform.subscription_payment_attempt_status NOT NULL,
        created_at timestamp with time zone NOT NULL DEFAULT (now()),
        CONSTRAINT "PK_subscription_payment_attempts" PRIMARY KEY (id),
        CONSTRAINT "AK_subscription_payment_attempts_gateway_account_id_id" UNIQUE (gateway_account_id, id),
        CONSTRAINT ck_subscription_attempt_scope CHECK ((gateway_owner_scope = 'platform')),
        CONSTRAINT "FK_subscription_payment_attempts_payment_requests_gateway_acco~" FOREIGN KEY (gateway_account_id, gateway_payment_request_id) REFERENCES payment.payment_requests (provider_account_id, id) ON DELETE RESTRICT,
        CONSTRAINT "FK_subscription_payment_attempts_provider_accounts_gateway_own~" FOREIGN KEY (gateway_owner_scope, gateway_account_id) REFERENCES payment.provider_accounts (owner_scope, id) ON DELETE RESTRICT,
        CONSTRAINT "FK_subscription_payment_attempts_subscription_invoices_subscri~" FOREIGN KEY (subscription_invoice_id) REFERENCES platform.subscription_invoices (id) ON DELETE RESTRICT,
        CONSTRAINT "FK_subscription_payment_attempts_users_requested_by_user_id" FOREIGN KEY (requested_by_user_id) REFERENCES identity.users (id) ON DELETE RESTRICT
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261109115802_InitialCreate') THEN
    CREATE TABLE platform.subscription_payment_allocations (
        id uuid NOT NULL DEFAULT (gen_random_uuid()),
        payment_attempt_id uuid NOT NULL,
        gateway_account_id uuid NOT NULL,
        gateway_transaction_id uuid NOT NULL,
        amount_applied numeric(18,2) NOT NULL,
        created_at timestamp with time zone NOT NULL DEFAULT (now()),
        CONSTRAINT "PK_subscription_payment_allocations" PRIMARY KEY (id),
        CONSTRAINT ck_subscription_allocation_amount CHECK ((amount_applied > 0)),
        CONSTRAINT "FK_subscription_payment_allocations_payment_transactions_gatew~" FOREIGN KEY (gateway_account_id, gateway_transaction_id) REFERENCES payment.payment_transactions (provider_account_id, id) ON DELETE RESTRICT,
        CONSTRAINT "FK_subscription_payment_allocations_provider_accounts_gateway_~" FOREIGN KEY (gateway_account_id) REFERENCES payment.provider_accounts (id) ON DELETE RESTRICT,
        CONSTRAINT "FK_subscription_payment_allocations_subscription_payment_attem~" FOREIGN KEY (gateway_account_id, payment_attempt_id) REFERENCES platform.subscription_payment_attempts (gateway_account_id, id) ON DELETE RESTRICT
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261109115802_InitialCreate') THEN
    CREATE INDEX "IX_apartments_apartment_renter" ON tenant.apartments (apartment_renter);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261109115802_InitialCreate') THEN
    CREATE UNIQUE INDEX "IX_apartments_tenant_id_apartment_no" ON tenant.apartments (tenant_id, apartment_no);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261109115802_InitialCreate') THEN
    CREATE INDEX "IX_complaints_created_by_resident" ON tenant.complaints (created_by_resident);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261109115802_InitialCreate') THEN
    CREATE INDEX "IX_complaints_received_by_staff" ON tenant.complaints (received_by_staff);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261109115802_InitialCreate') THEN
    CREATE INDEX "IX_complaints_tenant_id" ON tenant.complaints (tenant_id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261109115802_InitialCreate') THEN
    CREATE INDEX "IX_contracts_imported_by" ON tenant.contracts (imported_by);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261109115802_InitialCreate') THEN
    CREATE INDEX "IX_contracts_tenant_id_apartment_id" ON tenant.contracts (tenant_id, apartment_id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261109115802_InitialCreate') THEN
    CREATE UNIQUE INDEX "IX_contracts_tenant_id_contract_no" ON tenant.contracts (tenant_id, contract_no);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261109115802_InitialCreate') THEN
    CREATE UNIQUE INDEX ix_invitations_active ON tenant.invitations (tenant_id, email, role_id) WHERE status = 'sent' AND is_deleted = false;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261109115802_InitialCreate') THEN
    CREATE INDEX "IX_invitations_created_by" ON tenant.invitations (created_by);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261109115802_InitialCreate') THEN
    CREATE INDEX ix_invitations_expiring ON tenant.invitations (status, expires_at) WHERE status = 'sent';
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261109115802_InitialCreate') THEN
    CREATE INDEX "IX_invitations_role_id" ON tenant.invitations (role_id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261109115802_InitialCreate') THEN
    CREATE INDEX "IX_invitations_target_user_id" ON tenant.invitations (target_user_id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261109115802_InitialCreate') THEN
    CREATE UNIQUE INDEX "IX_invitations_tenant_id_email_token_hash" ON tenant.invitations (tenant_id, email, token_hash);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261109115802_InitialCreate') THEN
    CREATE UNIQUE INDEX "IX_invitations_token_hash" ON tenant.invitations (token_hash);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261109115802_InitialCreate') THEN
    CREATE INDEX "IX_invoice_details_tenant_id_invoice_id" ON tenant.invoice_details (tenant_id, invoice_id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261109115802_InitialCreate') THEN
    CREATE INDEX "IX_invoice_details_tenant_id_service_id" ON tenant.invoice_details (tenant_id, service_id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261109115802_InitialCreate') THEN
    CREATE INDEX ix_invoice_allocation_attempt ON tenant.invoice_payment_allocations (tenant_id, payment_attempt_id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261109115802_InitialCreate') THEN
    CREATE UNIQUE INDEX "IX_invoice_payment_allocations_gateway_account_id_gateway_tran~" ON tenant.invoice_payment_allocations (gateway_account_id, gateway_transaction_id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261109115802_InitialCreate') THEN
    CREATE INDEX "IX_invoice_payment_allocations_tenant_id_gateway_account_id_pa~" ON tenant.invoice_payment_allocations (tenant_id, gateway_account_id, payment_attempt_id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261109115802_InitialCreate') THEN
    CREATE INDEX ix_invoice_payment_attempt_invoice ON tenant.invoice_payment_attempts (tenant_id, invoice_id, created_at DESC);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261109115802_InitialCreate') THEN
    CREATE INDEX ix_invoice_payment_attempt_open ON tenant.invoice_payment_attempts (tenant_id, status, created_at) WHERE status IN ('pending', 'partially_paid');
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261109115802_InitialCreate') THEN
    CREATE INDEX "IX_invoice_payment_attempts_gateway_account_id_gateway_payment~" ON tenant.invoice_payment_attempts (gateway_account_id, gateway_payment_request_id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261109115802_InitialCreate') THEN
    CREATE UNIQUE INDEX "IX_invoice_payment_attempts_gateway_account_id_id" ON tenant.invoice_payment_attempts (gateway_account_id, id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261109115802_InitialCreate') THEN
    CREATE UNIQUE INDEX "IX_invoice_payment_attempts_gateway_payment_request_id" ON tenant.invoice_payment_attempts (gateway_payment_request_id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261109115802_InitialCreate') THEN
    CREATE INDEX "IX_invoice_payment_attempts_requested_by_user_id" ON tenant.invoice_payment_attempts (requested_by_user_id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261109115802_InitialCreate') THEN
    CREATE UNIQUE INDEX "IX_invoice_payment_attempts_tenant_id_id" ON tenant.invoice_payment_attempts (tenant_id, id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261109115802_InitialCreate') THEN
    CREATE INDEX "IX_invoices_created_by_staff" ON tenant.invoices (created_by_staff);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261109115802_InitialCreate') THEN
    CREATE INDEX "IX_invoices_deleted_by_staff" ON tenant.invoices (deleted_by_staff);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261109115802_InitialCreate') THEN
    CREATE INDEX "IX_invoices_tenant_id_apartment_id" ON tenant.invoices (tenant_id, apartment_id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261109115802_InitialCreate') THEN
    CREATE UNIQUE INDEX "IX_invoices_tenant_id_invoice_no" ON tenant.invoices (tenant_id, invoice_no);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261109115802_InitialCreate') THEN
    CREATE INDEX "IX_invoices_updated_by_staff" ON tenant.invoices (updated_by_staff);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261109115802_InitialCreate') THEN
    CREATE INDEX ix_payment_requests_created ON payment.payment_requests (provider_account_id, created_at DESC);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261109115802_InitialCreate') THEN
    CREATE UNIQUE INDEX "IX_payment_requests_provider_account_id_order_code" ON payment.payment_requests (provider_account_id, order_code);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261109115802_InitialCreate') THEN
    CREATE INDEX ix_payment_requests_request_open ON payment.payment_requests (provider_account_id, status, expires_at) WHERE status IN ('pending', 'underpaid');
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261109115802_InitialCreate') THEN
    CREATE UNIQUE INDEX ux_payment_requests__provider_link ON payment.payment_requests (provider_account_id, provider_payment_link_id) WHERE provider_payment_link_id IS NOT NULL;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261109115802_InitialCreate') THEN
    CREATE UNIQUE INDEX "IX_payment_transactions_provider_account_id_id" ON payment.payment_transactions (provider_account_id, id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261109115802_InitialCreate') THEN
    CREATE UNIQUE INDEX "IX_payment_transactions_provider_account_id_provider_transacti~" ON payment.payment_transactions (provider_account_id, provider_transaction_id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261109115802_InitialCreate') THEN
    CREATE INDEX "IX_payment_transactions_provider_account_id_source_webhook_eve~" ON payment.payment_transactions (provider_account_id, source_webhook_event_id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261109115802_InitialCreate') THEN
    CREATE INDEX ix_payment_transactions_request ON payment.payment_transactions (provider_account_id, payment_request_id, occurred_at);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261109115802_InitialCreate') THEN
    CREATE INDEX ix_payment_transactions_unresolved ON payment.payment_transactions (provider_account_id, occurred_at) WHERE payment_request_id IS NULL;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261109115802_InitialCreate') THEN
    CREATE UNIQUE INDEX "IX_payout_batches_provider_account_id_id" ON payment.payout_batches (provider_account_id, id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261109115802_InitialCreate') THEN
    CREATE UNIQUE INDEX "IX_payout_batches_provider_account_id_idempotency_key" ON payment.payout_batches (provider_account_id, idempotency_key);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261109115802_InitialCreate') THEN
    CREATE UNIQUE INDEX "IX_payout_batches_provider_account_id_reference_id" ON payment.payout_batches (provider_account_id, reference_id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261109115802_InitialCreate') THEN
    CREATE INDEX ix_payout_batches_reconciliation ON payment.payout_batches (provider_account_id, status, last_synced_at) WHERE status IN ('submitting', 'processing', 'partially_succeeded');
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261109115802_InitialCreate') THEN
    CREATE UNIQUE INDEX ux_payout_batches_provider_id ON payment.payout_batches (provider_account_id, provider_payout_id) WHERE provider_payout_id IS NOT NULL;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261109115802_InitialCreate') THEN
    CREATE INDEX ix_payout_items_batch_status ON payment.payout_items (provider_account_id, payout_batch_id, status);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261109115802_InitialCreate') THEN
    CREATE UNIQUE INDEX "IX_payout_items_provider_account_id_id" ON payment.payout_items (provider_account_id, id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261109115802_InitialCreate') THEN
    CREATE UNIQUE INDEX "IX_payout_items_provider_account_id_payout_batch_id_reference_~" ON payment.payout_items (provider_account_id, payout_batch_id, reference_id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261109115802_InitialCreate') THEN
    CREATE UNIQUE INDEX ux_payout_items_provider_transaction ON payment.payout_items (provider_account_id, provider_transaction_id) WHERE provider_transaction_id IS NOT NULL;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261109115802_InitialCreate') THEN
    CREATE INDEX "IX_payroll_details_staff_receive" ON tenant.payroll_details (staff_receive);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261109115802_InitialCreate') THEN
    CREATE UNIQUE INDEX "IX_payroll_details_tenant_id_payroll_id_staff_receive" ON tenant.payroll_details (tenant_id, payroll_id, staff_receive);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261109115802_InitialCreate') THEN
    CREATE INDEX "IX_payroll_disbursement_items_staff_user_id" ON tenant.payroll_disbursement_items (staff_user_id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261109115802_InitialCreate') THEN
    CREATE INDEX "IX_payroll_disbursement_items_tenant_id_payroll_detail_id" ON tenant.payroll_disbursement_items (tenant_id, payroll_detail_id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261109115802_InitialCreate') THEN
    CREATE UNIQUE INDEX "IX_payroll_disbursement_items_tenant_id_payroll_disbursement_i~" ON tenant.payroll_disbursement_items (tenant_id, payroll_disbursement_id, payroll_detail_id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261109115802_InitialCreate') THEN
    CREATE INDEX ix_payroll_item_status ON tenant.payroll_disbursement_items (tenant_id, payroll_disbursement_id, status);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261109115802_InitialCreate') THEN
    CREATE UNIQUE INDEX ux_payroll_item_gateway ON tenant.payroll_disbursement_items (gateway_account_id, gateway_payout_item_id) WHERE gateway_payout_item_id IS NOT NULL;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261109115802_InitialCreate') THEN
    CREATE INDEX ix_payroll_disbursement_status ON tenant.payroll_disbursements (tenant_id, status, created_at);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261109115802_InitialCreate') THEN
    CREATE INDEX "IX_payroll_disbursements_approved_by_user_id" ON tenant.payroll_disbursements (approved_by_user_id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261109115802_InitialCreate') THEN
    CREATE INDEX "IX_payroll_disbursements_issued_by_user_id" ON tenant.payroll_disbursements (issued_by_user_id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261109115802_InitialCreate') THEN
    CREATE UNIQUE INDEX "IX_payroll_disbursements_tenant_id_id" ON tenant.payroll_disbursements (tenant_id, id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261109115802_InitialCreate') THEN
    CREATE UNIQUE INDEX "IX_payroll_disbursements_tenant_id_payroll_id_approved_payroll~" ON tenant.payroll_disbursements (tenant_id, payroll_id, approved_payroll_version);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261109115802_InitialCreate') THEN
    CREATE UNIQUE INDEX ux_payroll_disbursement_gateway_batch ON tenant.payroll_disbursements (gateway_account_id, gateway_payout_batch_id) WHERE gateway_payout_batch_id IS NOT NULL;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261109115802_InitialCreate') THEN
    CREATE INDEX "IX_payrolls_created_by" ON tenant.payrolls (created_by);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261109115802_InitialCreate') THEN
    CREATE INDEX ix_provider_accounts_platform_active ON payment.provider_accounts (is_active) WHERE owner_scope = 'platform';
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261109115802_InitialCreate') THEN
    CREATE INDEX ix_provider_accounts_tenant_active ON payment.provider_accounts (tenant_id, is_active) WHERE owner_scope = 'tenant';
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261109115802_InitialCreate') THEN
    CREATE UNIQUE INDEX "IX_provider_accounts_webhook_key" ON payment.provider_accounts (webhook_key);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261109115802_InitialCreate') THEN
    CREATE UNIQUE INDEX ux_provider_accounts_merchant ON payment.provider_accounts (provider, merchant_code) WHERE merchant_code IS NOT NULL;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261109115802_InitialCreate') THEN
    CREATE UNIQUE INDEX ux_provider_accounts_platform_code ON payment.provider_accounts (code) WHERE owner_scope = 'platform';
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261109115802_InitialCreate') THEN
    CREATE UNIQUE INDEX ux_provider_accounts_tenant_code ON payment.provider_accounts (tenant_id, code) WHERE owner_scope = 'tenant';
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261109115802_InitialCreate') THEN
    CREATE INDEX ix_role_permissions_permission ON security.role_permissions (scope, permission_code, role_id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261109115802_InitialCreate') THEN
    CREATE UNIQUE INDEX ux_roles_platform_code ON security.roles (code) WHERE scope = 'platform' AND is_deleted = false;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261109115802_InitialCreate') THEN
    CREATE UNIQUE INDEX ux_roles_tenant_code ON security.roles (tenant_id, code) WHERE scope = 'tenant' AND is_deleted = false;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261109115802_InitialCreate') THEN
    CREATE INDEX "IX_rules_imported_by" ON tenant.rules (imported_by);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261109115802_InitialCreate') THEN
    CREATE INDEX "IX_rules_tenant_id" ON tenant.rules (tenant_id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261109115802_InitialCreate') THEN
    CREATE INDEX "IX_services_created_by_manager" ON tenant.services (created_by_manager);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261109115802_InitialCreate') THEN
    CREATE INDEX "IX_services_deleted_by_manager" ON tenant.services (deleted_by_manager);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261109115802_InitialCreate') THEN
    CREATE UNIQUE INDEX "IX_services_tenant_id_code" ON tenant.services (tenant_id, code);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261109115802_InitialCreate') THEN
    CREATE INDEX "IX_services_updated_by_manager" ON tenant.services (updated_by_manager);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261109115802_InitialCreate') THEN
    CREATE INDEX "IX_staff_time_sheets_created_by_manager" ON tenant.staff_time_sheets (created_by_manager);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261109115802_InitialCreate') THEN
    CREATE INDEX "IX_staff_time_sheets_staff_id" ON tenant.staff_time_sheets (staff_id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261109115802_InitialCreate') THEN
    CREATE UNIQUE INDEX "IX_staff_time_sheets_tenant_id_payroll_period_staff_id" ON tenant.staff_time_sheets (tenant_id, payroll_period, staff_id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261109115802_InitialCreate') THEN
    CREATE INDEX ix_subscription_invoices_due ON platform.subscription_invoices (tenant_id, status, due_at) WHERE status IN ('issued', 'partially_paid', 'overdue');
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261109115802_InitialCreate') THEN
    CREATE UNIQUE INDEX "IX_subscription_invoices_tenant_id_invoice_no" ON platform.subscription_invoices (tenant_id, invoice_no);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261109115802_InitialCreate') THEN
    CREATE INDEX "IX_subscription_invoices_tenant_id_subscription_id" ON platform.subscription_invoices (tenant_id, subscription_id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261109115802_InitialCreate') THEN
    CREATE UNIQUE INDEX "IX_subscription_payment_allocations_gateway_account_id_gateway~" ON platform.subscription_payment_allocations (gateway_account_id, gateway_transaction_id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261109115802_InitialCreate') THEN
    CREATE INDEX "IX_subscription_payment_allocations_gateway_account_id_payment~" ON platform.subscription_payment_allocations (gateway_account_id, payment_attempt_id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261109115802_InitialCreate') THEN
    CREATE INDEX ix_subscription_attempt_invoice ON platform.subscription_payment_attempts (subscription_invoice_id, created_at DESC);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261109115802_InitialCreate') THEN
    CREATE INDEX "IX_subscription_payment_attempts_gateway_account_id_gateway_pa~" ON platform.subscription_payment_attempts (gateway_account_id, gateway_payment_request_id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261109115802_InitialCreate') THEN
    CREATE UNIQUE INDEX "IX_subscription_payment_attempts_gateway_account_id_id" ON platform.subscription_payment_attempts (gateway_account_id, id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261109115802_InitialCreate') THEN
    CREATE INDEX "IX_subscription_payment_attempts_gateway_owner_scope_gateway_a~" ON platform.subscription_payment_attempts (gateway_owner_scope, gateway_account_id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261109115802_InitialCreate') THEN
    CREATE UNIQUE INDEX "IX_subscription_payment_attempts_gateway_payment_request_id" ON platform.subscription_payment_attempts (gateway_payment_request_id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261109115802_InitialCreate') THEN
    CREATE INDEX "IX_subscription_payment_attempts_requested_by_user_id" ON platform.subscription_payment_attempts (requested_by_user_id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261109115802_InitialCreate') THEN
    CREATE INDEX "IX_subscription_plans_created_by_admin" ON platform.subscription_plans (created_by_admin);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261109115802_InitialCreate') THEN
    CREATE INDEX "IX_subscription_plans_deleted_by_admin" ON platform.subscription_plans (deleted_by_admin);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261109115802_InitialCreate') THEN
    CREATE INDEX "IX_subscription_plans_updated_by_admin" ON platform.subscription_plans (updated_by_admin);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261109115802_InitialCreate') THEN
    CREATE INDEX "IX_subscriptions_plan_id" ON platform.subscriptions (plan_id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261109115802_InitialCreate') THEN
    CREATE UNIQUE INDEX ux_subscriptions_tenant_active ON platform.subscriptions (tenant_id) WHERE status IN ('trial', 'active', 'past_due');
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261109115802_InitialCreate') THEN
    CREATE UNIQUE INDEX "IX_tenants_code" ON platform.tenants (code);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261109115802_InitialCreate') THEN
    CREATE UNIQUE INDEX "IX_tenants_slug" ON platform.tenants (slug);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261109115802_InitialCreate') THEN
    CREATE UNIQUE INDEX "IX_tenants_tax_code" ON platform.tenants (tax_code);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261109115802_InitialCreate') THEN
    CREATE INDEX "IX_user_claims_UserId" ON identity.user_claims ("UserId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261109115802_InitialCreate') THEN
    CREATE INDEX "IX_user_logins_UserId" ON identity.user_logins ("UserId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261109115802_InitialCreate') THEN
    CREATE UNIQUE INDEX ix_user_role_active ON security.user_roles (user_id, role_id) WHERE revoked_at IS NULL;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261109115802_InitialCreate') THEN
    CREATE INDEX "IX_user_roles_granted_by_user_id" ON security.user_roles (granted_by_user_id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261109115802_InitialCreate') THEN
    CREATE INDEX "IX_user_roles_scope_role_id" ON security.user_roles (scope, role_id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261109115802_InitialCreate') THEN
    CREATE INDEX ix_user_roles_tenant_user_active ON security.user_roles (tenant_id, user_id) INCLUDE (role_id) WHERE scope = 'tenant' AND revoked_at IS NULL;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261109115802_InitialCreate') THEN
    CREATE UNIQUE INDEX ux_user_roles_platform_user_active ON security.user_roles (user_id) WHERE scope = 'platform' AND revoked_at IS NULL;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261109115802_InitialCreate') THEN
    CREATE INDEX ix_users_tenant ON identity.users (tenant_id) WHERE tenant_id IS NOT NULL;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261109115802_InitialCreate') THEN
    CREATE UNIQUE INDEX "UserNameIndex" ON identity.users (normalized_user_name);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261109115802_InitialCreate') THEN
    CREATE UNIQUE INDEX ux_users_normalized_email ON identity.users (normalized_email) WHERE normalized_email IS NOT NULL;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261109115802_InitialCreate') THEN
    CREATE UNIQUE INDEX ux_users_phone ON identity.users (phone_number) WHERE phone_number IS NOT NULL;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261109115802_InitialCreate') THEN
    CREATE INDEX ix_webhook_events_order_code ON payment.webhook_events (provider_account_id, order_code);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261109115802_InitialCreate') THEN
    CREATE INDEX ix_webhook_events_pending ON payment.webhook_events (next_attempt_at, received_at) WHERE signature_valid = true AND processing_status IN ('received', 'failed') AND processed_at IS NULL;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261109115802_InitialCreate') THEN
    CREATE UNIQUE INDEX "IX_webhook_events_provider_account_id_event_key" ON payment.webhook_events (provider_account_id, event_key) WHERE event_key IS NOT NULL;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261109115802_InitialCreate') THEN
    CREATE UNIQUE INDEX "IX_webhook_events_provider_account_id_id" ON payment.webhook_events (provider_account_id, id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261109115802_InitialCreate') THEN
    CREATE INDEX "IX_webhook_events_provider_account_id_payment_request_id" ON payment.webhook_events (provider_account_id, payment_request_id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261109115802_InitialCreate') THEN
    CREATE INDEX ix_webhook_events_provider_link ON payment.webhook_events (provider_account_id, provider_payment_link_id) WHERE provider_payment_link_id IS NOT NULL;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261109115802_InitialCreate') THEN
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
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261109115802_InitialCreate') THEN
    INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20261109115802_InitialCreate', '8.0.0');
    END IF;
END $EF$;
COMMIT;

START TRANSACTION;


DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261109121017_RestrictFinancialTenantDeletes') THEN
    ALTER TABLE tenant.invoice_payment_attempts DROP CONSTRAINT "FK_invoice_payment_attempts_tenants_tenant_id";
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261109121017_RestrictFinancialTenantDeletes') THEN
    ALTER TABLE tenant.payroll_disbursement_items DROP CONSTRAINT "FK_payroll_disbursement_items_tenants_tenant_id";
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261109121017_RestrictFinancialTenantDeletes') THEN
    ALTER TABLE tenant.payroll_disbursements DROP CONSTRAINT "FK_payroll_disbursements_tenants_tenant_id";
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261109121017_RestrictFinancialTenantDeletes') THEN
    ALTER TABLE tenant.invoice_payment_attempts ADD CONSTRAINT "FK_invoice_payment_attempts_tenants_tenant_id" FOREIGN KEY (tenant_id) REFERENCES platform.tenants (id) ON DELETE RESTRICT;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261109121017_RestrictFinancialTenantDeletes') THEN
    ALTER TABLE tenant.payroll_disbursement_items ADD CONSTRAINT "FK_payroll_disbursement_items_tenants_tenant_id" FOREIGN KEY (tenant_id) REFERENCES platform.tenants (id) ON DELETE RESTRICT;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261109121017_RestrictFinancialTenantDeletes') THEN
    ALTER TABLE tenant.payroll_disbursements ADD CONSTRAINT "FK_payroll_disbursements_tenants_tenant_id" FOREIGN KEY (tenant_id) REFERENCES platform.tenants (id) ON DELETE RESTRICT;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261109121017_RestrictFinancialTenantDeletes') THEN
    INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20261109121017_RestrictFinancialTenantDeletes', '8.0.0');
    END IF;
END $EF$;
COMMIT;

START TRANSACTION;


DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261109121157_RemoveRedundantSubscriptionGatewayScope') THEN
    ALTER TABLE platform.subscription_payment_attempts DROP CONSTRAINT "FK_subscription_payment_attempts_provider_accounts_gateway_own~";
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261109121157_RemoveRedundantSubscriptionGatewayScope') THEN
    DROP INDEX platform."IX_subscription_payment_attempts_gateway_owner_scope_gateway_a~";
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261109121157_RemoveRedundantSubscriptionGatewayScope') THEN
    ALTER TABLE platform.subscription_payment_attempts DROP CONSTRAINT ck_subscription_attempt_scope;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261109121157_RemoveRedundantSubscriptionGatewayScope') THEN
    ALTER TABLE payment.provider_accounts DROP CONSTRAINT "AK_provider_accounts_owner_scope_id";
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261109121157_RemoveRedundantSubscriptionGatewayScope') THEN
    ALTER TABLE platform.subscription_payment_attempts DROP COLUMN gateway_owner_scope;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261109121157_RemoveRedundantSubscriptionGatewayScope') THEN
    ALTER TABLE platform.subscription_payment_attempts ADD CONSTRAINT "FK_subscription_payment_attempts_provider_accounts_gateway_acc~" FOREIGN KEY (gateway_account_id) REFERENCES payment.provider_accounts (id) ON DELETE RESTRICT;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261109121157_RemoveRedundantSubscriptionGatewayScope') THEN
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

    CREATE OR REPLACE FUNCTION payment.assert_platform_provider()
    RETURNS trigger
    LANGUAGE plpgsql
    AS $$
    DECLARE
        account_scope payment.provider_owner_scope;
        account_active boolean;
        requires_active boolean;
    BEGIN
        requires_active := TG_OP = 'INSERT';
        IF TG_OP = 'UPDATE' THEN
            requires_active := NEW.gateway_account_id IS DISTINCT FROM OLD.gateway_account_id;
        END IF;

        SELECT owner_scope, is_active INTO account_scope, account_active
        FROM payment.provider_accounts
        WHERE id = NEW.gateway_account_id;

        IF NOT FOUND
           OR account_scope <> 'platform'
           OR (requires_active AND NOT account_active) THEN
            RAISE EXCEPTION 'Provider account does not belong to Platform'
                USING ERRCODE = '23514';
        END IF;

        RETURN NEW;
    END;
    $$;

    CREATE TRIGGER tr_subscription_attempts_provider
    BEFORE INSERT OR UPDATE ON platform.subscription_payment_attempts
    FOR EACH ROW EXECUTE FUNCTION payment.assert_platform_provider();
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261109121157_RemoveRedundantSubscriptionGatewayScope') THEN
    INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20261109121157_RemoveRedundantSubscriptionGatewayScope', '8.0.0');
    END IF;
END $EF$;
COMMIT;

START TRANSACTION;


DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261109121732_ArchiveLegacyMigrationHistory') THEN
    CREATE TABLE IF NOT EXISTS public."__EFMigrationsHistoryLegacy_20261109" (
        "MigrationId" character varying(150) NOT NULL,
        "ProductVersion" character varying(32) NOT NULL,
        CONSTRAINT "PK___EFMigrationsHistoryLegacy_20261109"
            PRIMARY KEY ("MigrationId")
    );

    INSERT INTO public."__EFMigrationsHistoryLegacy_20261109" ("MigrationId", "ProductVersion")
    SELECT "MigrationId", "ProductVersion"
    FROM public."__EFMigrationsHistory"
    WHERE "MigrationId" NOT IN (
        '20261109115802_InitialCreate',
        '20261109121017_RestrictFinancialTenantDeletes',
        '20261109121157_RemoveRedundantSubscriptionGatewayScope')
    ON CONFLICT ("MigrationId") DO NOTHING;

    DELETE FROM public."__EFMigrationsHistory"
    WHERE "MigrationId" NOT IN (
        '20261109115802_InitialCreate',
        '20261109121017_RestrictFinancialTenantDeletes',
        '20261109121157_RemoveRedundantSubscriptionGatewayScope');
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261109121732_ArchiveLegacyMigrationHistory') THEN
    INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20261109121732_ArchiveLegacyMigrationHistory', '8.0.0');
    END IF;
END $EF$;
COMMIT;

