# House Broker API — project rules

API-only MVP (no UI) for a house broker assignment: .NET 10, MSSQL, Clean Architecture.

## Layers and dependency rule
- `src/HouseBroker.Domain` — entities, enums, pure business rules. References nothing.
- `src/HouseBroker.Application` — services (use cases), DTOs, FluentValidation validators, interfaces. References Domain only.
- `src/HouseBroker.Infrastructure` — EF Core (SQL Server), ASP.NET Identity, caching, repositories. References Application.
- `src/HouseBroker.Api` — controllers, auth, Swagger, middleware. Composition root only; no business logic.
- `tests/*` — xUnit + Moq + FluentAssertions.

Never add a reference that points inward-to-outward (e.g. Application → Infrastructure).

## Conventions
- Routes: `api/v1/...`. Return ProblemDetails for errors (see `GlobalExceptionHandler`).
- Money is `decimal` with `HasPrecision(18,2)`. Commission rates come from the `CommissionRules` table, never hard-coded.
- Commission fields are only populated in responses for the broker who owns the listing.
- Each layer's DI lives in its own `DependencyInjection.cs` (`AddApplication`, `AddInfrastructure`).
- Add an XML/inline comment at the top of each layer's main files explaining the layer's role (assignment requirement).

## Commands
- Build: `dotnet build`
- Test: `dotnet test`
- Migration: `dotnet ef migrations add <Name> -p src/HouseBroker.Infrastructure -s src/HouseBroker.Api -o Persistence/Migrations`
- Apply migration (asks first): `dotnet ef database update -p src/HouseBroker.Infrastructure -s src/HouseBroker.Api`

## Secrets guard rail
Claude must not read or edit `appsettings*.json`, `secrets.json`, `.env*` or `launchSettings.json` (enforced in `.claude/settings.json`).
Connection strings and the JWT key live in **user-secrets**. If a config value needs changing, tell the user the exact key and value to set.
