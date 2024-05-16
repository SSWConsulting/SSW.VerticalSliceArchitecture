using FluentAssertions;
using VerticalSliceArchitecture.ArchTests.Common;
using Xunit.Abstractions;

namespace VerticalSliceArchitecture.ArchTests;

public class ArchitectureTests : TestBase
{
    private readonly ITestOutputHelper _outputHelper;

    public ArchitectureTests(ITestOutputHelper outputHelper)
    {
        _outputHelper = outputHelper;
    }

    private readonly string[] _applicationNamespaces =
    [
        "Queries",
        "Application",
        "Commands"
    ];

    private readonly string[] _domainNamespaces =
    [
        "Domain"
    ];

    private readonly string[] _infrastructureNamespaces =
    [
        "Infrastructure",
        "Persistence"
    ];

    [Fact]
    public void Domain_Should_Not_Depend_On_Infrastructure()
    {
        // Arrange
        var domainTypes = TypesMatchingAnyPattern(_domainNamespaces);
        var infraTypes = TypesMatchingAnyPattern(_infrastructureNamespaces);

        // Act
        var result = domainTypes
            .ShouldNot()
            .HaveDependencyOnAny(infraTypes.GetNames())
            .GetResult();

        // Assert
        result.DumpFailingTypes(_outputHelper);
        result.IsSuccessful.Should().BeTrue();
    }

    [Fact]
    public void Domain_Should_Not_Depend_On_Application()
    {
        // Arrange
        var domainTypes = TypesMatchingAnyPattern(_domainNamespaces);
        var applicationTypes = TypesMatchingAnyPattern(_applicationNamespaces);

        // Act
        var result = domainTypes
            .ShouldNot()
            .HaveDependencyOnAny(applicationTypes.GetNames())
            .GetResult();

        // Assert
        result.DumpFailingTypes(_outputHelper);
        result.IsSuccessful.Should().BeTrue();
    }
}
