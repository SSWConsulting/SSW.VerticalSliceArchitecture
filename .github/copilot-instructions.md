# Copilot Instructions for SSW Vertical Slice Architecture

## Project Overview
This is an enterprise-ready Vertical Slice Architecture template for .NET 9 with Aspire orchestration. Each feature is organized as a self-contained vertical slice in `src/WebApi/Features/`, with shared domain models in `Common/Domain/` and infrastructure in `Common/`.

## Architecture Patterns

### Vertical Slice Organization
- **Features**: `src/WebApi/Features/{FeatureName}/` contains Commands, Queries, and a `{FeatureName}Feature.cs` implementing `IFeature`
- **Commands/Queries**: Each is a static class containing nested `Request`, `Handler`, `Validator`, and `Endpoint` classes
- **Handlers**: Always named `Handler` (enforced by architecture tests in `tests/WebApi.ArchitectureTests/`)
- **Endpoints**: Implement `IEndpoint` with `MapEndpoint(IEndpointRouteBuilder)` - auto-discovered via reflection in `Host/EndpointDiscovery.cs`

### Domain Layer (`Common/Domain/`)
- **Entities**: Inherit from `Entity<TId>` or `AggregateRoot<TId>` for domain events
- **Strongly Typed IDs**: Use `[ValueObject<Guid>]` from Vogen (e.g., `HeroId`, `TeamId`)
  - **CRITICAL**: Register ALL new strongly typed IDs in `Common/Persistence/VogenEfCoreConverters.cs` with `[EfCoreConverter<YourId>]`
  - IDs use `Guid.CreateVersion7()` for time-ordered GUIDs
- **Value Objects**: Encapsulate invariants with private setters and validation (e.g., `Power`, `Mission`)
- **Domain Events**: Inherit from `DomainEvent`, raised via `AddDomainEvent()` on aggregates
- **Specifications**: Use Ardalis.Specification for loading aggregates and commonly used queries
  - Place specs in `Common/Domain/{Entity}/` (e.g., `HeroByIdSpec`, `TeamByIdSpec`)
  - Inherit from `SingleResultSpecification<T>` or `Specification<T>` 
  - Apply with `.WithSpecification(new YourSpec())` on DbSet queries
  - Example: `dbContext.Heroes.WithSpecification(new HeroByIdSpec(heroId)).FirstOrDefault()`

### Request/Response Flow
1. **Endpoint** receives HTTP request → sends `Request` via MediatR `ISender`
2. **ValidationErrorOrResultBehavior** validates using FluentValidation → returns `ErrorOr<T>` on failure
3. **Handler** executes business logic → returns `ErrorOr<TResponse>`
4. **Endpoint** maps result: `.Match(TypedResults.Ok, CustomResult.Problem)` for queries, `.Match(_ => TypedResults.Created(), CustomResult.Problem)` for commands
5. Use endpoint extension methods: `ProducesGet<T>()`, `ProducesPost()`, `ProducesPut()`, `ProducesDelete()`

### Error Handling
- Use `ErrorOr<T>` result pattern (not exceptions for flow control)
- Return `Error.Validation()`, `Error.NotFound()`, `Error.Conflict()`, etc.
- For eventual consistency failures in domain event handlers, throw `EventualConsistencyException` (handled by middleware)

## Adding New Features

### Quick Template Command
```bash
cd src/WebApi/
dotnet new ssw-vsa-slice --feature Person --feature-plural People
```

### Manual Steps After Template Generation
1. **Register Strongly Typed ID**: Add `[EfCoreConverter<PersonId>]` to `VogenEfCoreConverters` class
2. **Create Migration**: 
   ```bash
   dotnet ef migrations add PersonTable --project src/WebApi/WebApi.csproj --startup-project src/WebApi/WebApi.csproj --output-dir Common/Database/Migrations
   ```
3. **Entity Configuration**: Create `{Entity}Configuration.cs` in `Common/Persistence/` implementing `IEntityTypeConfiguration<T>`

## Testing Strategy

### Unit Tests (`tests/WebApi.UnitTests/`)
- Test domain logic, value objects, and entity invariants
- No EF Core mocking needed - pure domain tests
- Example: `Features/Heroes/HeroTests.cs` validates `Hero.Create()` and `UpdatePowers()`

