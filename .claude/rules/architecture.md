# Clean Architecture rules

- **Domain**: plain C# only. No EF, ASP.NET, FluentValidation or any NuGet package. Entities may hold small invariant methods (e.g. `CommissionRule.Matches`).
- **Application**: depends on Domain only. Declares interfaces (`IListingRepository`, `ICacheInvalidator`, `ICurrentUserService`, ...) that Infrastructure implements. Holds services, DTOs, validators, mapping.
- **Infrastructure**: implements Application interfaces. All EF Core, Identity, JWT issuing, file storage live here. No business rules. (HTTP-specific adapters — current user, output-cache eviction — live in Api/Services.)
- **Api**: thin controllers — validate → call one Application service → map to HTTP result. No `DbContext` in controllers.
- Each layer registers its services in its own `DependencyInjection.cs` (`AddApplication`, `AddInfrastructure`).
- Never return EF entities from controllers; always return DTOs.
- Every layer's key files start with a short comment explaining the layer's role (assignment requirement).
- Folder per feature inside Application: `Listings/`, `Commission/`, `Auth/` (DTOs, validators, service, interface together).
