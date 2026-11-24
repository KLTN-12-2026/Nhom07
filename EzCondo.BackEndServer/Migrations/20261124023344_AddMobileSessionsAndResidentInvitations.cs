using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EzCondo.BackEndServer.Migrations
{
    /// <inheritdoc />
    public partial class AddMobileSessionsAndResidentInvitations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ux_invitations_active_tenant_role",
                schema: "tenant",
                table: "invitations");

            migrationBuilder.AddColumn<bool>(
                name: "is_mobile_resident_invitation",
                schema: "tenant",
                table: "invitations",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "mobile_sessions",
                schema: "security",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    expires_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_mobile_sessions", x => x.id);
                    table.ForeignKey(
                        name: "FK_mobile_sessions_users_user_id",
                        column: x => x.user_id,
                        principalSchema: "identity",
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ux_invitations_active_mobile_resident",
                schema: "tenant",
                table: "invitations",
                columns: new[] { "tenant_id", "email", "role_id" },
                unique: true,
                filter: "status IN ('pending_delivery', 'sent') AND is_deleted = false AND is_mobile_resident_invitation = true");

            migrationBuilder.CreateIndex(
                name: "ux_invitations_active_tenant_role",
                schema: "tenant",
                table: "invitations",
                columns: new[] { "tenant_id", "role_id" },
                unique: true,
                filter: "status IN ('pending_delivery', 'sent') AND is_deleted = false AND is_mobile_resident_invitation = false");

            migrationBuilder.CreateIndex(
                name: "IX_mobile_sessions_expires_at",
                schema: "security",
                table: "mobile_sessions",
                column: "expires_at");

            migrationBuilder.CreateIndex(
                name: "IX_mobile_sessions_user_id",
                schema: "security",
                table: "mobile_sessions",
                column: "user_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "mobile_sessions",
                schema: "security");

            migrationBuilder.DropIndex(
                name: "ux_invitations_active_mobile_resident",
                schema: "tenant",
                table: "invitations");

            migrationBuilder.DropIndex(
                name: "ux_invitations_active_tenant_role",
                schema: "tenant",
                table: "invitations");

            migrationBuilder.DropColumn(
                name: "is_mobile_resident_invitation",
                schema: "tenant",
                table: "invitations");

            migrationBuilder.CreateIndex(
                name: "ux_invitations_active_tenant_role",
                schema: "tenant",
                table: "invitations",
                columns: new[] { "tenant_id", "role_id" },
                unique: true,
                filter: "status IN ('pending_delivery', 'sent') AND is_deleted = false");
        }
    }
}