### Integration Tests (`tests/WebApi.IntegrationTests/`)
- Inherit from `IntegrationTestBase` for `TestingDatabaseFixture`
- Uses **TestContainers** (SQL Server) + **Respawn** for database cleanup between tests
- Use `GetAnonymousClient()` for HTTP client, `GetQueryable<T>()` for EF queries
- Tests run against real database - fast due to Respawn
- Example: `Endpoints/Heroes/Commands/CreateHeroCommandTests.cs`

### Architecture Tests (`tests/WebApi.ArchitectureTests/`)
- Enforces naming: handlers must be named `Handler`
- Validates layer dependencies and namespace conventions
- Run with `dotnet test` - failures indicate architectural violations

## Development Workflows

### Running the Application
```bash
cd tools/AppHost/
dotnet run
```
- Opens Aspire Dashboard for observability
- Auto-provisions SQL Server (Docker/Podman), runs migrations, seeds data
- Access API at https://localhost:7255/scalar/v1 (Scalar UI for OpenAPI)

### Database Migrations
```bash
# Add migration
dotnet ef migrations add MigrationName --project src/WebApi/WebApi.csproj --startup-project src/WebApi/WebApi.csproj --output-dir Common/Database/Migrations

# Migrations run automatically via MigrationService on startup
```

### Running Tests
```bash
# All tests
dotnet test

# Specific test project
dotnet test tests/WebApi.IntegrationTests/
```

## Key Conventions

### Endpoint Routing
- Use `endpoints.MapApiGroup(FeatureName)` to create `/api/{featurename}` routes with OpenAPI tags
- Example: `HeroesFeature.FeatureName` → `/api/heroes`

### Global Usings (`GlobalUsings.cs`)
- `EntityFrameworkCore`, `FluentValidation`, `ErrorOr`, `Vogen`, `Ardalis.Specification` pre-imported
- `SSW.VerticalSliceArchitecture.Common.Features` and `.Common.Persistence` available globally

### Build Configuration
- `Directory.Build.props`: Treats warnings as errors in Release builds, enforces code style
- `Directory.Packages.props`: Central package management - update versions here
- Release builds enforce `TreatWarningsAsErrors` and `EnforceCodeStyleInBuild`

### MediatR Behaviors (Order Matters)
1. `UnhandledExceptionBehaviour` - catches unhandled exceptions
2. `ValidationErrorOrResultBehavior` - validates requests, returns `ErrorOr` errors
3. `PerformanceBehaviour` - logs slow requests

### Domain Events & Eventual Consistency
- Events dispatched by `DispatchDomainEventsInterceptor` after `SaveChangesAsync()`
- Event handlers run in same transaction - if they fail, throw `EventualConsistencyException`
- `EventualConsistencyMiddleware` catches these and returns appropriate HTTP errors
- Example: `PowerLevelUpdatedEventHandler` updates team power when hero power changes

## Aspire Configuration
- `AppHost` project orchestrates services: SQL Server, MigrationService, WebApi
- `ServiceDefaults` provides telemetry, health checks, service discovery
- `builder.AddServiceDefaults()` in `Program.cs` enables observability

## Data Seeding
- Seeding happens in `tools/MigrationService/Initializers/ApplicationDbContextInitializer.cs`
- Only runs in Development environment (controlled in `Worker.cs`)
- Uses **Bogus** library for fake data generation
- Pattern: Create initializer inheriting from `DbContextInitializerBase<T>`, implement `SeedDataAsync()`
- Seeds run in transaction for atomicity
- Check for existing data before seeding: `if (DbContext.Heroes.Any()) return;`
- Example seed flow:
  ```csharp
  var faker = new Faker<Hero>()
      .CustomInstantiator(f => Hero.Create(f.Person.FirstName, f.Random.AlphaNumeric(2)));
  var heroes = faker.Generate(20);
  await DbContext.Heroes.AddRangeAsync(heroes);
  await DbContext.SaveChangesAsync();
  ```

## Common Pitfalls
1. Forgetting to register strongly typed IDs in `VogenEfCoreConverters` → runtime EF Core errors
2. Not using `ProducesGet/Post/Put/Delete()` extensions → inconsistent OpenAPI documentation
3. Throwing exceptions in handlers instead of returning `ErrorOr<T>` → breaks error handling pipeline
4. Not inheriting handlers from nested `Handler` class → architecture test failures
5. Loading aggregates without specifications → missing related entities or inconsistent query patterns

## What's NOT Included
- **Authentication/Authorization**: This is a template - implement auth as needed for your use case
- **Feature-specific service registration**: Most features don't need custom DI - use `IFeature.ConfigureServices()` only when required
