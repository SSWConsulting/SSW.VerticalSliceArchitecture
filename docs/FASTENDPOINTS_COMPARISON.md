# FastEndpoints vs Minimal API - Side-by-Side Comparison

This document shows the differences between the Minimal API (MediatR) and FastEndpoints implementations for the Heroes feature.

## GetAllHeroes - Query Implementation

### Minimal API (MediatR)
**File:** `GetAllHeroesQuery.cs`

```csharp
public static class GetAllHeroesQuery
{
    public record HeroDto(Guid Id, string Name, string Alias, int PowerLevel, IReadOnlyList<HeroPowerDto> Powers);
    public record HeroPowerDto(string Name, int PowerLevel);
    
    public record Request : IRequest<ErrorOr<IReadOnlyList<HeroDto>>>;
    
    public class Endpoint : IEndpoint
    {
        public static void MapEndpoint(IEndpointRouteBuilder endpoints)
        {
            endpoints
                .MapApiGroup(HeroesFeature.FeatureName)
                .MapGet("/",
                    async (ISender sender, CancellationToken cancellationToken) =>
                    {
                        var request = new Request();
                        var result = await sender.Send(request, cancellationToken);
                        return result.Match(TypedResults.Ok, CustomResult.Problem);
                    })
                .WithName("GetAllHeroes")
                .ProducesGet<IReadOnlyList<HeroDto>>();
        }
    }
    
    public class Validator : AbstractValidator<Request>
    {
        public Validator() {}
    }
    
    internal sealed class Handler : IRequestHandler<Request, ErrorOr<IReadOnlyList<HeroDto>>>
    {
        private readonly ApplicationDbContext _dbContext;

        public Handler(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<ErrorOr<IReadOnlyList<HeroDto>>> Handle(
            Request request,
            CancellationToken cancellationToken)
        {
            return await _dbContext.Heroes
                .Select(h => new HeroDto(
                    h.Id.Value,
                    h.Name,
                    h.Alias,
                    h.PowerLevel,
                    h.Powers.Select(p => new HeroPowerDto(p.Name, p.PowerLevel)).ToList()))
                .ToListAsync(cancellationToken);
        }
    }
}
```

### FastEndpoints
**File:** `GetAllHeroesFastEndpoint.cs`

```csharp
public static class GetAllHeroesFastEndpoint
{
    public record HeroDto(Guid Id, string Name, string Alias, int PowerLevel, IReadOnlyList<HeroPowerDto> Powers);
    public record HeroPowerDto(string Name, int PowerLevel);
    
    public class Endpoint : EndpointBase<IReadOnlyList<HeroDto>>
    {
        private readonly ApplicationDbContext _dbContext;

        public Endpoint(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public override void Configure()
        {
            Get("/heroes");
            Group<HeroesGroup>();
            AllowAnonymous();
            Description(x => x
                .WithName("GetAllHeroesFast")
                .WithTags("Heroes")
                .Produces<IReadOnlyList<HeroDto>>(200)
                .ProducesProblemDetails(500));
        }

        public override async Task HandleAsync(CancellationToken ct)
        {
            var result = await ExecuteAsync(ct);
            
            await SendErrorOrAsync(result, async heroes =>
            {
                await SendOkAsync(heroes, ct);
            }, ct);
        }

        private async Task<ErrorOr<IReadOnlyList<HeroDto>>> ExecuteAsync(CancellationToken cancellationToken)
        {
            return await _dbContext.Heroes
                .Select(h => new HeroDto(
                    h.Id.Value,
                    h.Name,
                    h.Alias,
                    h.PowerLevel,
                    h.Powers.Select(p => new HeroPowerDto(p.Name, p.PowerLevel)).ToList()))
                .ToListAsync(cancellationToken);
        }
    }
}
```

**Key Differences:**
- ✅ No separate `Handler` class needed
- ✅ No `IRequest` or `IRequestHandler` interfaces
- ✅ Route configured in `Configure()` method
- ✅ Direct database access without MediatR
- ✅ Better OpenAPI documentation support

---

## CreateHero - Command Implementation

### Minimal API (MediatR)
**File:** `CreateHeroCommand.cs`

