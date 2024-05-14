using System.ComponentModel.DataAnnotations;
using VerticalSliceArchitectureTemplate.Common.Domain;
using VerticalSliceArchitectureTemplate.Features.Todos.Application;
using VerticalSliceArchitectureTemplate.Features.Todos.Commands;
using VerticalSliceArchitectureTemplate.Features.Todos.Domain.Events;
using VerticalSliceArchitectureTemplate.Features.Todos.Infrastructure;
using VerticalSliceArchitectureTemplate.Features.Todos.Queries;

namespace VerticalSliceArchitectureTemplate.Features.Todos.Domain;

public class Todo : BaseEntity
{
    public Todo()
    {
        StagedEvents.Add(new TodoCreatedEvent(Id));
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
    
        StagedEvents.Add(new TodoCompletedEvent(Id));
    }

    public void Save(DataService service)
    {

    }

    // public void Save(MyApp app)
    // {
    //
    // }

    // public void Save(MyQuery app)
    // {
    //
    // }
    //
    // public void Save(MyCommand app)
    // {
    //
    // }
}
