using VerticalSliceArchitectureTemplate.Common.Domain.Base.Interfaces;

namespace VerticalSliceArchitectureTemplate.Features.Todos.Domain;

public record TodoCreatedEvent(TodoId TodoId) : IDomainEvent;