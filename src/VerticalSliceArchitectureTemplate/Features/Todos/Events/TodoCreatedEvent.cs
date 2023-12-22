namespace VerticalSliceArchitectureTemplate.Features.Todos.Events;

public record TodoCreatedEvent(Todo Todo) : INotification;