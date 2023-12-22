namespace VerticalSliceArchitectureTemplate.Features.Todos.Events;

public record TodoCompletedEvent(Todo Todo) : INotification;