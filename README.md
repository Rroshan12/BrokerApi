# House Broker API

A REST API (MVP) for a house broker platform: brokers list properties, house seekers search them,
and each listing carries an estimated broker commission visible only to its owner.

**Stack:** .NET 10 · ASP.NET Core Web API · EF Core + SQL Server · ASP.NET Identity + JWT · FluentValidation · xUnit

## Project structure (Clean Architecture)

```
src/
  HouseBroker.Domain/          Entities and business rules (no dependencies)
  HouseBroker.Application/     Use cases, DTOs, validation, interfaces
  HouseBroker.Infrastructure/  EF Core/MSSQL, Identity, caching, repositories
  HouseBroker.Api/             Controllers, JWT auth, Swagger (composition root)
tests/
  HouseBroker.Application.Tests/
  HouseBroker.Api.Tests/
```

## Prerequisites
- .NET SDK 10
- SQL Server (LocalDB, Express, Developer or Docker)
- EF Core CLI: `dotnet tool install --global dotnet-ef`

## Setup

1. **Configure secrets** (kept out of source control via user-secrets):

   ```bash
   cd src/HouseBroker.Api
   dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Server=localhost;Database=HouseBroker;Trusted_Connection=True;TrustServerCertificate=True"
   dotnet user-secrets set "Jwt:Key" "<a random secret of at least 32 characters>"
   ```

2. **Create the database:**

   ```bash
   dotnet ef migrations add InitialCreate -p src/HouseBroker.Infrastructure -s src/HouseBroker.Api -o Persistence/Migrations
   dotnet ef database update -p src/HouseBroker.Infrastructure -s src/HouseBroker.Api
   ```

   This seeds the commission rules:

   | Price (INR) | Rate |
   |---|---|
   | below 50,00,000 | 2% |
   | 50,00,000 – 1,00,00,000 (inclusive) | 1.75% |
   | above 1,00,00,000 | 1.5% |

3. **Run:**

   ```bash
   dotnet run --project src/HouseBroker.Api
   ```

   Open `/swagger` to explore and test the API.

4. **Test:** `dotnet test`
