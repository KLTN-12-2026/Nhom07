using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EzCondo.BackEndServer.Migrations
{
    /// <inheritdoc />
    public partial class ArchiveLegacyMigrationHistory : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
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
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                INSERT INTO public."__EFMigrationsHistory" ("MigrationId", "ProductVersion")
                SELECT "MigrationId", "ProductVersion"
                FROM public."__EFMigrationsHistoryLegacy_20261109"
                ON CONFLICT ("MigrationId") DO NOTHING;

                DROP TABLE IF EXISTS public."__EFMigrationsHistoryLegacy_20261109";
                """);
        }
    }
}
