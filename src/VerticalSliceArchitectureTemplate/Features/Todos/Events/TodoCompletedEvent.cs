namespace VerticalSliceArchitectureTemplate.Features.Todos.Events;

public record TodoCompletedEvent(Guid TodoId) : INotification;