```csharp
public static class CreateHeroCommand
{
    public record CreateHeroPowerDto(string Name, int PowerLevel);
    
    public record Request(
        string Name,
        string Alias,
        IEnumerable<CreateHeroPowerDto> Powers) : IRequest<ErrorOr<Guid>>;
    
    public class Endpoint : IEndpoint
    {
        public static void MapEndpoint(IEndpointRouteBuilder endpoints)
        {
            endpoints
                .MapApiGroup(HeroesFeature.FeatureName)
                .MapPost("/",
                    async (ISender sender, Request request, CancellationToken ct) =>
                    {
                        var result = await sender.Send(request, ct);
                        return result.Match(_ => TypedResults.Created(), CustomResult.Problem);
                    })
                .WithName("CreateHero")
                .ProducesPost();
        }
    }
    
    internal sealed class Validator : AbstractValidator<Request>
    {
        public Validator()
        {
            RuleFor(v => v.Name).NotEmpty();
            RuleFor(v => v.Alias).NotEmpty();
        }
    }
    
    internal sealed class Handler(ApplicationDbContext dbContext)
        : IRequestHandler<Request, ErrorOr<Guid>>
    {
        public async Task<ErrorOr<Guid>> Handle(Request request, CancellationToken cancellationToken)
        {
            var hero = Hero.Create(request.Name, request.Alias);
            var powers = request.Powers.Select(p => new Power(p.Name, p.PowerLevel));
            hero.UpdatePowers(powers);

            await dbContext.Heroes.AddAsync(hero, cancellationToken);
            await dbContext.SaveChangesAsync(cancellationToken);
            // Events queued automatically by DispatchDomainEventsInterceptor

            return hero.Id.Value;
        }
    }
}
```

### FastEndpoints
**File:** `CreateHeroFastEndpoint.cs`

```csharp
public static class CreateHeroFastEndpoint
{
    public record CreateHeroPowerDto(string Name, int PowerLevel);
    
    public record Request(
        string Name,
        string Alias,
        IEnumerable<CreateHeroPowerDto> Powers);

    public record Response(Guid Id);
    
    public class Endpoint : EndpointBase<Request, Response>
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly IFastEndpointEventPublisher _eventPublisher;

        public Endpoint(ApplicationDbContext dbContext, IFastEndpointEventPublisher eventPublisher)
        {
            _dbContext = dbContext;
            _eventPublisher = eventPublisher;
        }

        public override void Configure()
        {
            Post("/heroes");
            Group<HeroesGroup>();
            Description(x => x
                .WithName("CreateHeroFast")
                .WithTags("Heroes")
                .Produces<Response>(201)
                .ProducesProblemDetails(400)
                .ProducesProblemDetails(500));
        }

        public override async Task HandleAsync(Request req, CancellationToken ct)
        {
            var result = await ExecuteAsync(req, ct);
            
            await SendErrorOrAsync(result, async id =>
            {
                await SendCreatedAtAsync<GetAllHeroesFastEndpoint.Endpoint>(null, new Response(id), cancellation: ct);
            }, ct);
        }

        private async Task<ErrorOr<Guid>> ExecuteAsync(Request request, CancellationToken cancellationToken)
        {
            var hero = Hero.Create(request.Name, request.Alias);
            var powers = request.Powers.Select(p => new Power(p.Name, p.PowerLevel));
            hero.UpdatePowers(powers);

            await _dbContext.Heroes.AddAsync(hero, cancellationToken);
            await _dbContext.SaveChangesAsync(cancellationToken);
            
            // Queue domain events for eventual consistency processing
            var domainEvents = hero.PopDomainEvents();
            foreach (var domainEvent in domainEvents)
            {
                _eventPublisher.QueueDomainEvent(domainEvent);
            }

            return hero.Id.Value;
        }
    }
    
    public class Validator : Validator<Request>
    {
        public Validator()
        {
            RuleFor(v => v.Name).NotEmpty();
            RuleFor(v => v.Alias).NotEmpty();
        }
    }
}
```

