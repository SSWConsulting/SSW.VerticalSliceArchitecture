using VerticalSliceArchitectureTemplate.Common.Domain.Base.EventualConsistency;
using VerticalSliceArchitectureTemplate.Common.Domain.Base.Interfaces;

namespace VerticalSliceArchitectureTemplate.Common.Domain.Heroes;

public record PowerLevelUpdatedEvent(Hero Hero) : IDomainEvent
{
    public static readonly Error TeamNotFound = EventualConsistencyError.From(
        code: "PowerLeveUpdated.TeamNotFound",
        description: "Team not found");
}