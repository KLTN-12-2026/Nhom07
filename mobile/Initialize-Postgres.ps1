$ErrorActionPreference = 'Stop'
$local = Join-Path $PSScriptRoot '.local'
$pgRoot = Join-Path $local 'postgresql'
$bin = Join-Path $pgRoot 'pgsql/bin'
$data = Join-Path $pgRoot 'data'
$settingsPath = Join-Path $local 'shared-backend.json'
if (!(Test-Path (Join-Path $bin 'initdb.exe'))) {
    throw 'Extract the official PostgreSQL Windows binaries into mobile/.local/postgresql/pgsql first. See mobile/README.md.'
}
if (!(Test-Path $settingsPath)) {
    if (Test-Path $data) { throw 'Existing PostgreSQL data has no matching local settings. Restore shared-backend.json; do not reinitialize.' }
    $bytes = New-Object byte[] 32
    $random = [System.Security.Cryptography.RandomNumberGenerator]::Create()
    $random.GetBytes($bytes)
    $password = [Convert]::ToBase64String($bytes)
    $random.GetBytes($bytes)
    $key = [Convert]::ToBase64String($bytes)
    $random.Dispose()
    $pwFile = Join-Path $pgRoot 'init-password.tmp'
    [IO.File]::WriteAllText($pwFile, $password)
    try {
        & (Join-Path $bin 'initdb.exe') -D $data -U postgres --auth=scram-sha-256 --encoding=UTF8 --locale=C --pwfile=$pwFile
        if ($LASTEXITCODE -ne 0) { throw 'PostgreSQL initialization failed.' }
    } finally { Remove-Item -LiteralPath $pwFile -ErrorAction SilentlyContinue }
    Add-Content (Join-Path $data 'postgresql.conf') "`nlisten_addresses = '127.0.0.1'`nport = 55432" -Encoding ascii
    @{
        ConnectionStrings = @{ DefaultConnection = "Host=127.0.0.1;Port=55432;Database=ezcondo_mobile_local;Username=postgres;Password=$password" }
        Jwt = @{ Issuer='EzCondo.Local'; Audience='EzCondo.Local'; SigningKey=$key; AccessTokenMinutes=30 }
        PlatformAccounts = @{ Accounts=@() }
    } | ConvertTo-Json -Depth 5 | Set-Content -LiteralPath $settingsPath -Encoding utf8
}
& (Join-Path $bin 'pg_ctl.exe') -D $data status *> $null
if ($LASTEXITCODE -ne 0) {
    & (Join-Path $bin 'pg_ctl.exe') -D $data -l (Join-Path $pgRoot 'server.log') -w start
    if ($LASTEXITCODE -ne 0) { throw 'PostgreSQL startup failed.' }
}
$settings = Get-Content $settingsPath -Raw | ConvertFrom-Json
$connection = $settings.ConnectionStrings.DefaultConnection
$password = ($connection -split ';' | Where-Object { $_ -like 'Password=*' }).Substring(9)
$previousPassword = $env:PGPASSWORD
try {
    $env:PGPASSWORD = $password
    $exists = & (Join-Path $bin 'psql.exe') -h 127.0.0.1 -p 55432 -U postgres -d postgres -tAc "SELECT 1 FROM pg_database WHERE datname='ezcondo_mobile_local'"
    if ($LASTEXITCODE -ne 0) { throw 'PostgreSQL connection failed.' }
    if ($exists -ne '1') {
        & (Join-Path $bin 'createdb.exe') -h 127.0.0.1 -p 55432 -U postgres ezcondo_mobile_local
        if ($LASTEXITCODE -ne 0) { throw 'Local database creation failed.' }
    }
} finally { $env:PGPASSWORD = $previousPassword }
Write-Host 'Isolated PostgreSQL ready on 127.0.0.1:55432. Existing web configuration and databases were not modified.'
