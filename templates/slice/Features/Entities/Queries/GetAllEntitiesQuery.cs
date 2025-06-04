using MediatR;
using SSW.VerticalSliceArchitecture.Host.Extensions;

namespace SSW.VerticalSliceArchitecture.Features.Entities.Queries;

public static class GetAllEntitiesQuery
{
    public record EntityNameDto(Guid Id, string Name);
    
    public record Request : IRequest<ErrorOr<IReadOnlyList<EntityNameDto>>>;
    
    public class Endpoint : IEndpoint
    {
        public static void MapEndpoint(IEndpointRouteBuilder endpoints)
        {
            endpoints
                .MapApiGroup(EntitiesFeature.FeatureName)
                .MapGet("/",
                    async (ISender sender, CancellationToken cancellationToken) =>
                    {
                        var request = new Request();
                        var result = await sender.Send(request, cancellationToken);
                        return result.Match(TypedResults.Ok, CustomResult.Problem);
                    })
                .WithName("GetAllEntities")
                .ProducesGet<IReadOnlyList<EntityNameDto>>();
        }
    }
    
    public class Validator : AbstractValidator<Request>
    {
        public Validator() {}
    }
    
    internal sealed class Handler : IRequestHandler<Request, ErrorOr<IReadOnlyList<EntityNameDto>>>
    {
        private readonly ApplicationDbContext _dbContext;

        public Handler(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<ErrorOr<IReadOnlyList<EntityNameDto>>> Handle(
            Request request,
            CancellationToken cancellationToken)
        {
            return await _dbContext.Entities
                .Select(h => new EntityNameDto(h.Id.Value, h.Name))
                .ToListAsync(cancellationToken);
        }
    }
}