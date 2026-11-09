using EzCondo.BackEndServer.Data.Enums;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EzCondo.BackEndServer.Migrations
{
    /// <inheritdoc />
    public partial class RemoveRedundantSubscriptionGatewayScope : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_subscription_payment_attempts_provider_accounts_gateway_own~",
                schema: "platform",
                table: "subscription_payment_attempts");

            migrationBuilder.DropIndex(
                name: "IX_subscription_payment_attempts_gateway_owner_scope_gateway_a~",
                schema: "platform",
                table: "subscription_payment_attempts");

            migrationBuilder.DropCheckConstraint(
                name: "ck_subscription_attempt_scope",
                schema: "platform",
                table: "subscription_payment_attempts");

            migrationBuilder.DropUniqueConstraint(
                name: "AK_provider_accounts_owner_scope_id",
                schema: "payment",
                table: "provider_accounts");

            migrationBuilder.DropColumn(
                name: "gateway_owner_scope",
                schema: "platform",
                table: "subscription_payment_attempts");

            migrationBuilder.AddForeignKey(
                name: "FK_subscription_payment_attempts_provider_accounts_gateway_acc~",
                schema: "platform",
                table: "subscription_payment_attempts",
                column: "gateway_account_id",
                principalSchema: "payment",
                principalTable: "provider_accounts",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.Sql("""
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
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                DROP FUNCTION IF EXISTS payment.assert_platform_provider() CASCADE;
                """);

            migrationBuilder.DropForeignKey(
                name: "FK_subscription_payment_attempts_provider_accounts_gateway_acc~",
                schema: "platform",
                table: "subscription_payment_attempts");

            migrationBuilder.AddColumn<ProviderOwnerScope>(
                name: "gateway_owner_scope",
                schema: "platform",
                table: "subscription_payment_attempts",
                type: "payment.provider_owner_scope",
                nullable: false,
                defaultValue: ProviderOwnerScope.Platform);

            migrationBuilder.AddUniqueConstraint(
                name: "AK_provider_accounts_owner_scope_id",
                schema: "payment",
                table: "provider_accounts",
                columns: new[] { "owner_scope", "id" });

            migrationBuilder.CreateIndex(
                name: "IX_subscription_payment_attempts_gateway_owner_scope_gateway_a~",
                schema: "platform",
                table: "subscription_payment_attempts",
                columns: new[] { "gateway_owner_scope", "gateway_account_id" });

            migrationBuilder.AddCheckConstraint(
                name: "ck_subscription_attempt_scope",
                schema: "platform",
                table: "subscription_payment_attempts",
                sql: "(gateway_owner_scope = 'platform')");

            migrationBuilder.AddForeignKey(
                name: "FK_subscription_payment_attempts_provider_accounts_gateway_own~",
                schema: "platform",
                table: "subscription_payment_attempts",
                columns: new[] { "gateway_owner_scope", "gateway_account_id" },
                principalSchema: "payment",
                principalTable: "provider_accounts",
                principalColumns: new[] { "owner_scope", "id" },
                onDelete: ReferentialAction.Restrict);
        }
    }
}
