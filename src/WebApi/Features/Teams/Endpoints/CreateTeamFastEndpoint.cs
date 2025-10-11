using FastEndpoints;
using SSW.VerticalSliceArchitecture.Common.Domain.Teams;

namespace SSW.VerticalSliceArchitecture.Features.Teams.Endpoints;

public record CreateTeamRequest(string Name);

public class CreateTeamFastEndpoint : Endpoint<CreateTeamRequest>
{
    private readonly ApplicationDbContext _dbContext;

    public CreateTeamFastEndpoint(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public override void Configure()
    {
        Post("/");
        Group<TeamsGroup>();
        Description(x => x
            .WithName("CreateTeamFast"));
    }

    public override async Task HandleAsync(CreateTeamRequest req, CancellationToken ct)
    {
        var team = Team.Create(req.Name);

        await _dbContext.Teams.AddAsync(team, ct);
        await _dbContext.SaveChangesAsync(ct);

        await Send.NoContentAsync(ct);
    }
}

public class CreateTeamRequestValidator : Validator<CreateTeamRequest>
{
    public CreateTeamRequestValidator()
    {
        RuleFor(v => v.Name)
            .NotEmpty();
    }
}