namespace VerticalSliceArchitecture.Common;

public abstract class BaseEntity
{
    public readonly List<INotification> StagedEvents = new();
}