---
paths:
  - "src/WebApi/Common/Persistence/**/*"
  - "tools/MigrationService/**/*"
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

Migrations apply automatically on startup via `tools/MigrationService`, so you don't need to run `database update` manually in dev.

## Seeding

- Lives in `tools/MigrationService/Initializers/`.
- Inherit `DbContextInitializerBase<T>` and implement `SeedDataAsync()`.
- Dev-only: `Worker.cs` gates seeding by environment.
- Idempotent: short-circuit if data already exists (`if (DbContext.Heroes.Any()) return;`).
- Bogus for fake data: `new Faker<Hero>().CustomInstantiator(f => Hero.Create(...)).Generate(20)`.
