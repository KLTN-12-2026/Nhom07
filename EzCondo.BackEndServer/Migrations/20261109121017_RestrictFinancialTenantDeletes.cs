using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EzCondo.BackEndServer.Migrations
{
    /// <inheritdoc />
    public partial class RestrictFinancialTenantDeletes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_invoice_payment_attempts_tenants_tenant_id",
                schema: "tenant",
                table: "invoice_payment_attempts");

            migrationBuilder.DropForeignKey(
                name: "FK_payroll_disbursement_items_tenants_tenant_id",
                schema: "tenant",
                table: "payroll_disbursement_items");

            migrationBuilder.DropForeignKey(
                name: "FK_payroll_disbursements_tenants_tenant_id",
                schema: "tenant",
                table: "payroll_disbursements");

            migrationBuilder.AddForeignKey(
                name: "FK_invoice_payment_attempts_tenants_tenant_id",
                schema: "tenant",
                table: "invoice_payment_attempts",
                column: "tenant_id",
                principalSchema: "platform",
                principalTable: "tenants",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_payroll_disbursement_items_tenants_tenant_id",
                schema: "tenant",
                table: "payroll_disbursement_items",
                column: "tenant_id",
                principalSchema: "platform",
                principalTable: "tenants",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_payroll_disbursements_tenants_tenant_id",
                schema: "tenant",
                table: "payroll_disbursements",
                column: "tenant_id",
                principalSchema: "platform",
                principalTable: "tenants",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_invoice_payment_attempts_tenants_tenant_id",
                schema: "tenant",
                table: "invoice_payment_attempts");

            migrationBuilder.DropForeignKey(
                name: "FK_payroll_disbursement_items_tenants_tenant_id",
                schema: "tenant",
                table: "payroll_disbursement_items");

            migrationBuilder.DropForeignKey(
                name: "FK_payroll_disbursements_tenants_tenant_id",
                schema: "tenant",
                table: "payroll_disbursements");

            migrationBuilder.AddForeignKey(
                name: "FK_invoice_payment_attempts_tenants_tenant_id",
                schema: "tenant",
                table: "invoice_payment_attempts",
                column: "tenant_id",
                principalSchema: "platform",
                principalTable: "tenants",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_payroll_disbursement_items_tenants_tenant_id",
                schema: "tenant",
                table: "payroll_disbursement_items",
                column: "tenant_id",
                principalSchema: "platform",
                principalTable: "tenants",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_payroll_disbursements_tenants_tenant_id",
                schema: "tenant",
                table: "payroll_disbursements",
                column: "tenant_id",
                principalSchema: "platform",
                principalTable: "tenants",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
