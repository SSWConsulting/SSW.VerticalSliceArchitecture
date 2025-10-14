using FastEndpoints;
using SSW.VerticalSliceArchitecture.Common.Domain.Teams;

namespace SSW.VerticalSliceArchitecture.Features.Teams.Endpoints;

public record CreateTeamRequest(string Name);

public class CreateTeamFastEndpoint(ApplicationDbContext dbContext) 
    : Endpoint<CreateTeamRequest>
{
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

        await dbContext.Teams.AddAsync(team, ct);
        await dbContext.SaveChangesAsync(ct);

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

public class CreateTeamSummary : Summary<CreateTeamFastEndpoint>
{
    public CreateTeamSummary()
    {
        Summary = "Create a new team";
        Description = "Creates a new superhero team with the specified name. Heroes can be added to the team later.";
        
        ExampleRequest = new CreateTeamRequest("Avengers");
        
        Response(204, "Team created successfully");
        Response(400, "Invalid request - validation failed");
        Response(500, "Internal server error");
    }
}