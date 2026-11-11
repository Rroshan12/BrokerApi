# House Broker API

A REST API (MVP) for a house broker platform. Brokers list properties, house seekers search them,
and each listing carries an estimated broker commission that only its owning broker can see.

**Stack:** .NET 10 · ASP.NET Core Web API · EF Core + SQL Server · ASP.NET Identity + JWT ·
FluentValidation · API versioning · output caching · xUnit + Moq + FluentAssertions

## Project structure (Clean Architecture)

Dependencies point inward only: `Api → Infrastructure → Application → Domain`.

```
src/
  HouseBroker.Domain/          Entities and business rules. No dependencies.
                               PropertyListing, PropertyImage, CommissionRule (band match + rounding), Roles
  HouseBroker.Application/     Use cases, DTOs, validators and the interfaces Infrastructure implements
    Auth/                      Register/login, AccountType (Seeker | Broker)
    Commission/                CommissionCalculator: picks the DB band for a price
    Listings/                  ListingService: CRUD, ownership, commission visibility, search
    Common/Pagination/         PageRequest (+ validator) and PagedResult, shared by every list endpoint
    Common/                    Exceptions, ICurrentUserService, ICacheInvalidator, cache tags
  HouseBroker.Infrastructure/  EF Core/MSSQL, Identity, JWT issuing, repositories, migrations, seeding
                               (ToPagedResultAsync, listing filters/sorting and SQL projections)
  HouseBroker.Api/             Controllers, JWT auth, versioning, ProblemDetails, output cache, Swagger
                               (all registered in AddPresentation; Program.cs only builds the pipeline)
tests/
  HouseBroker.Application.Tests/   Commission engine, listing service, auth service
  HouseBroker.Api.Tests/           Controllers (mocked services) and the exception → ProblemDetails mapping
```

Every layer's entry file (`DependencyInjection.cs`, `Program.cs`, `BaseEntity.cs`) starts with a comment
explaining that layer's role.

## Prerequisites
- .NET SDK 10
- SQL Server (LocalDB, Express, Developer or Docker)
- EF Core CLI: `dotnet tool install --global dotnet-ef`

## Setup

1. **Set the secrets.** They are kept out of source control with user-secrets:

   ```bash
   cd src/HouseBroker.Api
   dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Server=localhost;Database=HouseBroker;Trusted_Connection=True;TrustServerCertificate=True"
   dotnet user-secrets set "Jwt:Key" "<a random secret of at least 32 characters>"
   ```

   Optional: `Jwt:Issuer`, `Jwt:Audience` and `Jwt:ExpiryMinutes` (default 60). In production, set the same
   keys as environment variables (`ConnectionStrings__DefaultConnection`, `Jwt__Key`).

2. **Create the database.** Choose one:
   - **Development (automatic):** running the app in the `Development` environment applies the migrations on
     startup and seeds demo data (see below).
   - **Manual:**
     ```bash
     dotnet ef database update -p src/HouseBroker.Infrastructure -s src/HouseBroker.Api
     ```

   The migration creates the Identity tables, the `Broker` and `Seeker` roles, and these commission rules:

   | Price (INR) | Rate |
   |---|---|
   | below 50,00,000 | 2% |
   | 50,00,000 to 1,00,00,000, both included | 1.75% |
   | above 1,00,00,000 | 1.5% |

   The assignment doesn't say which band the exact boundaries belong to. Here, 50 lakh and exactly 1 crore
   both get 1.75%, because "between 50,00,000 and 1 crore" reads as including both ends.

3. **Run:**

   ```bash
   dotnet run --project src/HouseBroker.Api
   ```

   Open `/swagger`. Log in with `POST /api/v1/auth/login`, click **Authorize**, and paste the `accessToken`.

4. **Test:** `dotnet test`

### Demo accounts (Development only)

| Email | Password | Role |
|---|---|---|
| `broker@demo.local` | `Demo@12345` | Broker (owns 4 demo listings) |
| `seeker@demo.local` | `Demo@12345` | Seeker |

Demo data is created only when the database has no users and the app runs in `Development`.

## API (v1)

