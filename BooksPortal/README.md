# BooksPortal Backend

ASP.NET Core Web API for the Books Portal textbook management system.

## Architecture

Clean Architecture with four layers:

| Project | Layer | Purpose |
|---------|-------|---------|
| `BooksPortal.Domain` | Domain | Entities, enums, value objects |
| `BooksPortal.Application` | Application | Services, DTOs, interfaces, validators |
| `BooksPortal.Infrastructure` | Infrastructure | EF Core, repositories, Identity, JWT |
| `BooksPortal.API` | Presentation | Controllers, middleware, DI registration |

## Prerequisites

- .NET 10 SDK
- SQL Server 2022 (or LocalDB)

## Getting Started

```bash
# Restore packages
dotnet restore

# Apply migrations
dotnet ef database update --project src/BooksPortal.Infrastructure --startup-project src/BooksPortal.API

# Run the API
dotnet run --project src/BooksPortal.API
```

The API will start at `https://localhost:5001` by default.

## Connection String

Development uses trusted authentication:

```
Server=.;Database=BooksPortalDev;Trusted_Connection=True;TrustServerCertificate=True;
```

Configure in `src/BooksPortal.API/appsettings.json` (or via environment variables).

## Auth and JWT Configuration

JWT and session-related settings are configured via `JwtSettings`:

- `JwtSettings:ExpiryInMinutes` (default `240`)
- `JwtSettings:RefreshTokenExpiryInDays` (default `7`)
- `JwtSettings:ClockSkewSeconds` (default `30`)
- `JwtSettings:SigningMode` (`Symmetric` or `Certificate`)
- `JwtSettings:Secret` (used in `Symmetric` mode)
- `JwtSettings:CertificateBase64` / `JwtSettings:CertificatePath` / `JwtSettings:CertificatePassword` (used in `Certificate` mode)

The API supports dual signing mode:

- `Symmetric` for local/CI convenience
- `Certificate` for deployment environments

All values can be overridden by environment variables (e.g. `JwtSettings__ExpiryInMinutes=240`).

## CORS Configuration

Allowed frontend origins are configured in:

- `Cors:AllowedOrigins` (array)

Override with environment variables, for example:

- `Cors__AllowedOrigins__0=http://localhost:3000`
- `Cors__AllowedOrigins__1=https://your-frontend.example.com`

## Project Structure

```
src/
  BooksPortal.API/
    Controllers/           # 18 API controllers
    Middleware/             # Exception handling, audit
    Program.cs             # DI registration, pipeline
  BooksPortal.Application/
    Common/                # ApiResponse, PaginatedList, interfaces
    Features/              # Feature folders (Auth, Books, Distribution, etc.)
      {Feature}/
        DTOs/              # Request/response DTOs
        Interfaces/        # Service interfaces
        Services/          # Service implementations
        Validators/        # FluentValidation validators
        Mappings/          # Mapster configurations
  BooksPortal.Domain/
    Entities/              # Entity classes (BaseEntity)
    Enums/                 # Term, BookCondition, MovementType, etc.
  BooksPortal.Infrastructure/
    Data/                  # DbContext, configurations, migrations
    Repositories/          # Generic repository, unit of work
    Identity/              # ASP.NET Identity, JWT service
    Services/              # PDF generation, Excel export, storage
tests/
  BooksPortal.Tests/       # xUnit unit tests (88 tests)
```

## Key Patterns

- **Generic Repository** with `IRepository<T>` and `IUnitOfWork`
- **ApiResponse<T>** wrapper on all endpoints
- **PaginatedList<T>** for list endpoints
- **FluentValidation** for request DTOs
- **Mapster** for entity-to-DTO mapping
- **Soft delete** via `BaseEntity.IsDeleted` with global query filter
- **Audit logging** via SaveChanges interceptor

## Running Tests

```bash
dotnet test
```

## API Reference

See [documentation/api-reference.md](../documentation/api-reference.md) for the complete endpoint reference.
