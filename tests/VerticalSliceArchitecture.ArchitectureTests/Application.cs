using MediatR;
using VerticalSliceArchitecture.ArchTests.Common;

namespace VerticalSliceArchitecture.ArchTests;

public class Application : TestBase
{
    private static readonly Type IRequestHandler = typeof(IRequestHandler<,>);

    [Fact]
    public void CommandHandlers_ShouldHaveCorrectSuffix()
    {
        var types = Types
            .InAssembly(ApplicationAssembly)
            .That()
            .ResideInNamespaceContaining("Commands")
            .And()
            .ImplementInterface(IRequestHandler);

        var result = types
            .Should()
            .HaveNameEndingWith("CommandHandler")
            .GetResult();

        result.Should().BeSuccessful();
    }

    [Fact]
    public void QueryHandlers_ShouldHaveCorrectSuffix()
    {
        var types = Types
                .InAssembly(ApplicationAssembly)
                .That()
                .ResideInNamespaceContaining("Queries")
                .And()
                .ImplementInterface(IRequestHandler);

        var result = types
            .Should()
            .HaveNameEndingWith("QueryHandler")
            .GetResult();

        result.Should().BeSuccessful();
    }
}