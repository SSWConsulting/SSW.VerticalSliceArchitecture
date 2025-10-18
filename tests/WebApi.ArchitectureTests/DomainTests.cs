using FastEndpoints;
using System.Reflection;
using SSW.VerticalSliceArchitecture.ArchitectureTests.Common;
using SSW.VerticalSliceArchitecture.Common.Domain.Base;
using SSW.VerticalSliceArchitecture.Common.Domain.Base.Interfaces;

namespace SSW.VerticalSliceArchitecture.ArchitectureTests;

public class DomainTests : TestBase
{
    private static readonly Type AggregateRoot = typeof(AggregateRoot<>);
    private static readonly Type Entity = typeof(Entity<>);
    private static readonly Type DomainEvent = typeof(IEvent);
    private static readonly Type ValueObject = typeof(IValueObject);

    private readonly ITestOutputHelper _output;

    public DomainTests(ITestOutputHelper output)
    {
        _output = output;
    }

    [Fact]
    public void DomainModel_Should_InheritsBaseClasses()
    {
        // Arrange
        var domainModels = Types.InAssembly(RootAssembly)
            .That()
            .ResideInNamespaceContaining(DomainAssemblyName)
            .And().DoNotResideInNamespaceContaining("Base")
            .And().DoNotHaveNameMatching(".*Id.*")
            .And().DoNotHaveNameMatching(".*Vogen.*")
            .And().DoNotHaveName("ThrowHelper")
            .And().DoNotHaveNameEndingWith("Spec")
            .And().DoNotHaveNameEndingWith("Errors")
            .And().MeetCustomRule(new IsNotEnumRule());
        var types = domainModels.GetTypes().ToList();
        
        types.Dump(_output);

        // Act
        var result = domainModels
            .Should()
            .Inherit(AggregateRoot)
            .Or().Inherit(Entity)
            .Or().ImplementInterface(DomainEvent)
            .Or().ImplementInterface(ValueObject)
            .GetResult();

        // Assert
        types.Should().NotBeEmpty();
        result.Should().BeSuccessful();
    }

    [Fact]
    public void EntitiesAndAggregates_Should_HavePrivateParameterlessConstructor()
    {
        // Arrange
        var entityTypes = Types
            .InAssembly(RootAssembly)
            .That()
            .Inherit(Entity)
            .Or()
            .Inherit(AggregateRoot);
        var types = entityTypes.GetTypes().ToList();
        
        types.Dump(_output);

        // Act
        var failingTypes = entityTypes
            .GetTypes()
            .Where(t => t != AggregateRoot && !t.GetConstructors(BindingFlags.NonPublic | BindingFlags.Instance)
                .Any(c => c.IsPrivate && c.GetParameters().Length == 0))
            .ToList();

        // Assert
        types.Should().NotBeEmpty();
        failingTypes.Should().BeEmpty();
    }
}