using FluentAssertions;
using Microsoft.AspNetCore.Http.HttpResults;
using NSubstitute;
using VerticalSliceArchitectureTemplate.Features.Todos;
using VerticalSliceArchitectureTemplate.Features.Todos.Queries.GetAllTodos;

namespace VerticalSliceArchitectureTemplate.Unit.Tests.Features.Todos.GetAllTodos;

public class GetAllTodosEndpointTests
{
    [Fact]
    public async Task GetAllTodos_WithData_ReturnsItems()
    {
        // Arrange
        var items = new[]
        {
            new Todo
            {
                Id = Guid.NewGuid(),
                Text = "My todo item"
            }
        };

        var repo = Substitute.For<ITodoRepository>();
        repo.GetAllAsync(Arg.Any<bool?>(), Arg.Any<CancellationToken>())
            .Returns(x => Task.FromResult<IEnumerable<Todo>>(items));

        // Act
        var result = await GetAllTodosEndpoint.HandleAsync(null, repo, CancellationToken.None);

        result.Should().BeOfType<Ok<IEnumerable<Todo>>>()
            .Which.Value.Should().BeEquivalentTo(items);
    }
}