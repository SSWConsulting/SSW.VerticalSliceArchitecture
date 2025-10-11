using FastEndpoints;
using SSW.VerticalSliceArchitecture.Common.FastEndpoints;

namespace SSW.VerticalSliceArchitecture.Features.Teams.Endpoints;

public record GetAllTeamsTeamDto(Guid Id, string Name);

public class GetAllTeamsFastEndpoint : EndpointBase<IReadOnlyList<GetAllTeamsTeamDto>>
{
    private readonly ApplicationDbContext _dbContext;

    public GetAllTeamsFastEndpoint(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public override void Configure()
    {
        Get("/teams");
        Group<TeamsGroup>();
        AllowAnonymous();
        Description(x => x
            .WithName("GetAllTeamsFast")
            .WithTags("Teams")
            .Produces<IReadOnlyList<GetAllTeamsTeamDto>>(200)
            .ProducesProblemDetails(500));
    }

    public override async Task HandleAsync(EmptyRequest req, CancellationToken ct)
    {
        var teams = await _dbContext.Teams
            .Select(t => new GetAllTeamsTeamDto(t.Id.Value, t.Name))
            .ToListAsync(ct);

        await HttpContext.Response.WriteAsJsonAsync(teams, ct);
    }
}