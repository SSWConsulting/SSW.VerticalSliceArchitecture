# Copilot Instructions for SSW Vertical Slice Architecture

## Project Overview
This is an enterprise-ready Vertical Slice Architecture template for .NET 9 with Aspire orchestration. Each feature is organized as a self-contained vertical slice in `src/WebApi/Features/`, with shared domain models in `Common/Domain/` and infrastructure in `Common/`.

## Architecture Patterns

### Vertical Slice Organization
- **Features**: `src/WebApi/Features/{FeatureName}/` contains Endpoints and a `{FeatureName}Feature.cs` implementing `IFeature`
- **Endpoints**: Located in `Features/{FeatureName}/Endpoints/` - each endpoint is a separate file inheriting from `Endpoint<TRequest, TResponse>` or `EndpointWithoutRequest<TResponse>`
- **Endpoint Discovery**: FastEndpoints automatically discovers and registers all endpoints at startup
- **Groups**: Each feature defines a `Group` class to configure routing prefix and shared settings (e.g., `HeroesGroup` for `/api/heroes`) 

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

### FastEndpoints

This project uses **FastEndpoints** for defining HTTP endpoints with a clean, strongly-typed API structure.

#### Endpoint Structure
Each feature slice in `Features/{FeatureName}/Endpoints/` contains:
- **Endpoint classes**: Inherit from `Endpoint<TRequest, TResponse>` or `EndpointWithoutRequest<TResponse>`
- **Request records**: Define the input contract (e.g., `CreateHeroRequest`)
- **Response records**: Define the output contract (e.g., `CreateHeroResponse`)
- **Validator classes**: Inherit from `Validator<TRequest>` for request validation
- **Summary classes**: Inherit from `Summary<TEndpoint>` for OpenAPI documentation with examples

#### Creating Endpoints

**With Request and Response:**
```csharp
public record CreateHeroRequest(string Name, string Alias, IEnumerable<PowerDto> Powers);
public record CreateHeroResponse(Guid Id);

public class CreateHeroEndpoint(ApplicationDbContext dbContext)
    : Endpoint<CreateHeroRequest, CreateHeroResponse>
{
    public override void Configure()
    {
        Post("/");
        Group<HeroesGroup>();
        Description(x => x.WithName("CreateHero"));
    }

    public override async Task HandleAsync(CreateHeroRequest req, CancellationToken ct)
    {
        // Business logic here
        await dbContext.SaveChangesAsync(ct);
        await Send.OkAsync(new CreateHeroResponse(id), ct);
    }
}
```

**Without Request (GET all):**
```csharp
public class GetAllHeroesEndpoint(ApplicationDbContext dbContext) 
    : EndpointWithoutRequest<GetAllHeroesResponse>
{
    public override void Configure()
    {
        Get("/");
        Group<HeroesGroup>();
        Description(x => x.WithName("GetAllHeroes"));
    }

    public async override Task HandleAsync(CancellationToken ct)
    {
        // Query logic here
        await Send.OkAsync(new GetAllHeroesResponse(heroes), ct);
    }
}
```

#### Endpoint Groups
Define a `Group` class per feature to configure common settings and routing:
```csharp
public class HeroesGroup : Group
{
    public HeroesGroup()
    {
        Configure("heroes", ep => ep.Description(x => x.ProducesProblemDetails(500)));
    }
}
```
- The prefix (e.g., "heroes") becomes `/api/heroes` and is used as the OpenAPI tag
- All endpoints using `Group<HeroesGroup>()` inherit this configuration

#### Validation
Create validators using FluentValidation patterns:
```csharp
public class CreateHeroRequestValidator : Validator<CreateHeroRequest>
{
    public CreateHeroRequestValidator()
    {
        RuleFor(v => v.Name).NotEmpty();
        RuleFor(v => v.Alias).NotEmpty();
    }
}
```
- Validation runs automatically before `HandleAsync()`
- Failed validation returns 400 Bad Request with error details

#### OpenAPI Documentation
Use `Summary<TEndpoint>` classes for rich API documentation:
```csharp
public class CreateHeroSummary : Summary<CreateHeroEndpoint>
{
    public CreateHeroSummary()
    {
        Summary = "Create a new hero";
        Description = "Creates a new hero with the specified name, alias, and powers.";
        ExampleRequest = new CreateHeroRequest(...);
        Response(200, "Hero created successfully", example: new CreateHeroResponse(...));
    }
}
```

#### Response Handling
FastEndpoints provides `Send` helper for responses:
- `await Send.OkAsync(response, ct)` - 200 OK
- `await Send.CreatedAsync(url, response, ct)` - 201 Created
- `await Send.NoContentAsync(ct)` - 204 No Content
- `await Send.NotFoundAsync(ct)` - 404 Not Found

