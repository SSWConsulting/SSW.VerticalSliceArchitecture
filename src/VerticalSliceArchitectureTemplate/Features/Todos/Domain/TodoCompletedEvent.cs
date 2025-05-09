using VerticalSliceArchitectureTemplate.Common.Domain.Base.Interfaces;

namespace VerticalSliceArchitectureTemplate.Features.Todos.Domain;

public record TodoCompletedEvent(TodoId TodoId) : IDomainEvent;