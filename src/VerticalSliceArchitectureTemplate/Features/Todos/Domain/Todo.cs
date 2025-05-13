using System.ComponentModel.DataAnnotations;
using VerticalSliceArchitectureTemplate.Common.Domain.Base;

namespace VerticalSliceArchitectureTemplate.Features.Todos.Domain;

// For strongly typed IDs, check out the rule: https://www.ssw.com.au/rules/do-you-use-strongly-typed-ids/
[ValueObject<Guid>]
public readonly partial struct TodoId;

public class Todo : AggregateRoot<TodoId>
{
    public Todo()
    {
        AddDomainEvent(new TodoCreatedEvent(Id));
    }

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

        AddDomainEvent(new TodoCompletedEvent(Id));
    }
}