**Key Differences:**
- ✅ No MediatR abstractions (`IRequest`, `IRequestHandler`)
- ✅ Manual event queuing (explicit control)
- ✅ Typed response with `Response` record
- ✅ Type-safe route references in `SendCreatedAtAsync`
- ⚠️ Requires manual event queuing vs automatic with interceptor

---

## Behaviors vs Processors

### MediatR Behaviors (Global Pipeline)

**LoggingBehaviour.cs**
```csharp
public class LoggingBehaviour<TRequest>(ILogger<TRequest> logger, ICurrentUserService currentUserService)
    : IRequestPreProcessor<TRequest>
    where TRequest : notnull
{
    public Task Process(TRequest request, CancellationToken cancellationToken)
    {
        var requestName = typeof(TRequest).Name;
        var userId = currentUserService.UserId ?? string.Empty;

        logger.LogInformation("WebApi Request: {Name} {@UserId} {@Request}",
            requestName, userId, request);

        return Task.CompletedTask;
    }
}
```

**PerformanceBehaviour.cs**
```csharp
public class PerformanceBehaviour<TRequest, TResponse>(
    ILogger<TRequest> logger,
    ICurrentUserService currentUserService)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private readonly Stopwatch _timer = new();

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        _timer.Start();
        var response = await next(cancellationToken);
        _timer.Stop();

        var elapsedMilliseconds = _timer.ElapsedMilliseconds;

        if (elapsedMilliseconds > 500)
        {
            var requestName = typeof(TRequest).Name;
            var userId = currentUserService.UserId ?? string.Empty;

            logger.LogWarning(
                "WebApi Long Running Request: {Name} ({ElapsedMilliseconds} milliseconds) {@UserId} {@Request}",
                requestName, elapsedMilliseconds, userId, request);
        }

        return response;
    }
}
```

### FastEndpoints Processors (Global Pipeline)

**LoggingPreProcessor.cs**
```csharp
public class LoggingPreProcessor<TRequest> : IPreProcessor<TRequest>
    where TRequest : notnull
{
    public Task PreProcessAsync(IPreProcessorContext<TRequest> context, CancellationToken ct)
    {
        var logger = context.HttpContext.Resolve<ILogger<TRequest>>();
        var currentUserService = context.HttpContext.Resolve<ICurrentUserService>();
        
        var requestName = typeof(TRequest).Name;
        var userId = currentUserService.UserId ?? string.Empty;

        logger.LogInformation("WebApi Request: {Name} {@UserId} {@Request}",
            requestName, userId, context.Request);

        return Task.CompletedTask;
    }
}
```

**PerformancePreProcessor.cs + PerformancePostProcessor.cs**
```csharp
// Pre-processor starts timing
public class PerformancePreProcessor<TRequest> : IPreProcessor<TRequest>
    where TRequest : notnull
{
    public Task PreProcessAsync(IPreProcessorContext<TRequest> context, CancellationToken ct)
    {
        var stopwatch = Stopwatch.StartNew();
        context.HttpContext.Items["PerformanceStopwatch"] = stopwatch;
        return Task.CompletedTask;
    }
}

// Post-processor logs slow requests
public class PerformancePostProcessor<TRequest, TResponse> : IPostProcessor<TRequest, TResponse>
    where TRequest : notnull
{
    public Task PostProcessAsync(IPostProcessorContext<TRequest, TResponse> context, CancellationToken ct)
    {
        if (context.HttpContext.Items.TryGetValue("PerformanceStopwatch", out var stopwatchObj) 
            && stopwatchObj is Stopwatch stopwatch)
        {
            stopwatch.Stop();
            var elapsedMilliseconds = stopwatch.ElapsedMilliseconds;

            if (elapsedMilliseconds > 500)
            {
                var logger = context.HttpContext.Resolve<ILogger<TRequest>>();
                var currentUserService = context.HttpContext.Resolve<ICurrentUserService>();
                
                var requestName = typeof(TRequest).Name;
                var userId = currentUserService.UserId ?? string.Empty;

                logger.LogWarning(
                    "WebApi Long Running Request: {Name} ({ElapsedMilliseconds} milliseconds) {@UserId} {@Request}",
                    requestName, elapsedMilliseconds, userId, context.Request);
            }
        }

        return Task.CompletedTask;
    }
}
```

