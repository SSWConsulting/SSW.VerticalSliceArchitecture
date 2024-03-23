using MediatR;

namespace VerticalSliceArchitectureTemplate.Features.Todos.Domain.Events;

public record TodoCompletedEvent(Guid TodoId) : INotification;