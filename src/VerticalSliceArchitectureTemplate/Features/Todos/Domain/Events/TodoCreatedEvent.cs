namespace VerticalSliceArchitectureTemplate.Features.Todos.Domain.Events;

public record TodoCreatedEvent(Guid TodoId) : INotification;