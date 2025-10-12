using Ardalis.Specification.EntityFrameworkCore;
using FastEndpoints;
using SSW.VerticalSliceArchitecture.Common.Domain.Teams;
using IEndpoint = FastEndpoints.IEndpoint;

namespace SSW.VerticalSliceArchitecture.Features.Teams.Endpoints;

public record CompleteMissionRequest(Guid TeamId);

public class CompleteMissionFastEndpoint(ApplicationDbContext dbContext) 
    : Endpoint<CompleteMissionRequest>
{
    public override void Configure()
    {
        Post("/{teamId}/complete-mission");
        Group<TeamsGroup>();
        Description(x => x
            .WithName("CompleteMissionFast")
            .Produces(404));
    }

    public override async Task HandleAsync(CompleteMissionRequest req, CancellationToken ct)
    {
        var teamId = TeamId.From(req.TeamId);
        var team = dbContext.Teams
            .WithSpecification(new TeamByIdSpec(teamId))
            .FirstOrDefault();

        if (team is null)
        {
            await Send.NotFoundAsync(ct);
            return;
        }

        var result = team.CompleteCurrentMission();
        if (result.IsError)
        {
            result.Errors.ForEach(e => AddError(e.Description, e.Code));
            await Send.ErrorsAsync(cancellation: ct);
            return;
        }

        await dbContext.SaveChangesAsync(ct);

        await Send.NoContentAsync(ct);
    }
}

public class CompleteMissionRequestValidator : Validator<CompleteMissionRequest>
{
    public CompleteMissionRequestValidator()
    {
        RuleFor(v => v.TeamId)
            .NotEmpty();
    }
}

// public static class FastEndpointsExtensions
// {
//     public static async Task SendResultAsync<TRequest>(this Endpoint<TRequest> endpoint, ErrorOr<Success> result, CancellationToken ct = default)
//     {
//         if (result.IsError)
//         {
//             var error = result.Errors.First();
//             endpoint.AddError(error.Description, error.Code);
//             await endpoint.Send.ErrorsAsync(cancellation: ct);
//             return;
//         }
//     }
// }