
using System.Collections;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using VerticalSliceArchitecture.Features.Todo;
using VerticalSliceArchitecture.Features.Todo.GetAllTodos;

namespace VerticalSliceArchitecture.Unit.Tests.Features.Todo.GetAllTodos;

public class GetAllTodosEndpointTests
{
    [Fact]
    public async Task GetAllTodos_WithData_ReturnsItems()
    {
        // Arrange
        var items = new[]
        {
            new TodoEntity
            {
                Id = Guid.NewGuid(),
                Text = "My todo item"
            }
        };
        
        var repo = Substitute.For<ITodoRepository>();
        repo.GetAllAsync(Arg.Any<CancellationToken>())
            .Returns(x => Task.FromResult<IEnumerable<TodoEntity>>(items));
        
        // Act
        var result = await GetAllTodosEndpoint.HandleAsync(repo, CancellationToken.None);

        result.Should().BeOfType<Ok<IEnumerable<TodoEntity>>>()
            .Which.Value.Should().BeEquivalentTo(items);
    }
    
}