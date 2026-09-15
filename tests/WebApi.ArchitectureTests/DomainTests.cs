using System.Reflection;
using SSW.VerticalSliceArchitecture.ArchitectureTests.Common;
using SSW.VerticalSliceArchitecture.Domain.Base;
using SSW.VerticalSliceArchitecture.Domain.Base.Interfaces;

namespace SSW.VerticalSliceArchitecture.ArchitectureTests;

public class DomainTests : TestBase
{
    private static readonly Type AggregateRoot = typeof(AggregateRoot<>);
    private static readonly Type Entity = typeof(Entity<>);
    private static readonly Type DomainEvent = typeof(IDomainEvent);
    private static readonly Type ValueObject = typeof(IValueObject);

    /// <summary>
    /// The packages the domain project is allowed to reference.
    /// </summary>
    /// <remarks>
    /// The whole reason the domain is its own project. Anything outside this list — EF Core,
    /// ASP.NET Core, HotChocolate, a messaging library — means the model has started to depend on
    /// how it is stored or served, which is the coupling the split exists to prevent.
    /// </remarks>
    private static readonly string[] AllowedReferences =
    [
        "System",
        "netstandard",
        "Ardalis.Specification",
        "ErrorOr",
        "Vogen.SharedTypes"
    ];

    private readonly ITestOutputHelper _output;

    public DomainTests(ITestOutputHelper output)
    {
        _output = output;
    }

    [Fact]
    public void Domain_Should_OnlyReferenceAllowedPackages()
    {
        // Arrange
        var references = DomainAssembly.GetReferencedAssemblies()
            .Select(a => a.Name)
            .OfType<string>()
            .Order(StringComparer.Ordinal)
            .ToList();

        foreach (var reference in references)
            _output.WriteLine(reference);

        // Act
        var disallowed = references
            .Where(name => !AllowedReferences.Any(allowed =>
                name.Equals(allowed, StringComparison.Ordinal) ||
                name.StartsWith(allowed + ".", StringComparison.Ordinal)))
            .ToList();

        // Assert
        references.Should().NotBeEmpty();
        disallowed.Should().BeEmpty(
            "the domain may only reference {0}, but it also references: {1}",
            string.Join(", ", AllowedReferences),
            string.Join(", ", disallowed));
    }

    [Fact]
    public void DomainModel_Should_InheritsBaseClasses()
    {
        // Arrange
        var domainModels = Types.InAssembly(DomainAssembly)
            .That()
            .DoNotResideInNamespaceContaining("Base")
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
            .InAssembly(DomainAssembly)
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
