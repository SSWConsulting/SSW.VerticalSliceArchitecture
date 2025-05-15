using Ardalis.Specification.EntityFrameworkCore;
using MediatR;
using System.Text.Json.Serialization;
using VerticalSliceArchitectureTemplate.Common.Domain.Teams;
using VerticalSliceArchitectureTemplate.Common.Extensions;

namespace VerticalSliceArchitectureTemplate.Features.Teams.Commands;

public static class CompleteMissionCommand
{
    public record Request : IRequest<ErrorOr<Success>>
    {
        [JsonIgnore]
        public Guid TeamId { get; set; }
    }
    
    public class Endpoint : IEndpoint
    {
        public static void MapEndpoint(IEndpointRouteBuilder endpoints)
        {
            endpoints
                .MapApiGroup(TeamsFeature.FeatureName)
                .MapPost("/{teamId:guid}/complete-mission",
                    async (ISender sender, Guid teamId, Request request, CancellationToken ct) =>
                    {
                        request.TeamId = teamId;
                        var result = await sender.Send(request, ct);
                        return result.Match(TypedResults.Ok, CustomResult.Problem);
                    })
                .WithName("CompleteMission")
                .ProducesPost();
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
        : IRequestHandler<Request, ErrorOr<Success>>
    {
        public async Task<ErrorOr<Success>> Handle(Request request, CancellationToken cancellationToken)
        {
            var teamId = TeamId.From(request.TeamId);
            var team = dbContext.Teams
                .WithSpecification(new TeamByIdSpec(teamId))
                .FirstOrDefault();

            if (team is null)
                return TeamErrors.NotFound;

            var error = team.CompleteCurrentMission();
            if (error.IsError)
                return error;

            await dbContext.SaveChangesAsync(cancellationToken);

            return new Success();
        }
    }
}