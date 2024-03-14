namespace VerticalSliceArchitectureTemplate.Common.Models;

public abstract class BaseEntity
{
    public readonly List<INotification> StagedEvents = [];
}