### Error Handling
- FastEndpoints provides built-in response helpers: `Send.NotFoundAsync()`, `Send.ForbiddenAsync()`, etc.
- Use `ThrowError()` in endpoints to return validation errors with custom messages
- Global exception handling catches unhandled exceptions and returns appropriate HTTP responses
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
- FastEndpoints automatically discovers and registers all endpoints at startup
- Use `Group<TGroup>()` in endpoint `Configure()` to assign to a route group
- Define a `Group` class per feature to set the route prefix (e.g., `HeroesGroup` configures `"heroes"` → `/api/heroes`)
- Example: `Group<HeroesGroup>()` in an endpoint routes to `/api/heroes/{endpoint-route}`

### Global Usings (`GlobalUsings.cs`)
- `EntityFrameworkCore`, `FluentValidation`, `ErrorOr`, `Vogen`, `Ardalis.Specification` pre-imported
- `SSW.VerticalSliceArchitecture.Common.Features` and `.Common.Persistence` available globally

### Build Configuration
- `Directory.Build.props`: Treats warnings as errors in Release builds, enforces code style
- `Directory.Packages.props`: Central package management - update versions here
- Release builds enforce `TreatWarningsAsErrors` and `EnforceCodeStyleInBuild`

### FastEndpoints Pipeline
- **Validation**: Automatically runs FluentValidation validators before `HandleAsync()` - returns 400 Bad Request on failure
- **Exception Handling**: Global exception handler catches unhandled exceptions and returns appropriate error responses
- **Performance Logging**: Built-in logging for slow requests

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
2. Not creating `Summary<TEndpoint>` classes → missing OpenAPI documentation and examples
3. Forgetting to call `Group<TGroup>()` in endpoint `Configure()` → endpoint not properly grouped/routed
4. Not using `await Send.OkAsync()` or similar response methods → no HTTP response sent
5. Forgetting to add `return` after `await Send.OkAsync()` (or similar)
6. Loading aggregates without specifications → missing related entities or inconsistent query patterns

## What's NOT Included
- **Authentication/Authorization**: This is a template - implement auth as needed for your use case
- **Feature-specific service registration**: Most features don't need custom DI - use `IFeature.ConfigureServices()` only when required

# CLI Task Documentation

When creating documentation files (MD files) during CLI tasks, follow these guidelines to avoid unnecessary documentation noise:

### When to Create New Documentation

**DO create new documentation for**:
- Significant architectural changes or new features
- Major refactorings that affect multiple modules
- New patterns or conventions being established
- Implementation guides that will be referenced by team members or future contributors
- Complex changes that need detailed explanation for future reference

**DO NOT create new documentation for**:
- Minor bug fixes or corrections
- Small adjustments to existing code
- Clarifications or improvements to existing implementations
- Changes that can be adequately explained in commit messages

**When unsure**: Ask if documentation should be created before writing it. It's better to update existing documentation than create redundant files.

### Documentation File Naming Format
All documentation files created during CLI tasks should be saved to `docs/cli-tasks/` with the following format:

```
yyyyMMdd-II-XX-description.md
```

Where:
- `yyyyMMdd` = Current date (e.g., 20251002)
- `II` = Author's initials from git config (e.g., GB for Gordon Beeming)
- `XX` = Sequential number starting at 01 for the day (01, 02, 03, etc.)
- `description` = Kebab-case description of the task/document

### Examples
- `20251002-GB-01-graceful-row-failure-implementation-summary.md`
- `20251002-GB-02-graceful-row-failure-refactoring-guide.md`
- `20251002-GB-03-graceful-row-failure-changes-summary.md`

### Process
1. **Determine if documentation is needed** - Is this a significant change?
2. Get current date: `date +%Y%m%d`
3. Get author initials from git config: `git config user.name`
4. Check existing files in `docs/cli-tasks/` for today's date to determine next sequence number
5. **Check if existing documentation should be updated instead** of creating new
6. Create file with proper naming format only if genuinely needed
7. If multiple related documents, use sequential numbers to maintain order

### Updating Existing Documentation

Prefer updating existing documentation when:
- The change is related to a recent task documented today
- It's a bug fix or improvement to something recently implemented
- It adds clarification or correction to existing docs
- The change is minor and fits within the scope of existing documentation

### Purpose
This approach:
- Reduces documentation noise and clutter
- Keeps related information together
- Makes documentation easier to navigate and maintain
- Ensures only significant changes are documented separately
- Maintains high signal-to-noise ratio in documentation

## Working Directory and File Management

### Repository Boundaries
All work, including temporary files, must be done within the repository boundaries:

**DO**:
- Create temporary files/directories within the repository root
- Use `/tmp/` directory at repository root for temporary work files
- Add temporary directories to `.gitignore` if they shouldn't be committed
- Clean up temporary files after completing tasks

**DO NOT**:
- Create files outside the repository directory
- Work in system temp directories or home directory
- Leave temporary files scattered throughout the repository

### Temporary Files
- Use `/tmp/` at the repository root for scratch work
- This directory is already in `.gitignore`
- Always clean up temporary files when done
- Document any temporary files that need to persist

### Purpose
This approach:
- Keeps all work contained within the project
- Prevents pollution of system directories
- Makes cleanup easier and more predictable
- Ensures proper git ignore handling
