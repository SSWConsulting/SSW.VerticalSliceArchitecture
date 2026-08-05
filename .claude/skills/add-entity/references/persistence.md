# Persistence templates

Placeholders as in [domain.md](domain.md): `{Aggregate}` the folder (`Teams`), `{Entity}` the type (`Team`), `{Entities}` its plural.

---

## EF Core configuration

`src/WebApi/Common/Persistence/{Aggregate}/{Entity}Configuration.cs`

Every entity inherits `AuditableConfiguration<T>` rather than implementing `IEntityTypeConfiguration<T>` directly. The base class maps the `CreatedBy` / `CreatedAt` / `UpdatedBy` audit columns and then calls `PostConfigure`, so entity-specific mapping goes in the override.

```csharp
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SSW.VerticalSliceArchitecture.Common.Domain.{Aggregate};

namespace SSW.VerticalSliceArchitecture.Common.Persistence.{Aggregate};

public class {Entity}Configuration : AuditableConfiguration<{Entity}>
{
    public override void PostConfigure(EntityTypeBuilder<{Entity}> builder)
    {
        builder.HasKey(t => t.Id);

        builder.Property(t => t.Name)
            .HasMaxLength({Entity}.NameMaxLength)
            .IsRequired();
    }
}
```

`HasMaxLength` reads the entity's `const`. Never write the number twice — that's how a column and its guard drift apart.

Nothing registers this class by hand: `ApplicationDbContext.OnModelCreating` calls `ApplyConfigurationsFromAssembly`, so it's picked up as long as it's in the WebApi assembly and implements the interface.

### Owned collections

Two ways to store a child collection, and the choice is a real one:

```csharp
// Separate table — the child is an entity with its own identity and lifecycle.
builder.HasMany(t => t.{Children})
    .WithOne()
    .IsRequired();

// Optional foreign key back to a different aggregate
builder.HasMany(t => t.Heroes)
    .WithOne()
    .HasForeignKey(h => h.TeamId)
    .IsRequired(false);
```

```csharp
// Serialised to a JSON column — the child is a value object with no identity
// of its own and is only ever read as part of the parent.
builder.OwnsMany(t => t.{Children}, b =>
{
    b.ToJson();
    b.Property(t => t.Name)
        .HasMaxLength({Child}.NameMaxLength)
        .IsRequired();
});
```

Pick `ToJson()` when the collection is small, always loaded with its parent, and never queried on its own. Pick a table when you need to filter, join, or page over the children. `HeroConfiguration` uses the JSON form for `Powers`; `TeamConfiguration` uses the table form for `Missions`.

---

## DbSet

`src/WebApi/Common/Persistence/{Aggregate}/ApplicationDbContext.{Entities}.cs`

Aggregate roots only. A child entity has no `DbSet` — it's reached through its aggregate, and that's what keeps the transactional boundary meaningful.

```csharp
using SSW.VerticalSliceArchitecture.Common.Domain.{Aggregate};

// Preserve the namespace across partial classes
// ReSharper disable once CheckNamespace
namespace SSW.VerticalSliceArchitecture.Common.Persistence;

public partial class ApplicationDbContext
{
    public DbSet<{Entity}> {Entities} => AggregateRootSet<{Entity}>();
}
```

`AggregateRootSet<T>()` is constrained to `IAggregateRoot`, so trying this for a plain entity won't compile — the constraint is the guardrail.

The namespace deliberately doesn't match the folder. It has to stay `Common.Persistence` to be part of the same partial class, which is what the ReSharper comment is there to explain.

---

## Vogen registration

`src/WebApi/Common/Persistence/VogenEfCoreConverters.cs`

This file already exists — edit it, don't regenerate it. Add one `using` for your aggregate's namespace *only if it isn't already there*, and one attribute per new ID. A duplicate `using` is CS0105, which Release turns into a build error via `TreatWarningsAsErrors`.

