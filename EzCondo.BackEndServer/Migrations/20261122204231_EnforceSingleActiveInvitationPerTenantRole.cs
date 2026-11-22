using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EzCondo.BackEndServer.Migrations
{
	/// <inheritdoc />
	public partial class EnforceSingleActiveInvitationPerTenantRole : Migration
	{
		/// <inheritdoc />
		protected override void Up(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.DropIndex(
				name: "ix_invitations_active",
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

		/// <inheritdoc />
		protected override void Down(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.DropIndex(
				name: "ux_invitations_active_tenant_role",
				schema: "tenant",
				table: "invitations");

			migrationBuilder.CreateIndex(
				name: "ix_invitations_active",
				schema: "tenant",
				table: "invitations",
				columns: new[] { "tenant_id", "email", "role_id" },
				unique: true,
				filter: "status IN ('pending_delivery', 'sent') AND is_deleted = false");
		}
	}
}