| Method | Route | Access | Notes |
|---|---|---|---|
| POST | `/api/v1/auth/register` | anonymous | `accountType`: `Seeker` or `Broker`. Brokers must give a phone number. Returns a JWT. |
| POST | `/api/v1/auth/login` | anonymous | Returns a JWT. The account locks for 15 minutes after 5 failed attempts. |
| GET | `/api/v1/listings` | anyone | Search. See the filters below. Output-cached. |
| GET | `/api/v1/listings/{id}` | anyone | Details, images, description, broker contact. Output-cached for anonymous callers. |
| GET | `/api/v1/listings/mine` | Broker | Your own listings **with commission**. |
| POST | `/api/v1/listings` | Broker | Create. Commission calculated from the DB. `201` + `Location`. |
| PUT | `/api/v1/listings/{id}` | owning Broker | Replace. Commission recalculated. `204`. |
| DELETE | `/api/v1/listings/{id}` | owning Broker | `204`. |
| GET | `/api/v1/commission/rules` | Broker | Active commission bands. |
| GET | `/api/v1/commission/estimate?price=` | Broker | Estimate before listing. |
| GET | `/api/v1/health` | anyone | Liveness check. |

**Search filters** (all optional, combined with AND): `location` (free text matched against city, area and
address), `city` (exact), `propertyType` (`House`, `Apartment`, `Land`, `Commercial`), `minPrice`,
`maxPrice`, `minBedrooms`, `minBathrooms`, `features` (repeatable; the listing must have all of them),
`sortBy` (`Newest`, `PriceAsc`, `PriceDesc`), `page`, `pageSize` (1 to 50).

```
GET /api/v1/listings?location=Kathmandu&propertyType=Apartment&minPrice=2000000&maxPrice=6000000&features=Parking&sortBy=PriceAsc
```

## Design notes

- **Seekers and brokers.** At registration, the account type becomes an Identity role, which is carried in the
  JWT `role` claim. Create, update and delete require `Broker`. Ownership is checked in `ListingService`, so a
  broker can't change another broker's listing; doing so returns `403`.
- **Commission engine.** `CommissionCalculator` reads the active rows of the `CommissionRules` table. Bands are
  `[MinPrice, MaxPrice)`, and amounts are rounded to 2 decimals (midpoint away from zero). The amount and rate
  are stored on the listing when it is created or updated. To change a rate, update the row; listings pick up
  the new rate the next time they are saved.
- **Commission visibility.** Commission is returned only when the caller is the owning broker: on listing
  details, on `listings/mine`, and in the create response. For everyone else the fields are `null`, and they're
  left out of the JSON entirely.
- **Caching: why output cache.** Most traffic on a listing site is the same public reads repeated: search
  pages and listing details. Output caching stores the whole HTTP response for 60 seconds, so a cache hit
  skips the controller, the service, the database and JSON serialization. It's built into ASP.NET Core and
  takes little code. All listing responses are tagged `listings`, and every create, update and delete evicts
  that tag through `ICacheInvalidator`. The Application layer declares that interface without knowing how
  caching works, and the Api layer implements it, so the dependency rule holds.
  - **Commission stays private.** The built-in policy never caches a request that sends an `Authorization`
    header. A broker's response containing commission is therefore never stored or served to anyone else,
    with no extra code needed.
  - **Trade-off.** Logged-in callers aren't cached, so they always read from the database. That's acceptable
    for an MVP because their responses differ per user anyway.
  - **Next step: Redis.** The store is in memory, so each instance has its own cache. For several instances
    the plan is to switch to the Redis output-cache store (`AddStackExchangeRedisOutputCache`). Cache entries
    and tag eviction are then shared across instances, and controllers and services don't change.
- **Rate limiting: left to the edge.** The assignment doesn't require it, and an in-app limiter counts
  requests in each instance's own memory. Behind a load balancer with N instances the real limit becomes
  N times the configured one, so it gives a false sense of protection. Rate limiting and bot protection
  belong in front of the app, in an API gateway, WAF or reverse proxy (for example Azure Front Door, AWS WAF,
  Cloudflare or nginx), which sees every request. Inside the app, brute-force protection comes from
  Identity's account lockout. That is stored in the database, so it works the same across instances.
- **Validation.** FluentValidation validators in Application, with limits that match the database columns.
  The database also has check constraints (`Price > 0`, rooms ≥ 0) and foreign keys.
- **Errors.** Every error is RFC 7807 ProblemDetails, built through MVC's `ProblemDetailsFactory`, so it has
  the same shape and `traceId` as automatic model-binding errors. Mapping: validation `400`, bad login `401`,
  not owner or wrong role `403`, missing `404`, duplicate email `409`, otherwise `500` (no internal details).
- **Third-party use.** The version is in the URL (`api/v1`, with an `api-supported-versions` response header).
  It uses bearer tokens, a CORS policy for browser clients, enums as strings, and Swagger with XML docs.
- **Images.** Listings store image URLs (absolute http/https). Uploading files is out of scope for this MVP.
- **Resilience (planned).** Polly circuit breakers for SQL Server, Redis and future outgoing HTTP calls are
  described in [docs/resilience.md](docs/resilience.md).
