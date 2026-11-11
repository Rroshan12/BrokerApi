---
paths:
  - "src/HouseBroker.Infrastructure/**"
---

# EF Core / MSSQL rules

- Configure entities with `IEntityTypeConfiguration<T>` classes in `Persistence/Configurations/` — no data annotations on Domain entities.
- Every `decimal` gets `HasPrecision(18, 2)` (rates: `HasPrecision(5, 3)`); strings get `HasMaxLength`.
- Add indexes for every search filter column (City, Price, PropertyType).
- Read queries use `AsNoTracking()` and project to DTOs with `Select` when possible.
- Seed data via `HasData` with fixed Guids and fixed dates (never `Guid.NewGuid()` / `DateTime.UtcNow` in seeds).
- Migrations go in `Persistence/Migrations`. Never edit an applied migration; add a new one.
- `dotnet ef database update` / `drop` require user approval — never run them silently.
- Caching is ASP.NET Core **output caching** in the Api layer (`[OutputCache(PolicyName = OutputCachePolicies.Listings)]`, tag `CacheTags.Listings`). Services evict via `ICacheInvalidator` after every create/update/delete. Only anonymous responses are cached — never a response containing commission.
