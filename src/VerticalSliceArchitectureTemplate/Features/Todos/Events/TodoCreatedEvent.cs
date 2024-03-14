namespace VerticalSliceArchitectureTemplate.Features.Todos.Events;

public record TodoCreatedEvent(Guid TodoId) : INotification;