using SSW.VerticalSliceArchitecture.Domain.Base.EventualConsistency;
using SSW.VerticalSliceArchitecture.Domain.Base.Interfaces;

namespace SSW.VerticalSliceArchitecture.Domain.Heroes;

public record PowerLevelUpdatedEvent(Hero Hero) : IDomainEvent
{
    public static readonly Error TeamNotFound = EventualConsistencyError.From(
        code: "PowerLeveUpdated.TeamNotFound",
        description: "Team not found");
}