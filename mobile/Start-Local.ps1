param([string]$DeviceId, [switch]$ApiOnly)
$ErrorActionPreference = 'Stop'
$taskRoot = Split-Path -Parent $PSScriptRoot
$apiProject = Join-Path $PSScriptRoot '../EzCondo.BackEndServer'
$appProject = Join-Path $PSScriptRoot 'EzCondo.Mobile'
$localPath = Join-Path $PSScriptRoot '.local'
$adb = Join-Path ${env:ProgramFiles(x86)} 'Android\android-sdk\platform-tools\adb.exe'
$environmentNames = @('ASPNETCORE_ENVIRONMENT', 'ASPNETCORE_URLS', 'EZCONDO_MOBILE_LOCAL')
$previousEnvironment = @{}
foreach ($name in $environmentNames) { $previousEnvironment[$name] = [Environment]::GetEnvironmentVariable($name, 'Process') }
try {
    $env:ASPNETCORE_ENVIRONMENT = 'Development'
    $env:ASPNETCORE_URLS = 'http://127.0.0.1:5275'
    $env:EZCONDO_MOBILE_LOCAL = '1'
    & (Join-Path $PSScriptRoot 'Initialize-Postgres.ps1')
    $healthy = $false
    try {
        $health = Invoke-RestMethod 'http://127.0.0.1:5275/api/mobile/server' -TimeoutSec 3
        $healthy = $health.backend -eq 'EzCondo.BackEndServer'
    } catch { }
    if (!$healthy) {
        dotnet build $apiProject --nologo -v minimal
        if ($LASTEXITCODE -ne 0) { throw 'API build failed.' }
        $apiDll = Join-Path $apiProject 'bin\Debug\net8.0\EzCondo.BackEndServer.dll'
        $apiProcess = Start-Process dotnet -ArgumentList ('"' + $apiDll + '"') -WorkingDirectory $apiProject -WindowStyle Hidden -PassThru -RedirectStandardOutput (Join-Path $localPath 'api.log') -RedirectStandardError (Join-Path $localPath 'api-errors.log')
        $apiProcess.Id | Set-Content (Join-Path $localPath 'api.pid')
        for ($attempt = 0; $attempt -lt 30; $attempt++) {
            Start-Sleep -Seconds 1
            if ($apiProcess.HasExited) { throw "API stopped. Read $localPath\api-errors.log" }
            try { $health = Invoke-RestMethod 'http://127.0.0.1:5275/api/mobile/server' -TimeoutSec 3; $healthy = $health.backend -eq 'EzCondo.BackEndServer' } catch { }
            if ($healthy) { break }
        }
        if (!$healthy) { throw 'API did not become ready on port 5275.' }
    }
    Write-Host 'API ready at http://127.0.0.1:5275'
    $seedProject = Join-Path $PSScriptRoot 'tools/SeedLocal'
    # The shared backend may already be running under F5 and its apphost is locked.
    dotnet build $seedProject --no-dependencies --nologo -v minimal
    if ($LASTEXITCODE -ne 0) { throw 'Local seed tool build failed.' }
    dotnet run --no-build --project $seedProject -- $localPath
    if ($LASTEXITCODE -ne 0) { throw 'Local seed failed.' }
    if ($ApiOnly) { return }
    if (!(Test-Path $adb)) { throw 'Android SDK platform-tools/adb.exe is missing.' }
    $devices = @(& $adb devices | Select-String '^([^\s]+)\s+device$' | ForEach-Object { $_.Matches[0].Groups[1].Value })
    if (!$DeviceId) {
        if ($devices.Count -eq 0) {
            $emulator = Join-Path ${env:ProgramFiles(x86)} 'Android\android-sdk\emulator\emulator.exe'
            if (!(Test-Path $emulator)) { throw 'Android Emulator is missing. Install it using Android Device Manager.' }
            $runningEmulators = @(& $adb devices | Select-String '^(emulator-\d+)\s+' | ForEach-Object { $_.Matches[0].Groups[1].Value })
            if ($runningEmulators.Count -eq 0) {
                $avds = @(& $emulator -list-avds | Where-Object { ![string]::IsNullOrWhiteSpace($_) })
                if ($LASTEXITCODE -ne 0 -or $avds.Count -eq 0) { throw 'Create an Android virtual device in Android Device Manager first.' }
                Write-Host "Starting Android emulator $($avds[0])..."
                Start-Process -FilePath $emulator -ArgumentList @('-avd', ('"' + $avds[0] + '"')) -WindowStyle Normal -RedirectStandardOutput (Join-Path $localPath 'emulator.log') -RedirectStandardError (Join-Path $localPath 'emulator-errors.log')
            }
            Write-Host 'Waiting for Android to finish booting...'
            for ($attempt = 0; $attempt -lt 120; $attempt++) {
                $devices = @(& $adb devices | Select-String '^(emulator-\d+)\s+device$' | ForEach-Object { $_.Matches[0].Groups[1].Value })
                if ($devices.Count -gt 0 -and ((& $adb -s $devices[0] shell getprop sys.boot_completed 2>$null) -join '').Trim() -eq '1') { break }
                $devices = @()
                Start-Sleep -Seconds 2
            }
            if ($devices.Count -eq 0) { throw "Android did not finish booting. Read $localPath\emulator-errors.log" }
        }
        if ($devices.Count -ne 1) { throw "Multiple Android devices are connected ($($devices -join ', ')). Specify -DeviceId." }
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
} finally {
    foreach ($name in $environmentNames) { [Environment]::SetEnvironmentVariable($name, $previousEnvironment[$name], 'Process') }
}
