using Ardalis.Specification.EntityFrameworkCore;
using MediatR;
using SSW.VerticalSliceArchitecture.Common.Domain.Teams;
using SSW.VerticalSliceArchitecture.Host.Extensions;
using System.Text.Json.Serialization;

namespace SSW.VerticalSliceArchitecture.Features.Teams.Commands;

public static class ExecuteMissionCommand
{
    public record Request(string Description) : IRequest<ErrorOr<Success>>
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
                .MapPost("/{teamId:guid}/execute-mission",
                    async (ISender sender, Guid teamId, Request command, CancellationToken ct) =>
                    {
                        command.TeamId = teamId;
                        var result = await sender.Send(command, ct);
                        return result.Match(TypedResults.Ok, CustomResult.Problem);
                    })
                .WithName("ExecuteMission")
                .ProducesPost();
        }
    }
    
    internal sealed class Validator : AbstractValidator<Request>
    {
        public Validator()
        {
            RuleFor(v => v.TeamId)
                .NotEmpty();

            RuleFor(v => v.Description)
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

            var result = team.ExecuteMission(request.Description);
            if (result.IsError)
                return result;

            await dbContext.SaveChangesAsync(cancellationToken);

            return new Success();
        }
    }
}