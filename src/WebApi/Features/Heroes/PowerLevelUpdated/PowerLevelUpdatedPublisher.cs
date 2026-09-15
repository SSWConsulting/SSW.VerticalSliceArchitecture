using HotChocolate.Subscriptions;
using SSW.VerticalSliceArchitecture.Common.Events;
using SSW.VerticalSliceArchitecture.Domain.Heroes;

namespace SSW.VerticalSliceArchitecture.Features.Heroes.PowerLevelUpdated;

/// <summary>
/// Turns the domain event into a GraphQL subscription message.
/// </summary>
/// <remarks>
/// A second handler for the same event, next to the one in the Teams feature that recalculates the
/// team total. Each slice reacts on its own, and neither knows the other exists.
/// </remarks>
public sealed class PowerLevelUpdatedPublisher(ITopicEventSender sender)
    : IDomainEventHandler<PowerLevelUpdatedEvent>
{
    public async Task HandleAsync(PowerLevelUpdatedEvent domainEvent, CancellationToken cancellationToken)
    {
        ThrowIfNull(domainEvent);

        await sender.SendAsync(
            nameof(PowerLevelUpdatedSubscription.HeroPowerLevelUpdated),
            domainEvent.Hero,
            cancellationToken);
    }
}
