# EzCondo Android — shared backend

The MAUI app now calls **EzCondo.BackEndServer** (ASP.NET Core 8 / EF Core / PostgreSQL). It no longer starts or calls EzCondo.Mobile.Api by default. The existing SQLite database and legacy API sources are retained, not migrated or deleted.

## Run on this computer

From the repository root:

```powershell
.\mobile\Start-Local.ps1
```

For only the backend:

```powershell
.\mobile\Start-Local.ps1 -ApiOnly
```

The script starts the isolated PostgreSQL cluster, builds/starts the shared backend, creates test data only if the database has no users, and optionally starts Android and installs the APK. It does not reset existing users or passwords.

The script restores its environment variables before returning, including on failure. The existing web backend profiles keep port 5158; the Mobile Local profile uses port 5275 so the two launch configurations do not compete for that port. The local seeder builds without rebuilding a backend executable that may already be locked by F5.

- API: `http://127.0.0.1:5275/api/mobile/server`
- Android emulator: `http://10.0.2.2:5275/`
- Physical Android over USB: `http://127.0.0.1:5275/`, with `adb reverse tcp:5275 tcp:5275`.
- PostgreSQL: `127.0.0.1:55432`, database `ezcondo_mobile_local`.
- Test emails: `resident@ezcondo.local`, `staff@ezcondo.local`.
- Generated test password: `mobile/.local/test-accounts.json`. Do not commit or share this file.

Login uses email/password checked through the backend's ASP.NET Identity user store. The old account picker and ID-only demo impersonation are not used by the shared backend. Tokens stay in app memory and expire; sign in again when prompted.

## Visual Studio 2022

1. Open `mobile/EzCondo.Mobile.slnx`.
2. Select the shared **New Profile** startup profile.
3. Confirm EzCondo.BackEndServer uses **Mobile Local** and EzCondo.Mobile uses the installed Pixel 7 Android emulator.
4. Press F5. Shift+F5 stops the debug session. PostgreSQL remains running for the next launch.

Only the explicit **Mobile Local** backend launch profile loads `mobile/.local/shared-backend.json` and starts the private PostgreSQL cluster automatically after a computer restart. The existing web http/https/IIS profiles keep their existing settings. Do not run a separate API on port 5275 while using multiple-startup F5.

`Initialize-Postgres.ps1` can be rerun without recreating the database. To stop just the local PostgreSQL cluster:

```powershell
& .\mobile\.local\postgresql\pgsql\bin\pg_ctl.exe -D .\mobile\.local\postgresql\data -m fast -w stop
```

## What is supported

| Mobile feature | Existing backend data |
|---|---|
| Resident/staff login and profile | identity.users, tenant user roles, platform.tenants |
| Resident invoices and detail | tenant.invoices, invoice_details, apartments, services |
| Resident contracts | tenant.contracts and apartments |
| Resident complaint creation/tracking | tenant.complaints |
| Staff assigned complaints and resolution | received_by_staff and resolved_at |
| Service catalogue and prices | tenant.services |
| Staff monthly work records | tenant.staff_time_sheets |

The schema has no parcels, facility bookings, service-request registrations, complaint chat, complaint priority/category/deadline, vehicles, or shift rosters. Their navigation/actions are hidden rather than inventing records or changing the web schema. Staff can mark assigned complaints resolved; no unsupported intermediate states are written. A service catalogue is not a service-request history. Work records are not shift assignments.

Resident queries use the live tenant and apartment ownership; staff queries are restricted to their own assignments/work records. Deleted/locked users, revoked roles and suspended/closed tenants are rechecked on each request. Draft invoices are not shown to residents. Existing controller routes, CORS configuration, entity mappings and migrations are unchanged.

## Database isolation and setup

The user explicitly requested a new local PostgreSQL test database. It uses the checked-in EzCondo.BackEndServer migrations, including its tenant integrity triggers. It is not a copy of a live website database, and does not import the old mobile SQLite data.

PostgreSQL Windows binaries were downloaded from the official EDB binaries page: https://www.enterprisedb.com/download-postgresql-binaries . Runtime files are under `mobile/.local/postgresql/pgsql`. A new computer must extract the official Windows archive there, then run `Initialize-Postgres.ps1` and `Start-Local.ps1 -ApiOnly` before F5.

