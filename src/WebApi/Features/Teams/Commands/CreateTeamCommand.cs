using MediatR;
using SSW.VerticalSliceArchitecture.Common.Domain.Teams;
using SSW.VerticalSliceArchitecture.Common.Extensions;

namespace SSW.VerticalSliceArchitecture.Features.Teams.Commands;

public static class CreateTeamCommand
{
    public record Request(string Name) : IRequest<ErrorOr<Success>>;
    
    public class Endpoint : IEndpoint
    {
        public static void MapEndpoint(IEndpointRouteBuilder endpoints)
        {
            endpoints
                .MapApiGroup(TeamsFeature.FeatureName)
                .MapPost("/",
                    async (ISender sender, Request command, CancellationToken ct) =>
                    {
                        var result = await sender.Send(command, ct);
                        return result.Match(_ => TypedResults.Created(), CustomResult.Problem);
                    })
                .WithName("CreateTeam")
                .ProducesPost();
        }
    }
    
    internal sealed class Validator : AbstractValidator<Request>
    {
        public Validator()
        {
            RuleFor(v => v.Name)
                .NotEmpty();
        }
    }
    
    internal sealed class Handler(ApplicationDbContext dbContext)
        : IRequestHandler<Request, ErrorOr<Success>>
    {
        public async Task<ErrorOr<Success>> Handle(Request request, CancellationToken cancellationToken)
        {
            var team = Team.Create(request.Name);

            await dbContext.Teams.AddAsync(team, cancellationToken);
            await dbContext.SaveChangesAsync(cancellationToken);

            return new Success();
        }
    }
}