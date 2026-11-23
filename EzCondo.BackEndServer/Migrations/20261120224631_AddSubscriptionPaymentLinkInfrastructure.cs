using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EzCondo.BackEndServer.Migrations
{
    /// <inheritdoc />
    public partial class AddSubscriptionPaymentLinkInfrastructure : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateSequence(
                name: "payment_order_code_seq",
                schema: "payment");

            migrationBuilder.CreateIndex(
                name: "ux_subscription_payment_attempts_open_invoice",
                schema: "platform",
                table: "subscription_payment_attempts",
                column: "subscription_invoice_id",
                unique: true,
                filter: "status IN ('pending', 'partially_paid')");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ux_subscription_payment_attempts_open_invoice",
                schema: "platform",
                table: "subscription_payment_attempts");

            migrationBuilder.DropSequence(
                name: "payment_order_code_seq",
                schema: "payment");
        }
    }
}
