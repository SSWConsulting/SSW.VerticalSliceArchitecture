
using System.Collections;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using VerticalSliceArchitecture.Features.Todos;
using VerticalSliceArchitecture.Features.Todos.Queries.GetAllTodos;

namespace VerticalSliceArchitecture.Unit.Tests.Features.Todo.GetAllTodos;

public class GetAllTodosEndpointTests
{
    [Fact]
    public async Task GetAllTodos_WithData_ReturnsItems()
    {
        // Arrange
        var items = new[]
        {
            new VerticalSliceArchitecture.Features.Todos.Todo
            {
                Id = Guid.NewGuid(),
                Text = "My todo item"
            }
        };
        
        var repo = Substitute.For<ITodoRepository>();
        repo.GetAllAsync(Arg.Any<CancellationToken>())
            .Returns(x => Task.FromResult<IEnumerable<VerticalSliceArchitecture.Features.Todos.Todo>>(items));
        
        // Act
        var result = await GetAllTodosEndpoint.HandleAsync(repo, CancellationToken.None);

        result.Should().BeOfType<Ok<IEnumerable<VerticalSliceArchitecture.Features.Todos.Todo>>>()
            .Which.Value.Should().BeEquivalentTo(items);
    }
    
}