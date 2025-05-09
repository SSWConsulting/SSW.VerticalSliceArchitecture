using VerticalSliceArchitectureTemplate.Features.Todos.Domain;
using TodoId = VerticalSliceArchitectureTemplate.Features.Todos.Domain.TodoId;

namespace VerticalSliceArchitectureTemplate.Unit.Tests.Features.Todos.Models;

public class TodoTests
{
    [Fact]
    public void Todo_Complete_ShouldUpdateCompleted()
    {
        // Arrange
        var item = new Todo
        {
            Id = TodoId.From(Guid.CreateVersion7()),
            Text = "My todo item"
        };
        
        // Act
        item.Complete();
        
        // Assert
        item.IsCompleted.Should().BeTrue();
    }
    
    [Fact]
    public void Todo_Complete_ShouldAddEvent()
    {
        // Arrange
        var item = new Todo
        {
            Id = TodoId.From(Guid.CreateVersion7()),
            Text = "My todo item"
        };
        
        // Act
        item.Complete();
        
        // Assert
        var domainEvents = item.PopDomainEvents();
        domainEvents.Should().NotBeNull();
        domainEvents.Should().HaveCount(2);
        domainEvents.Last().Should().BeOfType<TodoCompletedEvent>()
            .Which.Invoking(e =>
            {
                e.TodoId.Should().Be(item.Id);
            });
    }
}