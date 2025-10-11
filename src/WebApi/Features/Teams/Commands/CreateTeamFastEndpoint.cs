using FastEndpoints;
using SSW.VerticalSliceArchitecture.Common.Domain.Teams;
using SSW.VerticalSliceArchitecture.Common.FastEndpoints;

namespace SSW.VerticalSliceArchitecture.Features.Teams.Commands;

public record CreateTeamRequest(string Name);

public class CreateTeamRequestValidator : Validator<CreateTeamRequest>
{
    public CreateTeamRequestValidator()
    {
        RuleFor(v => v.Name)
            .NotEmpty();
    }
}

public class CreateTeamFastEndpoint : Endpoint<CreateTeamRequest>
{
    private readonly ApplicationDbContext _dbContext;

    public CreateTeamFastEndpoint(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public override void Configure()
    {
        Post("/teams");
        Group<TeamsGroup>();
        Description(x => x
            .WithName("CreateTeamFast")
            .WithTags("Teams")
            .Produces(201)
            .ProducesProblemDetails(400)
            .ProducesProblemDetails(500));
    }

    public override async Task HandleAsync(CreateTeamRequest req, CancellationToken ct)
    {
        var team = Team.Create(req.Name);

        await _dbContext.Teams.AddAsync(team, ct);
        await _dbContext.SaveChangesAsync(ct);

        HttpContext.Response.StatusCode = StatusCodes.Status201Created;
    }
}
