# Start Books Portal - Backend + Frontend Development Servers
# Usage: .\start-dev.ps1

Write-Host "Starting Books Portal Development Environment..." -ForegroundColor Cyan
Write-Host ""

# Start Backend API
Write-Host "[Backend] Starting ASP.NET Core API..." -ForegroundColor Green
$backend = Start-Process powershell -ArgumentList "-NoExit", "-Command", "cd '$PSScriptRoot\BooksPortal\src\BooksPortal.API'; Write-Host 'Backend API Server' -ForegroundColor Green; dotnet run" -PassThru

# Wait a bit for backend to initialize
Start-Sleep -Seconds 2

# Start Frontend
Write-Host "[Frontend] Starting Nuxt Dev Server..." -ForegroundColor Blue
$frontend = Start-Process powershell -ArgumentList "-NoExit", "-Command", "cd '$PSScriptRoot\BooksPortalFrontEnd'; Write-Host 'Frontend Dev Server' -ForegroundColor Blue; bun run dev" -PassThru

Write-Host ""
Write-Host "Development servers started!" -ForegroundColor Cyan
Write-Host "  Backend:  https://localhost:5001/api" -ForegroundColor Green
Write-Host "  Frontend: http://localhost:3000" -ForegroundColor Blue
Write-Host ""
Write-Host "Press Ctrl+C in each terminal window to stop the servers." -ForegroundColor Yellow
Write-Host ""

# Keep this window open
Read-Host "Press Enter to close this launcher window (servers will keep running)"
