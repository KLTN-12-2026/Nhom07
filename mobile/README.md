# EzCondo Android local

## Scope and design

Native .NET MAUI Android app for residents and staff. ASP.NET Core mobile API reuses `EZCondoDbContext` and entity classes from `EzCondo.Web/backend`. SQLite data starts as a consistent backup of `ezcondo.db`. The website API and database schema are retained.

The local build targets .NET 9 / MAUI 9.0.120 to match the installed SDK and existing web backend. Open `mobile/EzCondo.Mobile.slnx` in the installed Visual Studio 2022 (17.14) or open this repository in VS Code. Upgrade to a supported .NET/MAUI release before a production deployment.

Build order: session and tenant isolation → resident read screens → complaints and messages → bookings and services → staff work and shifts → Android verification.

Resident capabilities: home, invoices with line items, contracts, parcels, complaints with conversation, facility bookings, service requests, profile and vehicles. Staff capabilities: assigned complaints, validated status changes, conversation, personal shifts and profile. There is no payroll/timekeeping data in this SQLite database. PayOS and Gemini require credentials and their own verified integration before those actions can be enabled.

UI contract: Vietnamese content, teal primary color, light neutral surfaces, native Android controls, 48dp touch targets, four persistent tabs, scrollable details, visible loading/error/empty states, retry and form validation. A single main action per form. Status must have text as well as color. Data and role come from the API.

## Local authentication boundary

The source `UserAccounts` table has no password or Identity linkage. This build explicitly offers **local demo sessions**, using protected bearer tokens with an eight-hour expiry and a fresh database membership check on every request. Demo account selection is available only in Development. The host refuses to start outside Development and binds to loopback. Use `adb reverse tcp:5275 tcp:5275` for emulator or USB device. This host is a local development app, and must be replaced with production Identity login before hosting externally.

Legacy business tables lack TenantId and some assignments use display names. Access is restricted through unambiguous apartment codes within the signed-in tenant. Ambiguous apartment codes are denied. Staff assignments accept only an exact name or the existing `KTV ` prefix and require a unique staff name in the database. Resident private records additionally match the current account name. These constraints should become explicit foreign keys during a future database migration.

## Commands

From the repository root in PowerShell, run `./mobile/Start-Local.ps1`. It backs up the source SQLite database on first run into ignored `mobile/.local/mobile.db`, starts the API on `127.0.0.1:5275`, builds the APK, and installs/launches it on the selected Android device. An Android emulator or USB device must be available (`adb devices`). Re-running retains local edits. API logs are in `mobile/.local/`.

```powershell
dotnet build mobile/EzCondo.Mobile.Api
dotnet build mobile/EzCondo.Mobile -f net9.0-android
node mobile/tests/api-smoke.mjs
```

The app defaults to `http://127.0.0.1:5275/` over adb reverse. The login screen allows an API URL for other local configurations. Cleartext HTTP is allowed only for localhost and the emulator host, for this local setup. See [Microsoft's local web service guidance](https://learn.microsoft.com/en-us/dotnet/maui/data-cloud/local-web-services).

`node mobile/tests/api-smoke.mjs` owns port 5275 while it runs. Stop the local API first using the process ID in `mobile/.local/api.pid`. The test starts and stops its own API against a separate disposable SQLite backup. Never run it against a production host.

## Acceptance and verification

- Build API and signed Android debug APK with no errors.
- Authenticate each supported role and reject unsigned, tampered and expired tokens.
- Verify access isolation, private records, input validation and staff transitions using API smoke checks against a disposable copy.
- Create and reload a complaint/message, booking and service request. Changes survive restart.
- Install and launch on Android, inspect resident and staff screens, submit a form, check navigation and offline retry, inspect crash logs.
- Preserve the source database during tests.

Use existing C# naming and async EF methods. Shared wire contracts live in `EzCondo.Mobile.Contracts`. Native pages live in `EzCondo.Mobile/Pages`, HTTP session handling in `Services`. API endpoints live in `EzCondo.Mobile.Api`. No external runtime UI libraries are required.

## Verification results

Verified on 13 November 2026 with .NET SDK 9.0.320, MAUI 9.0.120 and the installed Pixel 7 Android API 35 x86_64 emulator.

- API and Android debug APK builds succeeded with zero warnings and zero errors.
- API smoke checks passed 46 HTTP assertions plus ownership, roles, tenant isolation, ambiguous apartment rejection, data persistence and SQLite integrity assertions.
- Android resident login, invoices and invoice lines, complaint creation, message submission, facility booking and service request submission worked against the API.
- Android staff login, assigned complaint detail, status update, shift schedule and profile worked against the API.
- Stopped and restarted the API, then verified the Android-created complaint/message and staff status persisted. The existing app session could reconnect.
- UI inspection found and fixed the post-creation complaint navigation. Network exception handling also covers the legacy WebException transport errors in the .NET 9 Android handler. The [Android handler source](https://github.com/dotnet/android/blob/main/src/Mono.Android/Xamarin.Android.Net/AndroidMessageHandler.cs) describes the legacy exception behavior and its later mapping to HttpRequestException.
- Reinstalled the final APK, verified complaint creation opens its detail, launched with the API stopped and verified the Vietnamese network error, then restarted the API and successfully retried and logged in. The full `Start-Local.ps1 -DeviceId emulator-5554` entry point also built, installed and launched successfully.
- Source `EzCondo.Web/backend/ezcondo.db` has no content change in Git. Device tests use `mobile/.local/mobile.db`; the isolated API test uses its own backup. Keys, database copies, logs and screenshots are ignored by Git.

The debug APK is `mobile/EzCondo.Mobile/bin/Debug/net9.0-android/vn.ezcondo.mobile-Signed.apk`. Screenshots and device inspection artifacts are in `mobile/.local/`. `node mobile/tests/android-ui.cjs snapshot` inspects the current Android screen and `shot <name>` saves a screenshot. It targets `emulator-5554` by default or the device in `ANDROID_SERIAL`.

Physical devices, PayOS/Gemini, production Identity, eight-hour expiry by waiting in real time, and production deployment have not been verified. There is no background sync or push notification service in this local build.