```csharp
using SSW.VerticalSliceArchitecture.Common.Domain.Heroes;
using SSW.VerticalSliceArchitecture.Common.Domain.Teams;
using SSW.VerticalSliceArchitecture.Common.Domain.{Aggregate};   // <-- only if new

namespace SSW.VerticalSliceArchitecture.Common.Persistence;

// TODO: New strongly typed IDs should be registered here

[EfCoreConverter<HeroId>]
[EfCoreConverter<TeamId>]
[EfCoreConverter<MissionId>]
[EfCoreConverter<{Entity}Id>]   // <-- add every new ID, including child entity IDs
internal sealed partial class VogenEfCoreConverters;
```

Add one attribute per ID — child entities included. `ConfigureConventions` calls `RegisterAllInVogenEfCoreConverters()`, which reads this list, so an ID that isn't here has no value converter and EF throws when it first tries to map the property.

That throw happens at **startup**, not at compile time. `dotnet build` stays green. This is the failure the whole skill is built around.

---

## Migration

```bash
dotnet ef migrations add Add{Entity} \
  --project src/WebApi/WebApi.csproj \
  --startup-project src/WebApi/WebApi.csproj \
  --output-dir Common/Persistence/Migrations
```

Needs the EF tool — `dotnet tool restore` if `dotnet ef` isn't found. The version is pinned in `.config/dotnet-tools.json`; don't reach for a global install, which can drift from the solution's EF Core packages.

Migrations apply in dev through the AppHost's `migrations` resource (`AddEFMigrations`), so there's no `database update` to run yourself. Just start the app.

Read the generated file before moving on. Things that show up here:

| Symptom | Cause |
|---|---|
| Empty `Up()` | EF didn't see the entity — missing `DbSet`, or the configuration isn't in the WebApi assembly |
| Column is `nvarchar(max)` | `HasMaxLength` missing from the configuration |
| Unexpected drops on other tables | The model snapshot was out of date; check nothing else was edited |

To undo an unwanted migration, `dotnet ef migrations remove` before it's been applied anywhere. Never edit an already-applied migration — roll forward instead.

---

## Test data factory

`tests/WebApi.IntegrationTests/Common/Factories/{Entity}Factory.cs`

Integration tests seed through these rather than constructing entities inline, which keeps a change to the entity's factory signature to one place.

```csharp
using SSW.VerticalSliceArchitecture.Common.Domain.{Aggregate};

namespace SSW.VerticalSliceArchitecture.IntegrationTests.Common.Factories;

public static class {Entity}Factory
{
    private static readonly Faker<{Entity}> {Entity}Faker = new Faker<{Entity}>().CustomInstantiator(f =>
        {Entity}.Create(f.Company.CompanyName()));

    public static {Entity} Generate() => {Entity}Faker.Generate();

    public static IReadOnlyList<{Entity}> Generate(int count) => {Entity}Faker.Generate(count);
}
```

`Bogus` is a global using in that project, so no `using Bogus;` is needed. `CustomInstantiator` rather than Bogus's property assignment, because the entity's setters are private — the static factory is the only way in, which is exactly what you want the tests exercising.

---

## Dev seeding (optional)

`tools/Seeder/Initializers/ApplicationDbContextInitializer.cs`

Add a private `Seed{Entities}` method and call it from `SeedDataAsync`, inside the existing transaction.

```csharp
private async Task Seed{Entities}()
{
    if (dbContext.{Entities}.Any())
        return;

    var faker = new Faker<{Entity}>()
        .CustomInstantiator(f => {Entity}.Create(f.Company.CompanyName()));

    var {entities} = faker.Generate(Num{Entities});
    await dbContext.{Entities}.AddRangeAsync({entities});
    await dbContext.SaveChangesAsync();
}
```

The `Any()` short-circuit is what makes it idempotent — the initializer runs on every startup, not just the first. Seeding never touches a real environment because the AppHost only adds the `seeder` resource under `builder.ExecutionContext.IsRunMode`.
