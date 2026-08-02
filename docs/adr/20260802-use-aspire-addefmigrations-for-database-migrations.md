# Use Aspire's AddEFMigrations for Database Migrations

- Status: accepted
- Deciders: Daniel Mackay, Anton Polkanov
- Date: 2026-08-02
- Tags: aspire, database, migrations, deployment

Technical Story: [#299](https://github.com/SSWConsulting/SSW.VerticalSliceArchitecture/issues/299)

## Context and Problem Statement

`tools/MigrationService` was a hand-rolled `BackgroundService` doing three jobs at once: create the database, apply migrations in-process via `Database.MigrateAsync()`, and seed dev data with Bogus. It was wired into the AppHost as an ordinary project, and on Azure it was published as an App Service with `IsAlwaysOn = true` — a web app that ran once at startup and then idled forever on a B1 plan.

It also had no real boundary around seeding. An `IsDevelopment()` check inside `Worker.cs` was the only thing standing between Bogus data and a production database, and that check depended on an `ASPNETCORE_ENVIRONMENT` value the deploy docs told you to set to `Development`.

Aspire now ships `AddEFMigrations` (`Aspire.Hosting.EntityFrameworkCore`), which models migrations as a first-class resource with dashboard visibility and publish-time artifacts. Should the template keep its own migration runner, or adopt the platform's?

## Decision Drivers

- Migrations should be a visible, inspectable step, not a side effect of a project starting
- Seeding must be structurally incapable of reaching a deployed environment
- Stop paying for an always-on App Service that does nothing after its first minute
- Keep Bogus out of the WebApi production dependency graph

## Considered Options

1. Keep `tools/MigrationService` as-is
2. Adopt `AddEFMigrations`, and trim the existing project to a seed-only `tools/Seeder`
3. Adopt `AddEFMigrations`, and move seeding into EF Core's `UseSeeding` / `UseAsyncSeeding`

## Decision Outcome

Chosen option: **Option 2 — `AddEFMigrations` plus a seed-only `tools/Seeder`**.

The AppHost now declares migrations as their own resource and guards seeding by execution context rather than by environment variable:

```csharp
var migrations = api.AddEFMigrations("migrations")
    .WithReference(db)
    .WaitFor(sqlServer)
    .RunDatabaseUpdateOnStart()
    .PublishAsMigrationBundle();

if (builder.ExecutionContext.IsRunMode)
{
    var seeder = builder.AddProject<Seeder>("seeder")
        .WithReference(db)
        .WaitForCompletion(migrations);

    api.WaitForCompletion(seeder);
}
else
{
    api.WaitForCompletion(migrations);
}
```

`IsRunMode` is the important part. The seeder resource is not merely disabled during publish, it is never added to the graph, so no amount of environment misconfiguration can seed a deployed database. Publishing the template confirms this: the output contains no reference to the seeder at all.

### Consequences

- ✅ Migrations are a real resource — visible in the dashboard, with their own logs and lifecycle
- ✅ Seeding can't reach a deployed environment; the guard is structural, not a runtime string comparison
- ✅ The always-on App Service is gone; Azure now provisions App Service for the API only
- ✅ Bogus stays out of the WebApi dependency graph
- ✅ `DbContextInitializerBase` and its `EnsureDatabaseAsync` / `CreateSchemaAsync` are deleted — `dotnet ef database update` owns both
- ❌ A prerelease package ships in a public template, and consumers inherit any API churn before GA
- ❌ Booting the app now requires a version-matched `dotnet-ef`; previously nothing beyond the SDK was needed
- ❌ `azd up` no longer applies migrations — that becomes a deployment step someone has to own

## Validating the decision

Four questions could have changed the design rather than just the implementation, so each was answered against this repo before committing to it.

**Does `AddEFMigrations` honour a local tool manifest, or only PATH?** It honours the manifest. Aspire invokes `dotnet tool exec dotnet-ef --yes -- database update …`, which resolved the manifest's 10.0.10 on a machine whose PATH offered a global `dotnet-ef` 9.0.3. Pinning in `.config/dotnet-tools.json` therefore works, and `--yes` means a developer who forgets `dotnet tool restore` gets the pinned version fetched on demand rather than an error.

**Does the prerelease package coexist with the stable `Aspire.Hosting.*` 13.4.6 pins?** Yes. Release builds are clean — zero warnings under `TreatWarningsAsErrors` and `CodeAnalysisTreatWarningsAsErrors`, and no `NU1903` from NuGetAudit. The prerelease build number tracks the stable line, so the two move together.

**Does design-time `dotnet ef database update` construct `ApplicationDbContext` correctly?** Yes, including `ConfigureConventions` → `RegisterAllInVogenEfCoreConverters()`. The strongly typed ID converters were the obvious risk, since design time builds the model without the app's DI container, and they resolved without special handling.

**Where does `PublishAsMigrationBundle()` put its output?** At `efmigrations/migrations` in the publish directory — see the negative consequence below, which is the one finding that changed the documentation.

## Applying migrations on Azure

`PublishAsMigrationBundle()` writes an artifact; it does not run one. `PublishAsAzureContainerAppJob()` is the only option that executes automatically on deploy, and it requires Azure Container Apps, which this template does not target. So applying the bundle is a documented pipeline step (see the README) rather than something the platform does.

The bundle is a **self-contained native executable built for the platform that published it** — publishing on an Apple Silicon Mac produces an arm64 Mach-O binary, which will not run on a Linux CI agent or App Service. Whatever runs `aspire publish` must match wherever the bundle will execute. Teams that would rather not think about this, or that need a reviewable artifact for a DBA-gated environment, should swap `PublishAsMigrationBundle()` for `PublishAsMigrationScript()`, which emits an idempotent `.sql` file with no platform coupling.

This is a genuine regression against the old behaviour, where deploying the App Service applied migrations as a side effect of it starting. It is accepted because the old behaviour bought that convenience with an always-on web app and an environment variable that also controlled whether production got seeded with fake superheroes.

## Pros and Cons of the Options

### Option 1 — Keep `tools/MigrationService`

- ✅ No prerelease dependency
- ✅ No new local tooling prerequisite
- ✅ Migrations apply automatically on deploy
- ❌ Migration progress is invisible except as log lines from a project that looks like any other
- ❌ Requires an always-on App Service that idles after its first run
- ❌ Seeding is guarded only by an environment variable the deploy instructions told you to set to `Development`
- ❌ Reimplements database creation and migration that EF Core's own tooling already does

### Option 2 — `AddEFMigrations` plus a seed-only `tools/Seeder`

- ✅ Migrations are a first-class resource with dashboard visibility
- ✅ Seeding is excluded from the published graph entirely
- ✅ Deletes the bespoke initializer base class
- ✅ Drops the always-on App Service
- ❌ Prerelease package in a public template
- ❌ `dotnet-ef` becomes a prerequisite for running the app
- ❌ Migrations on Azure become a pipeline step

### Option 3 — `AddEFMigrations` plus EF Core `UseSeeding` / `UseAsyncSeeding`

- ✅ No separate seeder project to maintain
- ❌ Puts Bogus in the WebApi production dependency graph
- ❌ Seeding runs from inside the app, so an environment check becomes the only thing between a migration bundle and a seeded production database — the exact failure mode this ADR set out to remove

## Links

- [Apply EF Core migrations in Aspire](https://aspire.dev/integrations/databases/efcore/migrations/)
- [Deploy Aspire AppHost projects to Azure App Service](https://aspire.dev/deployment/azure/app-service/)
- [EF Core migration bundles](https://learn.microsoft.com/en-us/ef/core/managing-schemas/migrations/applying#bundles)
- [SSW Rule - Do you use migrations to manage your database schema?](https://www.ssw.com.au/rules/use-migrations-to-manage-your-database-schema/)
