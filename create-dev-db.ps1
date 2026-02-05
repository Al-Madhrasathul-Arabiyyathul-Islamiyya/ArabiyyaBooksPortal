# ============================================
# Books Portal - Create Dev Database (PowerShell 7)
# ============================================
# Connects to local SQL Server with Windows Auth
# Drops existing database and creates fresh one

param(
    [string]$ServerInstance = ".",           # Use "." for local, or ".\SQLEXPRESS", "(localdb)\MSSQLLocalDB"
    [string]$DatabaseName = "BooksPortalDev"
)

$ErrorActionPreference = "Stop"

# SQL to drop and create database
$sql = @"
-- Drop existing connections and database
IF EXISTS (SELECT name FROM sys.databases WHERE name = N'$DatabaseName')
BEGIN
    ALTER DATABASE [$DatabaseName] SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
    DROP DATABASE [$DatabaseName];
    PRINT 'Dropped existing database: $DatabaseName';
END

-- Create fresh database
CREATE DATABASE [$DatabaseName];
PRINT 'Created database: $DatabaseName';

-- Optional: Set recovery model to Simple for dev (less log growth)
ALTER DATABASE [$DatabaseName] SET RECOVERY SIMPLE;
PRINT 'Set recovery model to SIMPLE';
"@

Write-Host ""
Write-Host "============================================" -ForegroundColor Cyan
Write-Host " Books Portal - Database Setup" -ForegroundColor Cyan
Write-Host "============================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "Server:   $ServerInstance" -ForegroundColor Gray
Write-Host "Database: $DatabaseName" -ForegroundColor Gray
Write-Host "Auth:     Windows Authentication" -ForegroundColor Gray
Write-Host ""

try {
    # Method 1: Using SqlServer module (preferred)
    if (Get-Module -ListAvailable -Name SqlServer) {
        Write-Host "Using SqlServer module..." -ForegroundColor Yellow
        Invoke-Sqlcmd -ServerInstance $ServerInstance -Query $sql -TrustServerCertificate
    }
    # Method 2: Using sqlcmd CLI
    else {
        Write-Host "SqlServer module not found, using sqlcmd..." -ForegroundColor Yellow
        $sql | sqlcmd -S $ServerInstance -E -b
        if ($LASTEXITCODE -ne 0) { throw "sqlcmd failed with exit code $LASTEXITCODE" }
    }

    Write-Host ""
    Write-Host "Database '$DatabaseName' created successfully!" -ForegroundColor Green
    Write-Host ""
    
    # Output connection string for appsettings.json
    Write-Host "Connection string for appsettings.json:" -ForegroundColor Cyan
    Write-Host ""
    Write-Host "  Server=$ServerInstance;Database=$DatabaseName;Trusted_Connection=True;TrustServerCertificate=True;" -ForegroundColor White
    Write-Host ""
}
catch {
    Write-Host ""
    Write-Host "Error: $_" -ForegroundColor Red
    Write-Host ""
    
    # Help message if SqlServer module is missing
    if ($_.Exception.Message -like "*sqlcmd*" -or $_.Exception.Message -like "*Invoke-Sqlcmd*") {
        Write-Host "To install SqlServer module, run:" -ForegroundColor Yellow
        Write-Host "  Install-Module -Name SqlServer -Scope CurrentUser" -ForegroundColor White
        Write-Host ""
    }
    
    exit 1
}
