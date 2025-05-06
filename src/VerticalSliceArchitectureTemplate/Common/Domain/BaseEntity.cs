using MediatR;

namespace VerticalSliceArchitectureTemplate.Common.Domain;

public abstract class BaseEntity
{
    private readonly List<INotification> _stagedEvents = [];

    public IReadOnlyList<INotification> StagedEvents => _stagedEvents;

    protected void AddEvent(INotification notification) => _stagedEvents.Add(notification);
    public void ClearEvents() => _stagedEvents.Clear();
}