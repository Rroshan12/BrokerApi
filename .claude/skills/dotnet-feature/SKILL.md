---
name: dotnet-feature
description: Implement a feature end-to-end across the Clean Architecture layers of the House Broker API (Domain entity → Application DTOs/validator/service/interface → Infrastructure repository/EF config → Api controller → tests). Use when the user asks to add an endpoint, resource or feature.
---

# Add a .NET feature across layers

Argument: the feature to build (e.g. "listing CRUD", "search with filters", "auth register/login").

Follow `.claude/rules/` throughout. Work inside-out, building after each layer:

1. **Domain** (`src/HouseBroker.Domain`) — add/extend entities, enums, invariant methods. No packages.
2. **Application** (`src/HouseBroker.Application/<Feature>/`)
   - `record` request/response DTOs
   - FluentValidation validator per request
   - interfaces for anything Infrastructure provides (`I<Feature>Repository`, `ICacheService`, ...)
   - service interface + implementation (business rules, ownership checks, commission)
   - register in `Application/DependencyInjection.cs`
3. **Infrastructure** (`src/HouseBroker.Infrastructure`)
   - `IEntityTypeConfiguration<T>` for new entities/columns
   - repository implementation (`AsNoTracking` for reads), cache invalidation
   - register in `Infrastructure/DependencyInjection.cs`
   - if the model changed → run the `/ef-migration` skill
4. **Api** (`src/HouseBroker.Api/Controllers`) — thin controller, `api/v1/...` route, correct status codes, `[Authorize]` roles, `[ProducesResponseType]`.
5. **Tests** — delegate to the `dotnet-test-writer` subagent for service + controller tests.
6. **Verify** — `dotnet build` and `dotnet test` green; then ask the `clean-arch-reviewer` subagent to review.

Report: files created/changed per layer, endpoints added (method + route + roles), test results.
