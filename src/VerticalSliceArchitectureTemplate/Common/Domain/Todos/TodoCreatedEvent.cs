using VerticalSliceArchitectureTemplate.Common.Domain.Base.Interfaces;

namespace VerticalSliceArchitectureTemplate.Common.Domain.Todos;

public record TodoCreatedEvent(TodoId TodoId) : IDomainEvent;