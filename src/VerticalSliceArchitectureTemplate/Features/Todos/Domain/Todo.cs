using System.ComponentModel.DataAnnotations;
using VerticalSliceArchitectureTemplate.Common.Domain;
using VerticalSliceArchitectureTemplate.Features.Todos.Domain.Events;

namespace VerticalSliceArchitectureTemplate.Features.Todos.Domain;

public class Todo : BaseEntity
{
    public Todo()
    {
        AddEvent(new TodoCreatedEvent(Id));
    }
    
    public Guid Id { get; init; }
    
    [MaxLength(1024)]
    public string Text { get; set; } = string.Empty;
    public bool IsCompleted { get; private set; }
    
    /// <exception cref="InvalidOperationException">Throws when trying to complete an already completed item</exception>
    public void Complete()
    {
        if (IsCompleted)
        {
            throw new InvalidOperationException("Todo is already completed");
        }
        
        IsCompleted = true;
    
        AddEvent(new TodoCompletedEvent(Id));
    }
}
