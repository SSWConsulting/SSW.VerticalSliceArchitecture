using VerticalSliceArchitectureTemplate.Common.Domain.Base.Interfaces;

namespace VerticalSliceArchitectureTemplate.Common.Domain.Todos;

public record TodoCompletedEvent(TodoId TodoId) : IDomainEvent;