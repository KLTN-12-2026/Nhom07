using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EzCondo.BackEndServer.Migrations
{
    /// <inheritdoc />
    public partial class AllowPlatformAdminToInviteTenantAdmin : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                CREATE OR REPLACE FUNCTION tenant.validate_invitation_scope()
                RETURNS trigger
                LANGUAGE plpgsql
                AS $$
                DECLARE
                    role_tenant uuid;
                    role_scope security.role_scope_enum;
                    role_code varchar(80);
                    creator_tenant uuid;
                    target_tenant uuid;
                BEGIN
                    SELECT tenant_id, scope, code
                    INTO role_tenant, role_scope, role_code
                    FROM security.roles
                    WHERE id = NEW.role_id
                      AND is_deleted = false;

                    IF NOT FOUND
                       OR role_scope <> 'tenant'
                       OR role_tenant IS DISTINCT FROM NEW.tenant_id THEN
                        RAISE EXCEPTION
                            'Invitation role does not belong to the invitation tenant'
                            USING ERRCODE = '23514';
                    END IF;

                    SELECT tenant_id
                    INTO creator_tenant
                    FROM identity.users
                    WHERE id = NEW.created_by
                      AND is_deleted = false;

                    IF NOT FOUND THEN
                        RAISE EXCEPTION
                            'Invitation creator does not exist or is deleted'
                            USING ERRCODE = '23514';
                    END IF;

                    IF role_code = 'TENANT_ADMIN' THEN
                        IF creator_tenant IS NOT NULL
                           OR NOT EXISTS (
                                SELECT 1
                                FROM security.user_roles AS ur
                                JOIN security.roles AS creator_role
                                  ON creator_role.id = ur.role_id
                                 AND creator_role.scope = ur.scope
                                WHERE ur.user_id = NEW.created_by
                                  AND ur.scope = 'platform'
                                  AND ur.tenant_id IS NULL
                                  AND ur.revoked_at IS NULL
                                  AND creator_role.scope = 'platform'
                                  AND creator_role.tenant_id IS NULL
                                  AND creator_role.code = 'PLATFORM_ADMIN'
                                  AND creator_role.is_deleted = false
                           ) THEN
                            RAISE EXCEPTION
                                'Only an active PLATFORM_ADMIN may create a TENANT_ADMIN invitation'
                                USING ERRCODE = '23514';
                        END IF;
                    ELSIF creator_tenant IS DISTINCT FROM NEW.tenant_id THEN
                        RAISE EXCEPTION
                            'Invitation creator does not belong to the invitation tenant'
                            USING ERRCODE = '23514';
                    END IF;

                    IF NEW.target_user_id IS NOT NULL THEN
                        SELECT tenant_id
                        INTO target_tenant
                        FROM identity.users
                        WHERE id = NEW.target_user_id;

                        IF NOT FOUND
                           OR target_tenant IS DISTINCT FROM NEW.tenant_id THEN
                            RAISE EXCEPTION
                                'Invitation target does not belong to the invitation tenant'
                                USING ERRCODE = '23514';
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
                    SELECT tenant_id, scope
                    INTO role_tenant, role_scope
                    FROM security.roles
                    WHERE id = NEW.role_id
                      AND is_deleted = false;

                    IF NOT FOUND
                       OR role_scope <> 'tenant'
                       OR role_tenant IS DISTINCT FROM NEW.tenant_id THEN
                        RAISE EXCEPTION
                            'Invitation role does not belong to the invitation tenant'
                            USING ERRCODE = '23514';
                    END IF;

                    SELECT tenant_id
                    INTO creator_tenant
                    FROM identity.users
                    WHERE id = NEW.created_by;

                    IF NOT FOUND
                       OR creator_tenant IS DISTINCT FROM NEW.tenant_id THEN
                        RAISE EXCEPTION
                            'Invitation creator does not belong to the invitation tenant'
                            USING ERRCODE = '23514';
                    END IF;

                    IF NEW.target_user_id IS NOT NULL THEN
                        SELECT tenant_id
                        INTO target_tenant
                        FROM identity.users
                        WHERE id = NEW.target_user_id;

                        IF NOT FOUND
                           OR target_tenant IS DISTINCT FROM NEW.tenant_id THEN
                            RAISE EXCEPTION
                                'Invitation target does not belong to the invitation tenant'
                                USING ERRCODE = '23514';
                        END IF;
                    END IF;

                    RETURN NEW;
                END;
                $$;
                """);
        }
    }
}
