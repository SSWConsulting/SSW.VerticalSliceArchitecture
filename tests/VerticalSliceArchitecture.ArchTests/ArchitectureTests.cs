using FluentAssertions;
using NetArchTest.Rules;

namespace VerticalSliceArchitecture.ArchTests;

public class ArchitectureTests : TestBase
{

    private string[] ApplicationNamespaces =
    [
        "Queries",
        "Application",
        "Commands"
    ];

    private string[] DomainNamespaces =
    [
        "Domain"
    ];

    private string[] InfrastructureNamespaces =
    [
        "Infrastructure",
        "Persistence"
    ];

    [Fact]
    public void Domain_Should_Not_Depend_On_Infrastructure()
    {
        var domainTypes = TypesMatchingAnyPattern(DomainNamespaces);
        var infraTypes = TypesMatchingAnyPattern(InfrastructureNamespaces);

        var result = domainTypes
            .ShouldNot()
            .HaveDependencyOnAny(infraTypes.GetNames())
            .GetResult();

        result.IsSuccessful.Should().BeTrue();
    }

    [Fact]
    public void Domain_Should_Not_Depend_On_Application()
    {
        var domainTypes = TypesMatchingAnyPattern(DomainNamespaces);
        var applicationTypes = TypesMatchingAnyPattern(ApplicationNamespaces);

        var result = domainTypes
            .ShouldNot()
            .HaveDependencyOnAny(applicationTypes.GetNames())
            .GetResult();

        result.IsSuccessful.Should().BeTrue();
    }
}
