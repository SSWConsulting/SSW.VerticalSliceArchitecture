using FluentAssertions;
using VerticalSliceArchitectureTemplate.Features.Todos.Events;
using VerticalSliceArchitectureTemplate.Features.Todos.Models;

namespace VerticalSliceArchitectureTemplate.Unit.Tests.Features.Todos.Models;

public class TodoTests
{
    [Fact]
    public void Todo_Complete_ShouldUpdateCompletedAndAddEvent()
    {
        // Arrange
        var item = new Todo
        {
            Id = Guid.NewGuid(),
            Text = "My todo item"
        };
        
        // Act
        item.Complete();
        
        // Assert
        item.Completed.Should().BeTrue();
        item.StagedEvents.Should().ContainSingle(x => x is TodoCompletedEvent, "because the item was completed");
    }
}