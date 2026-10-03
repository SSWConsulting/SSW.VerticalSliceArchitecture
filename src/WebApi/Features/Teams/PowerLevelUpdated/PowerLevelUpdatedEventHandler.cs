using Ardalis.Specification.EntityFrameworkCore;
using SSW.VerticalSliceArchitecture.Common.Events;
using SSW.VerticalSliceArchitecture.Domain.Base.EventualConsistency;
using SSW.VerticalSliceArchitecture.Domain.Heroes;
using SSW.VerticalSliceArchitecture.Domain.Teams;

namespace SSW.VerticalSliceArchitecture.Features.Teams.PowerLevelUpdated;

public class PowerLevelUpdatedEventHandler(
    ApplicationDbContext dbContext,
    ILogger<PowerLevelUpdatedEventHandler> logger)
    : IDomainEventHandler<PowerLevelUpdatedEvent>
{
    public async Task HandleAsync(PowerLevelUpdatedEvent domainEvent, CancellationToken cancellationToken)
    {
        ThrowIfNull(domainEvent);

        logger.PowerLevelUpdated(domainEvent.Hero.Name, domainEvent.Hero.PowerLevel);

        // The dispatcher runs each handler in its own scope, so this is a different DbContext from
        // the one whose SaveChanges raised the event.
        var hero = await dbContext.Heroes.FirstAsync(h => h.Id == domainEvent.Hero.Id, cancellationToken);

        if (hero.TeamId is null)
        {
            logger.HeroNotOnTeam(domainEvent.Hero.Name);
            return;
        }

        var team = await dbContext.Teams
            .WithSpecification(TeamSpec.ById(hero.TeamId.Value))
            .FirstOrDefaultAsync(cancellationToken);

        if (team is null)
            throw new EventualConsistencyException(PowerLevelUpdatedEvent.TeamNotFound);

        team.ReCalculatePowerLevel();
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}

// Compile-time logging: the source generator emits a level-checked, allocation-free
// method, which also satisfies CA1873 (the generated call is not an ILogger.Log* shape).
internal static partial class PowerLevelUpdatedEventHandlerLog
{
    [LoggerMessage(LogLevel.Information,
        "PowerLevelUpdatedEventHandler: {HeroName} power updated to {PowerLevel}")]
    public static partial void PowerLevelUpdated(this ILogger logger, string heroName, int powerLevel);

    [LoggerMessage(LogLevel.Information, "Hero {HeroName} is not on a team - nothing to do")]
    public static partial void HeroNotOnTeam(this ILogger logger, string heroName);
}
