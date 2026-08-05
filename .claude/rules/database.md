---
paths:
  - "src/WebApi/Common/Persistence/**/*"
  - "tools/Seeder/**/*"
---

# Database

## Adding a New Entity

1. Domain — `src/WebApi/Common/Domain/{Entity}/` (entity, ID, spec, errors). See [domain.md](domain.md).
2. EF configuration — `src/WebApi/Common/Persistence/{Entity}/{Entity}Configuration.cs` implementing `IEntityTypeConfiguration<T>`.
3. DbSet — add a `partial ApplicationDbContext` file at `src/WebApi/Common/Persistence/ApplicationDbContext.{Entities}.cs` exposing `DbSet<{Entity}>`.
4. Register the strongly typed ID in `VogenEfCoreConverters` — startup fails otherwise.
5. Add a migration (command below).

## Migrations

```bash
dotnet ef migrations add MigrationName \
  --project src/WebApi/WebApi.csproj \
  --startup-project src/WebApi/WebApi.csproj \
  --output-dir Common/Persistence/Migrations
```

Migrations apply automatically in dev, so you don't need to run `database update` by hand. The
AppHost declares a `migrations` resource (`AddEFMigrations` + `RunDatabaseUpdateOnStart`) that runs
`dotnet ef database update` before the API starts. That resource shells out to the `dotnet-ef` tool,
so `dotnet tool restore` must have been run — `.config/dotnet-tools.json` pins the matching version.

On Azure this is a deployment step, not something the app does on boot: publishing emits a
migration bundle to `efmigrations/` and running it is up to the pipeline. See the README.

## Seeding

- Lives in `tools/Seeder/Initializers/`.
- Implement `SeedDataAsync()`, taking `ApplicationDbContext` on the constructor.
- Dev-only, enforced by the AppHost: the `seeder` resource is only added under
  `builder.ExecutionContext.IsRunMode`, so it can never reach a deployed environment. There is no
  environment check in `Worker.cs` to keep in sync.
- Idempotent: short-circuit if data already exists (`if (dbContext.Heroes.Any()) return;`).
- Bogus for fake data: `new Faker<Hero>().CustomInstantiator(f => Hero.Create(...)).Generate(20)`.
