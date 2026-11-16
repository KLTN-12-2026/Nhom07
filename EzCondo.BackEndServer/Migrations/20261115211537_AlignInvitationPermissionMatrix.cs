using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EzCondo.BackEndServer.Migrations
{
    /// <inheritdoc />
    public partial class AlignInvitationPermissionMatrix : Migration
    {
		/// <inheritdoc />
		protected override void Up(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.Sql(
			    """
                INSERT INTO security.permissions(
                    scope,
                    code,
                    description)
                VALUES
                    (
                        'tenant'::security.role_scope_enum,
                        'tenant.managers.invite',
                        'Mời quản lý tham gia tenant'
                    ),
                    (
                        'tenant'::security.role_scope_enum,
                        'tenant.managers.revoke_invitation',
                        'Thu hồi lời mời quản lý'
                    ),
                    (
                        'tenant'::security.role_scope_enum,
                        'tenant.residents.revoke_invitation',
                        'Thu hồi lời mời cư dân'
                    )
                ON CONFLICT (scope, code)
                DO UPDATE
                SET description = EXCLUDED.description;
                """);

			migrationBuilder.Sql(
			    """
                DELETE FROM security.role_permissions AS rp
                USING security.roles AS r
                WHERE rp.scope = 'tenant'::security.role_scope_enum
                    AND r.scope = rp.scope
                    AND r.id = rp.role_id
                    AND
                    (
                        (
                            r.code = 'TENANT_ADMIN'
                            AND rp.permission_code IN
                            (
                                'tenant.staff.invite',
                                'tenant.staff.revoke_invitation'
                            )
                        )
                        OR
                        (
                            r.code = 'MANAGER'
                            AND rp.permission_code IN
                            (
                                'tenant.residents.invite',
                                'tenant.residents.revoke_invitation'
                            )
                        )
                    );
                """);

			migrationBuilder.Sql(
				"""
                INSERT INTO security.role_permissions(
                    scope,
                    role_id,
                    permission_code)
                SELECT
                    'tenant'::security.role_scope_enum,
                    r.id,
                    p.permission_code
                FROM security.roles AS r
                CROSS JOIN
                (
                    VALUES
                        ('tenant.managers.invite'),
                        ('tenant.managers.revoke_invitation')
                ) AS p(permission_code)
                WHERE r.scope = 'tenant'::security.role_scope_enum
                  AND r.code = 'TENANT_ADMIN'
                  AND r.is_deleted = false
                ON CONFLICT DO NOTHING;
                """);

			migrationBuilder.Sql(
			"""
                INSERT INTO security.role_permissions(
                    scope,
                    role_id,
                    permission_code)
                SELECT
                    'tenant'::security.role_scope_enum,
                    r.id,
                    p.permission_code
                FROM security.roles AS r
                CROSS JOIN
                (
                    VALUES
                        ('tenant.staff.invite'),
                        ('tenant.staff.revoke_invitation')
                ) AS p(permission_code)
                WHERE r.scope = 'tenant'::security.role_scope_enum
                  AND r.code = 'MANAGER'
                  AND r.is_deleted = false
                ON CONFLICT DO NOTHING;
                """);

			migrationBuilder.Sql(
				"""
                INSERT INTO security.role_permissions(
                    scope,
                    role_id,
                    permission_code)
                SELECT
                    'tenant'::security.role_scope_enum,
                    r.id,
                    p.permission_code
                FROM security.roles AS r
                CROSS JOIN
                (
                    VALUES
                        ('tenant.residents.invite'),
                        ('tenant.residents.revoke_invitation')
                ) AS p(permission_code)
                WHERE r.scope = 'tenant'::security.role_scope_enum
                  AND r.code = 'STAFF'
                  AND r.is_deleted = false
                ON CONFLICT DO NOTHING;
                """);
		}

		/// <inheritdoc />
		protected override void Down(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.Sql(
				"""
        DELETE FROM security.role_permissions AS rp
        USING security.roles AS r
        WHERE rp.scope = 'tenant'::security.role_scope_enum
          AND r.scope = rp.scope
          AND r.id = rp.role_id
          AND
          (
              (
                  r.code = 'TENANT_ADMIN'
                  AND rp.permission_code IN
                  (
                      'tenant.managers.invite',
                      'tenant.managers.revoke_invitation'
                  )
              )
              OR
              (
                  r.code = 'STAFF'
                  AND rp.permission_code =
                      'tenant.residents.revoke_invitation'
              )
          );
        """);

			migrationBuilder.Sql(
				"""
        INSERT INTO security.role_permissions(
            scope,
            role_id,
            permission_code)
        SELECT
            'tenant'::security.role_scope_enum,
            r.id,
            p.permission_code
        FROM security.roles AS r
        CROSS JOIN
        (
            VALUES
                ('tenant.staff.invite'),
                ('tenant.staff.revoke_invitation')
        ) AS p(permission_code)
        WHERE r.scope = 'tenant'::security.role_scope_enum
          AND r.code = 'TENANT_ADMIN'
          AND r.is_deleted = false
        ON CONFLICT DO NOTHING;
        """);

			migrationBuilder.Sql(
				"""
        INSERT INTO security.role_permissions(
            scope,
            role_id,
            permission_code)
        SELECT
            'tenant'::security.role_scope_enum,
            r.id,
            'tenant.residents.invite'
        FROM security.roles AS r
        WHERE r.scope = 'tenant'::security.role_scope_enum
          AND r.code = 'MANAGER'
          AND r.is_deleted = false
        ON CONFLICT DO NOTHING;
        """);

			migrationBuilder.Sql(
				"""
        DELETE FROM security.permissions AS p
        WHERE p.scope = 'tenant'::security.role_scope_enum
          AND p.code IN
          (
              'tenant.managers.invite',
              'tenant.managers.revoke_invitation',
              'tenant.residents.revoke_invitation'
          )
          AND NOT EXISTS
          (
              SELECT 1
              FROM security.role_permissions AS rp
              WHERE rp.scope = p.scope
                AND rp.permission_code = p.code
          );
        """);
		}
	}
}
