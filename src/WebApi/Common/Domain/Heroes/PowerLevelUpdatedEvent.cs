using SSW.VerticalSliceArchitecture.Common.Domain.Base.EventualConsistency;

namespace SSW.VerticalSliceArchitecture.Common.Domain.Heroes;

public record PowerLevelUpdatedEvent(Hero Hero) : IEvent
{
    public static readonly Error TeamNotFound = EventualConsistencyError.From(
        code: "PowerLeveUpdated.TeamNotFound",
        description: "Team not found");
}