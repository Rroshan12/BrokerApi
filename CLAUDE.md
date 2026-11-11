# House Broker API

API-only MVP for a house broker assignment: **.NET 10 · ASP.NET Core Web API · EF Core + MSSQL · Identity + JWT · Clean Architecture**.
No UI — Swagger is the client.

## Map
- `src/HouseBroker.Domain` — entities, enums, business rules (no dependencies)
- `src/HouseBroker.Application` — services, DTOs, validators, interfaces
- `src/HouseBroker.Infrastructure` — EF Core, Identity, caching, repositories
- `src/HouseBroker.Api` — controllers, auth, Swagger, middleware (composition root)
- `tests/` — xUnit + Moq + FluentAssertions

## Commands
- Build: `dotnet build` · Test: `dotnet test` · Run: `dotnet run --project src/HouseBroker.Api`
- Migrations: use the `/ef-migration` skill

## Where things live
- **Rules** (auto-loaded, some only for matching files): `.claude/rules/`
- **Subagents**: `.claude/agents/` — `clean-arch-reviewer`, `dotnet-test-writer`, `security-reviewer`
- **Skills**: `.claude/skills/` — `/dotnet-feature`, `/ef-migration`, `/pre-commit-check`

## Non-negotiables
1. Dependency rule: Domain ← Application ← Infrastructure ← Api. Never point outward.
2. Commission rates come from the `CommissionRules` table; commission is visible only to the owning broker.
3. Never read or edit `appsettings.*.json`, user secrets, `.env*`. Ask the user to change config.
4. Commits and PRs carry **no Claude / AI attribution** (no Co-Authored-By, no "Generated with").
5. `dotnet build` and `dotnet test` must pass before saying a task is done.
