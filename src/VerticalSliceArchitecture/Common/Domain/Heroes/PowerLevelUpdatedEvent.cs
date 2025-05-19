using SSW.VerticalSliceArchitecture.Common.Domain.Base.Interfaces;

namespace SSW.VerticalSliceArchitecture.Common.Domain.Heroes;

public record PowerLevelUpdatedEvent(Hero Hero) : IDomainEvent;