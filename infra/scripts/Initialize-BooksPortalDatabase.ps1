param(
  [string]$SqlServer = 'localhost,1433',
  [string]$DatabaseName = 'BooksPortal',

  [switch]$UseWindowsAuth,
  [string]$AdminUser,
  [string]$AdminPassword,

  [ValidateSet('SqlLogin', 'WindowsLogin')]
  [string]$AppAuthMode = 'SqlLogin',

  [string]$AppLoginName = 'booksportal_app',
  [string]$AppLoginPassword = '',
  [string]$AppWindowsPrincipal = '',

  [switch]$GrantDbOwner
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

if (-not (Get-Command sqlcmd -ErrorAction SilentlyContinue)) {
  throw 'sqlcmd is required. Install SQL Server command line tools first.'
}

if (-not $UseWindowsAuth -and ([string]::IsNullOrWhiteSpace($AdminUser) -or [string]::IsNullOrWhiteSpace($AdminPassword))) {
  throw 'Provide -AdminUser and -AdminPassword when not using -UseWindowsAuth.'
}

if ($AppAuthMode -eq 'SqlLogin' -and [string]::IsNullOrWhiteSpace($AppLoginPassword)) {
  throw 'Provide -AppLoginPassword when -AppAuthMode SqlLogin.'
}

if ($AppAuthMode -eq 'WindowsLogin' -and [string]::IsNullOrWhiteSpace($AppWindowsPrincipal)) {
  throw 'Provide -AppWindowsPrincipal when -AppAuthMode WindowsLogin.'
}

$escapedDb = $DatabaseName.Replace(']', ']]')
$appPrincipal = if ($AppAuthMode -eq 'SqlLogin') { $AppLoginName } else { $AppWindowsPrincipal }
$escapedAppPrincipal = $appPrincipal.Replace(']', ']]')
$escapedLogin = $AppLoginName.Replace(']', ']]')
$escapedPassword = $AppLoginPassword.Replace("'", "''")

$sql = @"
IF DB_ID(N'$DatabaseName') IS NULL
BEGIN
  CREATE DATABASE [$escapedDb];
END
GO

USE [$escapedDb];
GO

IF '$AppAuthMode' = 'SqlLogin'
BEGIN
  IF NOT EXISTS (SELECT 1 FROM sys.server_principals WHERE name = N'$AppLoginName')
  BEGIN
    EXEC(N'CREATE LOGIN [$escapedLogin] WITH PASSWORD = ''$escapedPassword'', CHECK_POLICY = ON, CHECK_EXPIRATION = OFF');
  END
END
ELSE
BEGIN
  IF NOT EXISTS (SELECT 1 FROM sys.server_principals WHERE name = N'$AppWindowsPrincipal')
  BEGIN
    EXEC(N'CREATE LOGIN [$escapedAppPrincipal] FROM WINDOWS');
  END
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.database_principals WHERE name = N'$appPrincipal')
BEGIN
  EXEC(N'CREATE USER [$escapedAppPrincipal] FOR LOGIN [$escapedAppPrincipal]');
END
GO

IF IS_ROLEMEMBER('db_datareader', '$appPrincipal') <> 1
BEGIN
  EXEC sp_addrolemember N'db_datareader', N'$appPrincipal';
END
GO

IF IS_ROLEMEMBER('db_datawriter', '$appPrincipal') <> 1
BEGIN
  EXEC sp_addrolemember N'db_datawriter', N'$appPrincipal';
END
GO
"@

if ($GrantDbOwner) {
  $sql += "`r`n" + @"
IF IS_ROLEMEMBER('db_owner', '$appPrincipal') <> 1
BEGIN
  EXEC sp_addrolemember N'db_owner', N'$appPrincipal';
END
GO
"@
}

$tempFile = Join-Path $env:TEMP "booksportal-db-init-$([guid]::NewGuid().ToString('n')).sql"
$sql | Set-Content -Path $tempFile -Encoding UTF8

try {
  $baseArgs = @('-S', $SqlServer, '-b', '-i', $tempFile)
  if ($UseWindowsAuth) {
    & sqlcmd @baseArgs -E
  }
  else {
    & sqlcmd @baseArgs -U $AdminUser -P $AdminPassword
  }

  if ($LASTEXITCODE -ne 0) {
    throw "sqlcmd failed with exit code $LASTEXITCODE"
  }

  Write-Host "Database bootstrap complete for [$DatabaseName] on [$SqlServer]" -ForegroundColor Green
  Write-Host "App principal: $appPrincipal (mode: $AppAuthMode)" -ForegroundColor Green
}
finally {
  Remove-Item -Path $tempFile -Force -ErrorAction SilentlyContinue
}
