using Ardalis.Specification.EntityFrameworkCore;
using MediatR;
using VerticalSliceArchitectureTemplate.Common.Domain.Base.EventualConsistency;
using VerticalSliceArchitectureTemplate.Common.Domain.Heroes;
using VerticalSliceArchitectureTemplate.Common.Domain.Teams;

namespace VerticalSliceArchitectureTemplate.Features.Heroes.Events;

internal sealed class PowerLevelUpdatedEventHandler(
    AppDbContext dbContext,
    ILogger<PowerLevelUpdatedEventHandler> logger)
    : INotificationHandler<PowerLevelUpdatedEvent>
{
    public async Task Handle(PowerLevelUpdatedEvent notification, CancellationToken cancellationToken)
    {
        logger.LogInformation("PowerLevelUpdatedEventHandler: {HeroName} power updated to {PowerLevel}",
            notification.Hero.Name, notification.Hero.PowerLevel);

        var hero = await dbContext.Heroes.FirstAsync(h => h.Id == notification.Hero.Id,
            cancellationToken: cancellationToken);

        if (hero.TeamId is null)
        {
            logger.LogInformation("Hero {HeroName} is not on a team - nothing to do", notification.Hero.Name);
            return;
        }

        var team = dbContext.Teams
            .WithSpecification(new TeamByIdSpec(hero.TeamId.Value))
            .FirstOrDefault();

        if (team is null)
            throw new EventualConsistencyException(PowerLevelUpdatedEvent.TeamNotFound);

        team.ReCalculatePowerLevel();
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}