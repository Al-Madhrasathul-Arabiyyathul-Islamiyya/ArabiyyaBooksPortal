# ============================================
# Books Portal - Backend Scaffolding (PowerShell 7)
# ============================================
# Run these commands in your desired parent directory

# --------------------------------------------
# 1. Create Solution
# --------------------------------------------
mkdir BooksPortal
cd BooksPortal
dotnet new sln -n BooksPortal

# --------------------------------------------
# 2. Create Projects
# --------------------------------------------
mkdir src

dotnet new classlib -n BooksPortal.Domain -o src/BooksPortal.Domain -f net10.0
dotnet new classlib -n BooksPortal.Application -o src/BooksPortal.Application -f net10.0
dotnet new classlib -n BooksPortal.Infrastructure -o src/BooksPortal.Infrastructure -f net10.0
dotnet new webapi -n BooksPortal.API -o src/BooksPortal.API -f net10.0 --no-openapi

# --------------------------------------------
# 3. Add Projects to Solution
# --------------------------------------------
dotnet sln add src/BooksPortal.Domain/BooksPortal.Domain.csproj
dotnet sln add src/BooksPortal.Application/BooksPortal.Application.csproj
dotnet sln add src/BooksPortal.Infrastructure/BooksPortal.Infrastructure.csproj
dotnet sln add src/BooksPortal.API/BooksPortal.API.csproj

# --------------------------------------------
# 4. Add Project References
# --------------------------------------------
dotnet add src/BooksPortal.Application reference src/BooksPortal.Domain
dotnet add src/BooksPortal.Infrastructure reference src/BooksPortal.Application
dotnet add src/BooksPortal.API reference src/BooksPortal.Infrastructure

# --------------------------------------------
# 5. Install NuGet Packages - Application
# --------------------------------------------
dotnet add src/BooksPortal.Application package FluentValidation
dotnet add src/BooksPortal.Application package FluentValidation.DependencyInjectionExtensions
dotnet add src/BooksPortal.Application package Mapster
dotnet add src/BooksPortal.Application package Mapster.DependencyInjection

# --------------------------------------------
# 6. Install NuGet Packages - Infrastructure
# --------------------------------------------
dotnet add src/BooksPortal.Infrastructure package Microsoft.EntityFrameworkCore.SqlServer
dotnet add src/BooksPortal.Infrastructure package Microsoft.EntityFrameworkCore.Tools
dotnet add src/BooksPortal.Infrastructure package Microsoft.AspNetCore.Identity.EntityFrameworkCore
dotnet add src/BooksPortal.Infrastructure package Serilog.Sinks.MSSqlServer

# --------------------------------------------
# 7. Install NuGet Packages - API
# --------------------------------------------
dotnet add src/BooksPortal.API package Microsoft.AspNetCore.Authentication.JwtBearer
dotnet add src/BooksPortal.API package FluentValidation.AspNetCore
dotnet add src/BooksPortal.API package Serilog.AspNetCore
dotnet add src/BooksPortal.API package Swashbuckle.AspNetCore
dotnet add src/BooksPortal.API package QuestPDF

# --------------------------------------------
# 8. Create Folder Structure
# --------------------------------------------

# Domain
mkdir src/BooksPortal.Domain/Entities
mkdir src/BooksPortal.Domain/Enums
mkdir src/BooksPortal.Domain/Common

# Application
mkdir src/BooksPortal.Application/Common
mkdir src/BooksPortal.Application/Common/Interfaces
mkdir src/BooksPortal.Application/Common/Mappings
mkdir src/BooksPortal.Application/Common/Behaviors
mkdir src/BooksPortal.Application/Common/Exceptions
mkdir src/BooksPortal.Application/DTOs
mkdir src/BooksPortal.Application/Features
mkdir src/BooksPortal.Application/Features/Auth
mkdir src/BooksPortal.Application/Features/Users
mkdir src/BooksPortal.Application/Features/Books
mkdir src/BooksPortal.Application/Features/Distribution
mkdir src/BooksPortal.Application/Features/Returns
mkdir src/BooksPortal.Application/Features/TeacherIssues
mkdir src/BooksPortal.Application/Features/MasterData
mkdir src/BooksPortal.Application/Features/Reports

# Infrastructure
mkdir src/BooksPortal.Infrastructure/Data
mkdir src/BooksPortal.Infrastructure/Data/Configurations
mkdir src/BooksPortal.Infrastructure/Data/Migrations
mkdir src/BooksPortal.Infrastructure/Repositories
mkdir src/BooksPortal.Infrastructure/Services
mkdir src/BooksPortal.Infrastructure/Identity

# API
mkdir src/BooksPortal.API/Controllers
mkdir src/BooksPortal.API/Middleware
mkdir src/BooksPortal.API/Filters

# Other
mkdir tests
mkdir docker
mkdir docs

# --------------------------------------------
# 9. Build Solution
# --------------------------------------------
dotnet restore
dotnet build

Write-Host "`nScaffolding complete!" -ForegroundColor Green