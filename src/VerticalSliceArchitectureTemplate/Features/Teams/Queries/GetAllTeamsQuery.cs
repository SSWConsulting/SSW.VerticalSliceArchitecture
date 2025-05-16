using MediatR;
using VerticalSliceArchitectureTemplate.Common.Extensions;

namespace VerticalSliceArchitectureTemplate.Features.Teams.Queries;

public static class GetAllTeamsQuery
{
    public record TeamDto(Guid Id, string Name);

    public record Request : IRequest<ErrorOr<IReadOnlyList<TeamDto>>>;
    
    public class Endpoint : IEndpoint
    {
        public static void MapEndpoint(IEndpointRouteBuilder endpoints)
        {
            endpoints
                .MapApiGroup(TeamsFeature.FeatureName)
                .MapGet("/",
                    async (ISender sender, CancellationToken ct) =>
                    {
                        var request = new Request();
                        var result = await sender.Send(request, ct);
                        return result.Match(TypedResults.Ok, CustomResult.Problem);
                    })
                .WithName("GetAllTeams")
                .ProducesGet<IReadOnlyList<TeamDto>>();
        }
    }
    
    internal sealed class Validator : AbstractValidator<Request>
    {
        public Validator() { }
    }
    
    internal sealed class Handler(ApplicationDbContext dbContext)
        : IRequestHandler<Request, ErrorOr<IReadOnlyList<TeamDto>>>
    {
        public async Task<ErrorOr<IReadOnlyList<TeamDto>>> Handle(
            Request request,
            CancellationToken cancellationToken)
        {
            return await dbContext.Teams
                .Select(t => new TeamDto(t.Id.Value, t.Name))
                .ToListAsync(cancellationToken);
        }
    }
}