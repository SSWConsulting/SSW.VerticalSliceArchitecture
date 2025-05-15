using MediatR;
using VerticalSliceArchitectureTemplate.Common.Domain.Teams;
using VerticalSliceArchitectureTemplate.Common.Extensions;
using VerticalSliceArchitectureTemplate.Features.Heroes;

namespace VerticalSliceArchitectureTemplate.Features.Teams.Queries;

public static class GetTeamQuery
{
    public record TeamDto(Guid Id, string Name, IEnumerable<HeroDto> Heroes);
    public record HeroDto(Guid Id, string Name, string Alias, int PowerLevel, IEnumerable<HeroPowerDto> Powers);
    public record HeroPowerDto(string Name, int PowerLevel);
    
    public record Request(Guid TeamId) : IRequest<ErrorOr<TeamDto>>;
    
    public class Endpoint : IEndpoint
    {
        public static void MapEndpoint(IEndpointRouteBuilder endpoints)
        {
            endpoints
                .MapApiGroup(TeamsFeature.FeatureName)
                .MapGet("/{teamId:guid}",
                    async (ISender sender, Guid teamId, CancellationToken ct) =>
                    {
                        var query = new Request(teamId);
                        var result = await sender.Send(query, ct);
                        return result.Match(TypedResults.Ok, CustomResult.Problem);
                    })
                .WithName("GetTeam")
                .ProducesGet<TeamDto>();
        }
    }
    
    internal sealed class Validator : AbstractValidator<Request>
    {
        public Validator()
        {
            RuleFor(v => v.TeamId)
                .NotEmpty();
        }
    }
    
    internal sealed class Handler(ApplicationDbContext dbContext)
        : IRequestHandler<Request, ErrorOr<TeamDto>>
    {
        public async Task<ErrorOr<TeamDto>> Handle(
            Request request,
            CancellationToken cancellationToken)
        {
            var teamId = TeamId.From(request.TeamId);

            var team = await dbContext.Teams
                .Where(t => t.Id == teamId)
                .Select(t => new TeamDto(
                    t.Id.Value,
                    t.Name,
                    t.Heroes.Select(
                        h => new HeroDto(h.Id.Value, h.Name, h.Alias, h.PowerLevel, h.Powers.Select(
                                p => new HeroPowerDto(
                                    p.Name,
                                    p.PowerLevel
                                )
                            )
                        )
                    )))
                .FirstOrDefaultAsync(cancellationToken);

            if (team is null)
                return TeamErrors.NotFound;

            return team;
        }
    }
}