Connection credentials, random local JWT key, database files and test account credentials are kept under ignored `mobile/.local`. Never point the local seeder at a website/production database. It rejects connections other than the isolated local database. The Mobile Local profile skips the website's platform-account seeder because placeholder platform account configuration is incomplete; other profiles retain their original seeding behavior.

To later use the actual website database, configure its connection and JWT settings through the existing backend configuration mechanism and use the corresponding backend URL in mobile. Do not use the Mobile Local profile for that connection. Review and back up that database before running the backend's pre-existing automatic migrations.

## Android demo acceptance criteria

Run the demo on the Android emulator with the shared backend and the isolated local PostgreSQL database. Before each demo, confirm that the staff account has at least one assigned, unresolved sample complaint. The local launcher preserves existing data, so a complaint resolved in a previous demo stays resolved.

- **Resident** signs in, views their profile, invoices, contracts, and service catalogue with prices, then submits a complaint and reopens it to check its status.
- **Staff** signs in, views their profile and monthly work records, then opens the separately prepared sample complaint assigned to them and marks it resolved.
- **API authorization** passes the shared-backend checks below. Residents can read records within their authorized scope, and requests for another apartment's or tenant's private records are denied. Staff can resolve complaints assigned to them, and requests to resolve complaints outside their assignments are denied.

## Checks

```powershell
dotnet run --project mobile/tests/EzCondo.Mobile.ConnectionTests --no-launch-profile
dotnet run --project mobile/tests/EzCondo.SharedBackend.Tests --no-launch-profile
dotnet build mobile/EzCondo.Mobile -f net9.0-android
dotnet build EzCondo.Portal
```

The shared-backend test harness runs real HTTP controllers with an isolated in-memory store by default. For PostgreSQL, set `EZCONDO_TEST_POSTGRES` to a connection for **127.0.0.1:55432/ezcondo_mobile_regression**. That mode recreates ONLY that disposable regression database and applies the checked-in migrations. It rejects other hosts, ports and database names. Never supply the website database connection.

Startup readiness retries only transport failures for up to 30 seconds, plus an in-flight HTTP timeout of up to 12 seconds. HTTP errors are reported immediately; POST/PATCH actions are not replayed automatically.

## Verified on 23 November 2026

- Eleven client checks passed: transport/Android/timeout recovery, bounded offline retries, HTTP errors, no POST replay, invalid JSON, invalid login and rejecting the wrong backend.
- Nineteen HTTP/security checks plus fixture checks passed with both the in-memory provider and real PostgreSQL in `ezcondo_mobile_regression`. These cover resident/tenant isolation, draft invoices, decimal amounts, assignment checks, disabled users, password lockout, login throttling and keeping mobile tokens out of web routes. Existing weather and subscription-plan APIs remained reachable.
- Android APK and Portal builds succeeded with zero warnings/errors during verification. Existing web controller, model and migration files were unchanged. The original http/https/IIS launch profile values and default profile order were checked against Git and preserved.
- Reproduced an API startup hang when PostgreSQL was initially stopped: the daemon inherited redirected pipes, so waiting for EOF never completed. A regression check failed with a timeout before the fix and passed afterward. The backend now waits for pg_ctl itself rather than daemon pipe closure.
- Two successful F5 cold starts after the fix began with both API and PostgreSQL stopped; the final run used port 5275 and left web port 5158 free. The Android resident signed in and loaded invoices on that final run.
- Android resident flows loaded home, invoices, contracts and service prices, then created a complaint and loaded it back. Staff signed in, loaded monthly work records and resolved an assigned complaint. The launcher reused the F5 API without rebuilding its locked executable and restored the caller's environment.

Screenshots are under ignored `mobile/.local/shared-backend-*.png`, including `shared-backend-final-f5.png`. During the initial heavy build the emulator displayed a System UI nonresponse dialog; it continued after waiting. No Windows restart or complete manual website UI regression was performed. No live website database was supplied or modified; these results use the explicitly requested new local PostgreSQL database.