**Key Differences:**
- ✅ FastEndpoints uses `HttpContext.Resolve<T>()` for dependency resolution
- ✅ Separate pre/post processors vs combined behavior
- ✅ Access to full HTTP context in processors
- ✅ Similar logging and performance tracking functionality

---

## Event Handling

### MediatR Event Handler
**File:** `PowerLevelUpdatedEventHandler.cs`

```csharp
internal sealed class PowerLevelUpdatedEventHandler(
    ApplicationDbContext dbContext,
    ILogger<PowerLevelUpdatedEventHandler> logger)
    : INotificationHandler<PowerLevelUpdatedEvent>
{
    public async Task Handle(PowerLevelUpdatedEvent notification, CancellationToken cancellationToken)
    {
        logger.LogInformation("PowerLevelUpdatedEventHandler: {HeroName} power updated to {PowerLevel}",
            notification.Hero.Name, notification.Hero.PowerLevel);

        var hero = await dbContext.Heroes.FirstAsync(h => h.Id == notification.Hero.Id,
            cancellationToken: cancellationToken);

        if (hero.TeamId is null)
        {
            logger.LogInformation("Hero {HeroName} is not on a team - nothing to do", notification.Hero.Name);
            return;
        }

        var team = dbContext.Teams
            .WithSpecification(new TeamByIdSpec(hero.TeamId.Value))
            .FirstOrDefault();

        if (team is null)
            throw new EventualConsistencyException(PowerLevelUpdatedEvent.TeamNotFound);

        team.ReCalculatePowerLevel();
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
```

### FastEndpoints Event Handler
**File:** `PowerLevelUpdatedFastEventHandler.cs`

```csharp
public class PowerLevelUpdatedFastEventHandler : IEventHandler<PowerLevelUpdatedEvent>
{
    public async Task HandleAsync(PowerLevelUpdatedEvent eventModel, CancellationToken ct)
    {
        var dbContext = eventModel.Resolve<ApplicationDbContext>();
        var logger = eventModel.Resolve<ILogger<PowerLevelUpdatedFastEventHandler>>();
        
        logger.LogInformation("PowerLevelUpdatedFastEventHandler: {HeroName} power updated to {PowerLevel}",
            eventModel.Hero.Name, eventModel.Hero.PowerLevel);

        var hero = await dbContext.Heroes.FirstAsync(h => h.Id == eventModel.Hero.Id,
            cancellationToken: ct);

        if (hero.TeamId is null)
        {
            logger.LogInformation("Hero {HeroName} is not on a team - nothing to do", eventModel.Hero.Name);
            return;
        }

        var team = dbContext.Teams
            .WithSpecification(new TeamByIdSpec(hero.TeamId.Value))
            .FirstOrDefault();

        if (team is null)
            throw new EventualConsistencyException(PowerLevelUpdatedEvent.TeamNotFound);

        team.ReCalculatePowerLevel();
        await dbContext.SaveChangesAsync(ct);
    }
}
```

**Key Differences:**
- ✅ FastEndpoints uses `eventModel.Resolve<T>()` for dependency resolution
- ✅ Constructor injection vs method-based resolution
- ✅ Same business logic in both implementations
- ✅ Both called by `EventualConsistencyMiddleware`

---

## Summary

| Aspect | Minimal API (MediatR) | FastEndpoints |
|--------|----------------------|---------------|
| **Setup Complexity** | More boilerplate | Less boilerplate |
| **Performance** | Reflection-based handler discovery | Compile-time binding |
| **Type Safety** | Strong at compile time | Even stronger (routes, etc.) |
| **Co-location** | Endpoint, Handler, Validator separate | All in one file |
| **Testing** | Requires MediatR infrastructure | Direct endpoint testing |
| **Event Publishing** | Automatic via interceptor | Manual queuing required |
| **Learning Curve** | Higher (MediatR, CQRS concepts) | Lower (direct HTTP semantics) |
| **Flexibility** | High (mediator pattern) | High (direct control) |
| **OpenAPI Support** | Good | Excellent |
| **Dependency Injection** | Constructor injection | Constructor + Resolve |

Both patterns work well and can coexist in the same application. Choose based on your team's preferences and project requirements.
