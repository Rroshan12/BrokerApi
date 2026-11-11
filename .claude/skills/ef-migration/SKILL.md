---
name: ef-migration
description: Create (and optionally apply) an EF Core migration for the House Broker API with the correct project/startup paths. Use when entities or EF configurations change, or the user asks to create/update the database.
---

# EF Core migration

Argument: migration name in PascalCase (e.g. `AddPropertyImages`). Infer one from the change if not given.

1. Make sure `dotnet-ef` exists: `dotnet ef --version`. If missing, tell the user to run `dotnet tool install --global dotnet-ef` (don't install global tools yourself).
2. `dotnet build` — fix errors first.
3. Create:
   ```
   dotnet ef migrations add <Name> -p src/HouseBroker.Infrastructure -s src/HouseBroker.Api -o Persistence/Migrations
   ```
4. Read the generated migration. Flag anything destructive (`DropTable`, `DropColumn`, column type narrowing, data loss) to the user before going further.
5. Only if the user asked to update the database:
   ```
   dotnet ef database update -p src/HouseBroker.Infrastructure -s src/HouseBroker.Api
   ```
   (This prompts for approval by project settings.) If it fails on the connection string, tell the user to set `ConnectionStrings:DefaultConnection` via user-secrets — never read config files.
6. Never edit a migration that has already been applied; create a new one.

Report: migration file path, a 1–3 line summary of schema changes, and whether it was applied.
