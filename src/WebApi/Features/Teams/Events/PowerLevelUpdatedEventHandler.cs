using Ardalis.Specification.EntityFrameworkCore;
using SSW.VerticalSliceArchitecture.Common.Domain.Base.EventualConsistency;
using SSW.VerticalSliceArchitecture.Common.Domain.Heroes;
using SSW.VerticalSliceArchitecture.Common.Domain.Teams;

namespace SSW.VerticalSliceArchitecture.Features.Teams.Events;

public class PowerLevelUpdatedEventHandler(
    IServiceScopeFactory scopeFactory,
    ILogger<PowerLevelUpdatedEventHandler> logger)
    : IEventHandler<PowerLevelUpdatedEvent>
{
    public async Task HandleAsync(PowerLevelUpdatedEvent eventModel, CancellationToken ct)
    {
        logger.LogInformation("PowerLevelUpdatedEventHandler: {HeroName} power updated to {PowerLevel}",
        eventModel.Hero.Name, eventModel.Hero.PowerLevel);

        using var scope = scopeFactory.CreateScope();
        var dbContext = scope.Resolve<ApplicationDbContext>();


        var hero = await dbContext.Heroes.FirstAsync(h => h.Id == eventModel.Hero.Id,
            cancellationToken: ct);

        if (hero.TeamId is null)
        {
            logger.LogInformation("Hero {HeroName} is not on a team - nothing to do", eventModel.Hero.Name);
            return;
        }

        var team = dbContext.Teams
            .WithSpecification(new TeamByIdSpec(hero.TeamId.Value))
            .FirstOrDefault();

        if (team is null)
            throw new EventualConsistencyException(PowerLevelUpdatedEvent.TeamNotFound);

        team.ReCalculatePowerLevel();
        await dbContext.SaveChangesAsync(ct);
    }
}