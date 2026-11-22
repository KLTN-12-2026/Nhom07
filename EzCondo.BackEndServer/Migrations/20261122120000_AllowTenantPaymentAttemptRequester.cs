using EzCondo.BackEndServer.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EzCondo.BackEndServer.Migrations
{
	[Migration("20261122120000_AllowTenantPaymentAttemptRequester")]
	[DbContext(typeof(ApplicationDbContext))]
	public partial class AllowTenantPaymentAttemptRequester : Migration
	{
		protected override void Up(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.Sql(
				"""
				DROP TRIGGER IF EXISTS
					tr_subscription_attempts_platform_users
				ON platform.subscription_payment_attempts;

				CREATE OR REPLACE FUNCTION
					security.assert_subscription_attempt_requester()
				RETURNS trigger
				LANGUAGE plpgsql
				AS $$
				BEGIN
					IF NOT EXISTS
					(
						SELECT 1
						FROM identity.users u
						INNER JOIN platform.subscription_invoices i
							ON i.id = NEW.subscription_invoice_id
						WHERE u.id = NEW.requested_by_user_id
						  AND u.is_deleted = false
						  AND
						  (
							u.tenant_id IS NULL
							OR u.tenant_id = i.tenant_id
						  )
					)
					THEN
						RAISE EXCEPTION
							'Payment requester must be a Platform user or belong to the invoice tenant'
							USING ERRCODE = '23514';
					END IF;

					RETURN NEW;
				END;
				$$;

				CREATE TRIGGER
					tr_subscription_attempts_requester_scope
				BEFORE INSERT OR UPDATE
				ON platform.subscription_payment_attempts
				FOR EACH ROW
				EXECUTE FUNCTION
					security.assert_subscription_attempt_requester();
				""");
		}

		protected override void Down(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.Sql(
				"""
				DROP TRIGGER IF EXISTS
					tr_subscription_attempts_requester_scope
				ON platform.subscription_payment_attempts;

				DROP FUNCTION IF EXISTS
					security.assert_subscription_attempt_requester();

				CREATE TRIGGER
					tr_subscription_attempts_platform_users
				BEFORE INSERT OR UPDATE
				ON platform.subscription_payment_attempts
				FOR EACH ROW
				EXECUTE FUNCTION
					security.assert_platform_users(
						'requested_by_user_id');
				""");
		}
	}
}