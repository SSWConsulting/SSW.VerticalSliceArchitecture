using Ardalis.Specification.EntityFrameworkCore;
using MediatR;
using System.Text.Json.Serialization;
using VerticalSliceArchitectureTemplate.Common.Domain.Heroes;
using VerticalSliceArchitectureTemplate.Common.Domain.Teams;
using VerticalSliceArchitectureTemplate.Common.Extensions;

namespace VerticalSliceArchitectureTemplate.Features.Teams.Commands;

public static class AddHeroToTeamCommand
{
    public record Request : IRequest<ErrorOr<Success>>
    {
        [JsonIgnore] public Guid TeamId { get; set; }
        [JsonIgnore] public Guid HeroId { get; set; }
    }
    
    public class Endpoint : IEndpoint
    {
        public static void MapEndpoint(IEndpointRouteBuilder endpoints)
        {
            endpoints
                .MapApiGroup(TeamsFeature.FeatureName)
                .MapPost("/{teamId:guid}/heroes/{heroId:guid}",
                    async (
                        ISender sender,
                        Guid teamId,
                        Guid heroId,
                        Request request,
                        CancellationToken ct) =>
                    {
                        request.TeamId = teamId;
                        request.HeroId = heroId;
                        var result = await sender.Send(request, ct);
                        return result.Match(_ => TypedResults.Created(), CustomResult.Problem);
                    })
                .WithName("AddHeroToTeam")
                .ProducesPost();
        }
    }
    
    internal sealed class Validator : AbstractValidator<Request>
    {
        public Validator()
        {
            RuleFor(v => v.HeroId)
                .NotEmpty();

            RuleFor(v => v.TeamId)
                .NotEmpty();
        }
    }
    
    internal sealed class Handler(AppDbContext dbContext)
        : IRequestHandler<Request, ErrorOr<Success>>
    {
        public async Task<ErrorOr<Success>> Handle(Request request, CancellationToken cancellationToken)
        {
            var teamId = TeamId.From(request.TeamId);
            var heroId = HeroId.From(request.HeroId);

            var team = dbContext.Teams
                .WithSpecification(new TeamByIdSpec(teamId))
                .FirstOrDefault();

            if (team is null)
                return TeamErrors.NotFound;

            var hero = dbContext.Heroes
                .WithSpecification(new HeroByIdSpec(heroId))
                .FirstOrDefault();

            if (hero is null)
                return HeroErrors.NotFound;

            team.AddHero(hero);
            await dbContext.SaveChangesAsync(cancellationToken);

            return new Success();
        }
    }
}