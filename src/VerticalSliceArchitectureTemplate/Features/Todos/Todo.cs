using System.ComponentModel.DataAnnotations;
using Ardalis.Result;
using VerticalSliceArchitectureTemplate.Common;
using VerticalSliceArchitectureTemplate.Features.Todos.Events;

namespace VerticalSliceArchitectureTemplate.Features.Todos;

public class Todo : BaseEntity
{
    public Todo()
    {
        StagedEvents.Add(new TodoCreatedEvent(this));
    }
    
    public Guid Id { get; init; }
    
    [MaxLength(1024)]
    public string Text { get; set; } = string.Empty;
    public bool Completed { get; private set; }
    
    public Result Complete()
    {
        if (Completed)
        {
            return Result.Invalid(new ValidationError
            {
                Identifier = nameof(Completed),
                ErrorMessage = "Todo is already completed."
            });
        }
        
        Completed = true;
    
        StagedEvents.Add(new TodoCompletedEvent(this));
        
        return Result.Success();
    }
}