param([string]$DeviceId, [switch]$ApiOnly)
$ErrorActionPreference = 'Stop'
$taskRoot = Split-Path -Parent $PSScriptRoot
$apiProject = Join-Path $PSScriptRoot 'EzCondo.Mobile.Api'
$appProject = Join-Path $PSScriptRoot 'EzCondo.Mobile'
$localPath = Join-Path $PSScriptRoot '.local'
$adb = Join-Path ${env:ProgramFiles(x86)} 'Android\android-sdk\platform-tools\adb.exe'
$env:ASPNETCORE_ENVIRONMENT = 'Development'
$env:MobileDatabase = Join-Path $localPath 'mobile.db'
node (Join-Path $PSScriptRoot 'prepare-db.cjs')
if ($LASTEXITCODE -ne 0) { throw 'Database backup failed.' }
dotnet build $apiProject --nologo -v minimal
if ($LASTEXITCODE -ne 0) { throw 'API build failed.' }
$healthy = $false
try {
    $health = Invoke-RestMethod 'http://127.0.0.1:5275/health'
    $healthy = $health.mode -eq 'local-demo'
} catch { }
if (!$healthy) {
    $apiDll = Join-Path $apiProject 'bin\Debug\net9.0\EzCondo.Mobile.Api.dll'
    $apiProcess = Start-Process dotnet -ArgumentList ('"' + $apiDll + '"') -WorkingDirectory $apiProject -WindowStyle Hidden -PassThru -RedirectStandardOutput (Join-Path $localPath 'api.log') -RedirectStandardError (Join-Path $localPath 'api-errors.log')
    $apiProcess.Id | Set-Content (Join-Path $localPath 'api.pid')
    for ($attempt = 0; $attempt -lt 30; $attempt++) {
        Start-Sleep -Seconds 1
        if ($apiProcess.HasExited) { throw "API stopped. Read $localPath\api-errors.log" }
        try { $health = Invoke-RestMethod 'http://127.0.0.1:5275/health'; $healthy = $health.mode -eq 'local-demo' } catch { }
        if ($healthy) { break }
    }
    if (!$healthy) { throw 'API did not become ready on port 5275.' }
}
Write-Host 'API ready at http://127.0.0.1:5275'
if ($ApiOnly) { return }
if (!(Test-Path $adb)) { throw 'Android SDK platform-tools/adb.exe is missing.' }
$devices = @(& $adb devices | Select-String '^([^\s]+)\s+device$' | ForEach-Object { $_.Matches[0].Groups[1].Value })
if (!$DeviceId) {
    if ($devices.Count -ne 1) { throw 'Start one Android emulator or specify -DeviceId from adb devices.' }
    $DeviceId = $devices[0]
}
if ($DeviceId -notin $devices) { throw 'The selected Android device is unavailable.' }
& $adb -s $DeviceId reverse tcp:5275 tcp:5275
if ($LASTEXITCODE -ne 0) { throw 'adb reverse failed.' }
dotnet build $appProject -f net9.0-android --nologo -v minimal
if ($LASTEXITCODE -ne 0) { throw 'Android build failed.' }
$apk = Join-Path $appProject 'bin\Debug\net9.0-android\vn.ezcondo.mobile-Signed.apk'
& $adb -s $DeviceId install -r $apk
if ($LASTEXITCODE -ne 0) { throw 'APK install failed.' }
& $adb -s $DeviceId shell monkey -p vn.ezcondo.mobile -c android.intent.category.LAUNCHER 1
if ($LASTEXITCODE -ne 0) { throw 'Android launch failed.' }
Write-Host "EzCondo launched on $DeviceId"
