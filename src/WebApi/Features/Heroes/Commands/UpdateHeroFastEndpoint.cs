using FastEndpoints;
using SSW.VerticalSliceArchitecture.Common.Domain.Heroes;
using SSW.VerticalSliceArchitecture.Common.FastEndpoints;

namespace SSW.VerticalSliceArchitecture.Features.Heroes.Commands;

public static class UpdateHeroFastEndpoint
{
    public record UpdateHeroPowerDto(string Name, int PowerLevel);
    
    public record Request(
        string Name,
        string Alias,
        IEnumerable<UpdateHeroPowerDto> Powers)
    {
        public Guid HeroId { get; set; }
    }

    public class Endpoint : Endpoint<Request>
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
            Put("/heroes/{heroId}");
            Group<HeroesGroup>();
            Description(x => x
                .WithName("UpdateHeroFast")
                .WithTags("Heroes")
                .Produces(204)
                .ProducesProblemDetails(400)
                .ProducesProblemDetails(404)
                .ProducesProblemDetails(500));
        }

        public override async Task HandleAsync(Request req, CancellationToken ct)
        {
            req.HeroId = Route<Guid>("heroId");
            
            var heroId = HeroId.From(req.HeroId);
            var hero = await _dbContext.Heroes
                .Include(h => h.Powers)
                .FirstOrDefaultAsync(h => h.Id == heroId, ct);

            if (hero is null)
            {
                HttpContext.Response.StatusCode = StatusCodes.Status404NotFound;
                await HttpContext.Response.WriteAsJsonAsync(new
                {
                    errors = new[] { new { HeroErrors.NotFound.Code, HeroErrors.NotFound.Description } }
                }, ct);
                return;
            }

            hero.Name = req.Name;
            hero.Alias = req.Alias;
            var powers = req.Powers.Select(p => new Power(p.Name, p.PowerLevel));
            hero.UpdatePowers(powers);

            await _dbContext.SaveChangesAsync(ct);
            
            // Queue domain events for eventual consistency processing
            // These will be processed by EventualConsistencyMiddleware after response is sent
            var domainEvents = hero.PopDomainEvents();
            foreach (var domainEvent in domainEvents)
            {
                _eventPublisher.QueueDomainEvent(domainEvent);
            }

            HttpContext.Response.StatusCode = StatusCodes.Status204NoContent;
        }
    }
    
    public class Validator : Validator<Request>
    {
        public Validator()
        {
            RuleFor(v => v.HeroId)
                .NotEmpty();

            RuleFor(v => v.Name)
                .NotEmpty();

            RuleFor(v => v.Alias)
                .NotEmpty();
        }
    }
}
