using MediatR;
using VerticalSliceArchitecture.ArchitectureTests.Common;

namespace VerticalSliceArchitecture.ArchitectureTests;

public class ApplicationTests: TestBase
{
    private readonly ITestOutputHelper _output;

    public ApplicationTests(ITestOutputHelper output)
    {
        _output = output;
    }
    
    [Fact]
    public void CommandHandlers_Should_HaveCorrectSuffix()
    {
        // Arrange
        var commandTypes = Types
            .InAssembly(RootAssembly)
            .That()
            .ResideInNamespaceContaining(CommandsAssemblyName)
            .GetTypes()
            .WithNestedTypes()
            .Where(t => t.GetInterfaces().Any(i =>
                i.IsGenericType &&
                i.GetGenericTypeDefinition() == typeof(IRequestHandler<,>)
            ))
            .ToList();
        
        commandTypes.Dump(_output);
        
        // Act
        var invalidNames = commandTypes
            .Where(t => t.Name != "Handler")
            .ToList();
        
        // Assert
        commandTypes.Should().NotBeEmpty();
        invalidNames.Should().BeEmpty();
    }

    [Fact]
    public void QueryHandlers_Should_HaveCorrectSuffix()
    {
        // Arrange
        var commandTypes = Types
            .InAssembly(RootAssembly)
            .That()
            .ResideInNamespaceContaining(QueriesAssemblyName)
            .GetTypes()
            .WithNestedTypes()
            .Where(t => t.GetInterfaces().Any(i =>
                i.IsGenericType &&
                i.GetGenericTypeDefinition() == typeof(IRequestHandler<,>)
            ))
            .ToList();
        
        commandTypes.Dump(_output);
        
        // Act
        var invalidNames = commandTypes
            .Where(t => t.Name != "Handler")
            .ToList();
        
        // Assert
        commandTypes.Should().NotBeEmpty();
        invalidNames.Should().BeEmpty();
    }
}