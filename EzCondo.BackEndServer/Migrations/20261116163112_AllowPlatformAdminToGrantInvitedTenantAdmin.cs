using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EzCondo.BackEndServer.Migrations
{
    /// <inheritdoc />
    public partial class AllowPlatformAdminToGrantInvitedTenantAdmin : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                CREATE OR REPLACE FUNCTION security.validate_user_role_assignment()
                RETURNS trigger
                LANGUAGE plpgsql
                AS $$
                DECLARE
                    role_tenant uuid;
                    role_code text;
                    user_tenant uuid;
                    grantor_tenant uuid;
                    platform_grant_allowed boolean := false;
                    has_tenant_admin boolean;
                    has_other_active boolean;
                BEGIN
                    PERFORM pg_advisory_xact_lock(
                        hashtextextended(NEW.user_id::text, 0));

                    SELECT tenant_id, code
                    INTO role_tenant, role_code
                    FROM security.roles
                    WHERE id = NEW.role_id
                      AND scope = NEW.scope
                      AND is_deleted = false;

                    IF NOT FOUND THEN
                        RAISE EXCEPTION
                            'Role % is missing, deleted, or belongs to another scope',
                            NEW.role_id
                            USING ERRCODE = '23514';
                    END IF;

                    SELECT tenant_id
                    INTO user_tenant
                    FROM identity.users
                    WHERE id = NEW.user_id
                      AND is_deleted = false;

                    IF NOT FOUND THEN
                        RAISE EXCEPTION
                            'User % is missing or deleted', NEW.user_id
                            USING ERRCODE = '23514';
                    END IF;

                    IF NEW.scope = 'platform' THEN
                        IF NEW.tenant_id IS NOT NULL
                           OR role_tenant IS NOT NULL
                           OR user_tenant IS NOT NULL
                           OR role_code <> 'PLATFORM_ADMIN' THEN
                            RAISE EXCEPTION
                                'Invalid Platform role assignment'
                                USING ERRCODE = '23514';
                        END IF;
                    ELSE
                        IF NEW.tenant_id IS NULL
                           OR role_tenant IS DISTINCT FROM NEW.tenant_id
                           OR user_tenant IS DISTINCT FROM NEW.tenant_id THEN
                            RAISE EXCEPTION
                                'User, role, and assignment must belong to the same tenant'
                                USING ERRCODE = '23514';
                        END IF;
                    END IF;

                    IF NEW.granted_by_user_id IS NOT NULL THEN
                        SELECT tenant_id
                        INTO grantor_tenant
                        FROM identity.users
                        WHERE id = NEW.granted_by_user_id
                          AND is_deleted = false;

                        IF NOT FOUND THEN
                            RAISE EXCEPTION
                                'Grantor does not exist or is deleted'
                                USING ERRCODE = '23514';
                        END IF;

                        IF NEW.scope = 'platform' THEN
                            IF grantor_tenant IS NOT NULL THEN
                                RAISE EXCEPTION
                                    'Grantor does not belong to the assignment scope'
                                    USING ERRCODE = '23514';
                            END IF;
                        ELSIF grantor_tenant IS DISTINCT FROM NEW.tenant_id THEN
                            IF grantor_tenant IS NULL
                               AND role_code = 'TENANT_ADMIN' THEN
                                IF TG_OP = 'INSERT' THEN
                                    SELECT
                                        EXISTS (
                                            SELECT 1
                                            FROM security.user_roles AS ur
                                            JOIN security.roles AS grantor_role
                                              ON grantor_role.id = ur.role_id
                                             AND grantor_role.scope = ur.scope
                                            WHERE ur.user_id = NEW.granted_by_user_id
                                              AND ur.scope = 'platform'
                                              AND ur.tenant_id IS NULL
                                              AND ur.revoked_at IS NULL
                                              AND grantor_role.scope = 'platform'
                                              AND grantor_role.tenant_id IS NULL
                                              AND grantor_role.code = 'PLATFORM_ADMIN'
                                              AND grantor_role.is_deleted = false
                                        )
                                        AND EXISTS (
                                            SELECT 1
                                            FROM tenant.invitations AS invitation
                                            WHERE invitation.tenant_id = NEW.tenant_id
                                              AND invitation.role_id = NEW.role_id
                                              AND invitation.target_user_id = NEW.user_id
                                              AND invitation.created_by = NEW.granted_by_user_id
                                              AND invitation.status = 'sent'
                                              AND invitation.accepted_at IS NULL
                                              AND invitation.revoked_at IS NULL
                                              AND invitation.expires_at > CURRENT_TIMESTAMP
                                              AND invitation.is_deleted = false
                                        )
                                    INTO platform_grant_allowed;
                                ELSIF TG_OP = 'UPDATE' THEN
                                    platform_grant_allowed :=
                                        NEW.granted_by_user_id = OLD.granted_by_user_id
                                        AND NEW.user_id = OLD.user_id
                                        AND NEW.role_id = OLD.role_id
                                        AND NEW.scope = OLD.scope
                                        AND NEW.tenant_id IS NOT DISTINCT FROM OLD.tenant_id;
                                END IF;
                            END IF;

                            IF NOT platform_grant_allowed THEN
                                RAISE EXCEPTION
                                    'Platform grantor is not authorized by an active TENANT_ADMIN invitation'
                                    USING ERRCODE = '23514';
                            END IF;
                        END IF;
                    END IF;

                    IF NEW.scope = 'tenant' AND NEW.revoked_at IS NULL THEN
                        SELECT EXISTS (
                            SELECT 1
                            FROM security.user_roles AS ur
                            JOIN security.roles AS r
                              ON r.id = ur.role_id
                             AND r.scope = ur.scope
                            WHERE ur.user_id = NEW.user_id
                              AND ur.tenant_id = NEW.tenant_id
                              AND ur.revoked_at IS NULL
                              AND ur.id <> NEW.id
                              AND r.code = 'TENANT_ADMIN'
                        )
                        INTO has_tenant_admin;

                        IF role_code = 'TENANT_ADMIN' OR has_tenant_admin THEN
                            IF role_code NOT IN (
                                    'TENANT_ADMIN',
                                    'MANAGER',
                                    'STAFF')
                               OR EXISTS (
                                    SELECT 1
                                    FROM security.user_roles AS ur
                                    JOIN security.roles AS r
                                      ON r.id = ur.role_id
                                     AND r.scope = ur.scope
                                    WHERE ur.user_id = NEW.user_id
                                      AND ur.tenant_id = NEW.tenant_id
                                      AND ur.revoked_at IS NULL
                                      AND ur.id <> NEW.id
                                      AND r.code NOT IN (
                                          'TENANT_ADMIN',
                                          'MANAGER',
                                          'STAFF')
                               ) THEN
                                RAISE EXCEPTION
                                    'TENANT_ADMIN may only combine TENANT_ADMIN, MANAGER, and STAFF'
                                    USING ERRCODE = '23514';
                            END IF;
                        ELSE
                            SELECT EXISTS (
                                SELECT 1
                                FROM security.user_roles AS ur
                                WHERE ur.user_id = NEW.user_id
                                  AND ur.tenant_id = NEW.tenant_id
                                  AND ur.revoked_at IS NULL
                                  AND ur.id <> NEW.id
                            )
                            INTO has_other_active;

                            IF has_other_active THEN
                                RAISE EXCEPTION
                                    'Only TENANT_ADMIN may have multiple active tenant roles'
                                    USING ERRCODE = '23514';
                            END IF;
                        END IF;
                    END IF;

                    RETURN NEW;
                END;
                $$;
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
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
                    PERFORM pg_advisory_xact_lock(
                        hashtextextended(NEW.user_id::text, 0));

                    SELECT tenant_id, code
                    INTO role_tenant, role_code
                    FROM security.roles
                    WHERE id = NEW.role_id
                      AND scope = NEW.scope
                      AND is_deleted = false;

                    IF NOT FOUND THEN
                        RAISE EXCEPTION
                            'Role % is missing, deleted, or belongs to another scope',
                            NEW.role_id
                            USING ERRCODE = '23514';
                    END IF;

                    SELECT tenant_id
                    INTO user_tenant
                    FROM identity.users
                    WHERE id = NEW.user_id
                      AND is_deleted = false;

                    IF NOT FOUND THEN
                        RAISE EXCEPTION
                            'User % is missing or deleted', NEW.user_id
                            USING ERRCODE = '23514';
                    END IF;

                    IF NEW.scope = 'platform' THEN
                        IF NEW.tenant_id IS NOT NULL
                           OR role_tenant IS NOT NULL
                           OR user_tenant IS NOT NULL
                           OR role_code <> 'PLATFORM_ADMIN' THEN
                            RAISE EXCEPTION
                                'Invalid Platform role assignment'
                                USING ERRCODE = '23514';
                        END IF;
                    ELSE
                        IF NEW.tenant_id IS NULL
                           OR role_tenant IS DISTINCT FROM NEW.tenant_id
                           OR user_tenant IS DISTINCT FROM NEW.tenant_id THEN
                            RAISE EXCEPTION
                                'User, role, and assignment must belong to the same tenant'
                                USING ERRCODE = '23514';
                        END IF;
                    END IF;

                    IF NEW.granted_by_user_id IS NOT NULL THEN
                        SELECT tenant_id
                        INTO grantor_tenant
                        FROM identity.users
                        WHERE id = NEW.granted_by_user_id
                          AND is_deleted = false;

                        IF NOT FOUND
                           OR (NEW.scope = 'platform' AND grantor_tenant IS NOT NULL)
                           OR (NEW.scope = 'tenant'
                               AND grantor_tenant IS DISTINCT FROM NEW.tenant_id) THEN
                            RAISE EXCEPTION
                                'Grantor does not belong to the assignment scope'
                                USING ERRCODE = '23514';
                        END IF;
                    END IF;

                    IF NEW.scope = 'tenant' AND NEW.revoked_at IS NULL THEN
                        SELECT EXISTS (
                            SELECT 1
                            FROM security.user_roles AS ur
                            JOIN security.roles AS r
                              ON r.id = ur.role_id
                             AND r.scope = ur.scope
                            WHERE ur.user_id = NEW.user_id
                              AND ur.tenant_id = NEW.tenant_id
                              AND ur.revoked_at IS NULL
                              AND ur.id <> NEW.id
                              AND r.code = 'TENANT_ADMIN'
                        )
                        INTO has_tenant_admin;

                        IF role_code = 'TENANT_ADMIN' OR has_tenant_admin THEN
                            IF role_code NOT IN (
                                    'TENANT_ADMIN',
                                    'MANAGER',
                                    'STAFF')
                               OR EXISTS (
                                    SELECT 1
                                    FROM security.user_roles AS ur
                                    JOIN security.roles AS r
                                      ON r.id = ur.role_id
                                     AND r.scope = ur.scope
                                    WHERE ur.user_id = NEW.user_id
                                      AND ur.tenant_id = NEW.tenant_id
                                      AND ur.revoked_at IS NULL
                                      AND ur.id <> NEW.id
                                      AND r.code NOT IN (
                                          'TENANT_ADMIN',
                                          'MANAGER',
                                          'STAFF')
                               ) THEN
                                RAISE EXCEPTION
                                    'TENANT_ADMIN may only combine TENANT_ADMIN, MANAGER, and STAFF'
                                    USING ERRCODE = '23514';
                            END IF;
                        ELSE
                            SELECT EXISTS (
                                SELECT 1
                                FROM security.user_roles AS ur
                                WHERE ur.user_id = NEW.user_id
                                  AND ur.tenant_id = NEW.tenant_id
                                  AND ur.revoked_at IS NULL
                                  AND ur.id <> NEW.id
                            )
                            INTO has_other_active;

                            IF has_other_active THEN
                                RAISE EXCEPTION
                                    'Only TENANT_ADMIN may have multiple active tenant roles'
                                    USING ERRCODE = '23514';
                            END IF;
                        END IF;
                    END IF;

                    RETURN NEW;
                END;
                $$;
                """);
        }
    }
}
