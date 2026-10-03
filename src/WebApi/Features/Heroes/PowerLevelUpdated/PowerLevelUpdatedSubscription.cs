using SSW.VerticalSliceArchitecture.Domain.Heroes;

namespace SSW.VerticalSliceArchitecture.Features.Heroes.PowerLevelUpdated;

[SubscriptionType]
public static partial class PowerLevelUpdatedSubscription
{
    /// <summary>Pushes a hero to the client each time their power level changes.</summary>
    /// <remarks>
    /// <c>[EventMessage]</c> marks the parameter the topic delivers, so the method body only shapes
    /// what the client sees. The topic name is the field name, which is what
    /// <see cref="PowerLevelUpdatedPublisher"/> sends to.
    /// </remarks>
    [Subscribe]
    public static Hero HeroPowerLevelUpdated([EventMessage] Hero hero) => hero;
}
