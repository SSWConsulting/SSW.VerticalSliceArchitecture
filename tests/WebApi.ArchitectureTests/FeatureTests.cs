using System.Reflection;
using System.Text.RegularExpressions;
using FluentValidation;
using HotChocolate.Types;
using Microsoft.EntityFrameworkCore;
using Mono.Cecil;
using SSW.VerticalSliceArchitecture.ArchitectureTests.Common;
using SSW.VerticalSliceArchitecture.Common.Persistence;

// Cecil and HotChocolate both ship a TypeDefinition; this file means Cecil's throughout.
using TypeDefinition = Mono.Cecil.TypeDefinition;

namespace SSW.VerticalSliceArchitecture.ArchitectureTests;

public class FeatureTests : TestBase
{
    private static readonly string DbContextFullName = typeof(DbContext).FullName!;
    private static readonly string ApplicationDbContextFullName = typeof(ApplicationDbContext).FullName!;

    private static readonly string[] ResolverSuffixes = ["Query", "Mutation", "Subscription"];

    /// <summary>
    /// Every class that contributes fields to a root operation type.
    /// </summary>
    /// <remarks>
    /// Found by attribute rather than by base type: a HotChocolate resolver class is a static
    /// partial with no base to inherit, so the attribute is the only thing that marks it.
    /// </remarks>
    private static readonly List<Type> Resolvers = RootAssembly
        .GetTypes()
        .Where(HasOperationTypeAttribute)
        .ToList();

    private static readonly Lazy<ModuleDefinition> LazyRootModule = new(ReadRootModule);

    private readonly ITestOutputHelper _output;

    public FeatureTests(ITestOutputHelper output)
    {
        _output = output;
    }

    [Fact]
    public void Resolvers_Should_BeNamedForTheirOperationAndLiveInASliceNamespace()
    {
        // Arrange
        Resolvers.Dump(_output);

        // Act
        var invalidTypes = Resolvers
            .Where(t => !ResolverSuffixes.Any(s => t.Name.EndsWith(s, StringComparison.Ordinal)) ||
                        !IsSliceNamespace(t.Namespace))
            .ToList();

        // Assert
        Resolvers.Should().NotBeEmpty();
        invalidTypes.Should().BeEmpty(
            "every resolver must be named *Query, *Mutation or *Subscription and sit two segments below {0} — one for the feature, one for the use case — but these do not: {1}",
            FeaturesNamespace,
            Describe(invalidTypes));
    }

    /// <remarks>
    /// Static partial is not a style preference: the HotChocolate source generator emits the other
    /// half of the class, and a non-partial or instance class produces no schema field at all —
    /// silently, because nothing fails to compile.
    /// </remarks>
    [Fact]
    public void Resolvers_Should_BeStaticAndPartial()
    {
        // Arrange
        Resolvers.Dump(_output);

        // Act
        var invalidTypes = Resolvers
            .Where(t => !(t.IsAbstract && t.IsSealed))
            .ToList();

        // Assert
        Resolvers.Should().NotBeEmpty();
        invalidTypes.Should().BeEmpty(
            "every resolver must be a static class so the source generator can extend it, but these are not: {0}",
            Describe(invalidTypes));
    }

    /// <summary>
    /// A mutation that takes an input record must have a FluentValidation validator beside it.
    /// </summary>
    /// <remarks>
    /// GraphQL validates the shape of an input, never its content: it will happily accept an empty
    /// name or a power level of 500. Nothing else in the pipeline checks, so a missing validator is
    /// the difference between a typed error in the payload and a faulted resolver.
    /// </remarks>
    [Fact]
    public void MutationInputs_Should_HaveAValidatorInTheirSlice()
    {
        // Arrange
        var inputs = Resolvers
            .Where(t => t.GetCustomAttributes().Any(a => a is MutationTypeAttribute))
            .SelectMany(GetInputTypes)
            .DistinctBy(x => x.Input)
            .ToList();

        inputs.Select(x => x.Input).Dump(_output);

        // Act
        var missingValidators = inputs
            .Where(x => !HasValidatorInSlice(x.Resolver, x.Input))
            .Select(x => x.Input)
            .ToList();

        // Assert
        inputs.Should().NotBeEmpty();
        missingValidators.Should().BeEmpty(
            "every mutation input must have an AbstractValidator<TInput> in the same slice, but these do not: {0}",
            Describe(missingValidators));
    }

    [Fact]
    public void Slices_Should_NotDependOnOtherSlices()
    {
        // Arrange
        var emptySlices = new List<string>();
        var crossSliceTypes = new List<string>();

        foreach (var slice in SliceNamespaces)
        {
            var otherSlices = SliceNamespaces
                .Where(ns => !string.Equals(ns, slice, StringComparison.Ordinal))
                .ToArray();

            var sliceTypes = Types
                .InAssembly(RootAssembly)
                .That()
                .ResideInNamespaceMatching(ExactNamespaceOrChildPattern(slice));

            var types = sliceTypes.GetTypes().ToList();
            types.Dump(_output);

            if (types.Count == 0)
            {
                emptySlices.Add(slice);
                continue;
            }

            if (otherSlices.Length == 0)
                continue;

            // Act — every slice is checked, so one run reports all violations rather than the first
            var result = sliceTypes
                .ShouldNot()
                .HaveDependencyOnAny(otherSlices)
                .GetResult();

            result.DumpFailingTypes(_output);

            if (!result.IsSuccessful)
                crossSliceTypes.AddRange(result.FailingTypeNames ?? []);
        }

        // Assert
        SliceNamespaces.Should().NotBeEmpty();
        emptySlices.Should().BeEmpty(
            "a namespace was discovered as a slice, so it must contain types, but these are empty: {0}",
            string.Join(", ", emptySlices));
        crossSliceTypes.Should().BeEmpty(
            "no slice may depend on another slice's types, but these do: {0}",
            string.Join(", ", crossSliceTypes));
    }

    [Fact]
    public void Resolvers_Should_OnlyDependOnApplicationDbContext()
    {
        // Arrange
        var resolverDefinitions = GetResolverDefinitions();

        var resolversUsingADbContext = resolverDefinitions
            .Select(td => (Resolver: td, DbContexts: GetDbContextDependencies(td)))
            .Where(x => x.DbContexts.Count > 0)
            .ToList();

        foreach (var (resolver, dbContexts) in resolversUsingADbContext)
            _output.WriteLine($"{resolver.FullName} -> {string.Join(", ", dbContexts.Select(d => d.FullName))}");

        // Act
        var invalidTypes = resolversUsingADbContext
            .Where(x => x.DbContexts.Any(db => !string.Equals(db.FullName, ApplicationDbContextFullName, StringComparison.Ordinal)))
            .Select(x => x.Resolver.FullName)
            .ToList();

        // Assert
        resolverDefinitions.Should().HaveCount(Resolvers.Count, "the IL scan must see the same resolvers reflection found");
        resolversUsingADbContext.Should().NotBeEmpty();
        invalidTypes.Should().BeEmpty(
            "resolvers must take {0}, not the DbContext base type or a second DbContext, but these do not: {1}",
            nameof(ApplicationDbContext),
            string.Join(", ", invalidTypes));
    }

    /// <summary>
    /// The object types that bind an aggregate to the schema belong to the feature, not to a slice.
    /// </summary>
    /// <remarks>
    /// One Hero type serves every hero slice — that is what makes it a graph. Declaring it inside a
    /// slice would make every other slice depend on that slice to return a hero.
    /// </remarks>
    [Fact]
    public void ObjectTypes_Should_LiveAtFeatureLevel()
    {
        // Arrange
        var objectTypes = RootAssembly
            .GetTypes()
            .Where(t => t.GetCustomAttributes().Any(a => a.GetType().Name.StartsWith("ObjectTypeAttribute", StringComparison.Ordinal)))
            .ToList();

        objectTypes.Dump(_output);

        // Act
        var invalidTypes = objectTypes
            .Where(t => IsSliceNamespace(t.Namespace))
            .ToList();

        // Assert
        objectTypes.Should().NotBeEmpty();
        invalidTypes.Should().BeEmpty(
            "an [ObjectType<T>] must sit at feature level, not inside a slice, but these do not: {0}",
            Describe(invalidTypes));
    }

    private static bool HasOperationTypeAttribute(Type type) =>
        type.GetCustomAttributes().Any(a =>
            a is QueryTypeAttribute or MutationTypeAttribute or SubscriptionTypeAttribute);

    /// <summary>
    /// Every parameter of a resolver method that is an input record declared in the same slice.
    /// </summary>
    /// <remarks>
    /// Identified by namespace rather than by a name suffix: services, DataLoaders and the
    /// cancellation token all come from elsewhere, and a slice's own types are exactly the ones the
    /// slice is responsible for validating.
    /// </remarks>
    private static IEnumerable<(Type Resolver, Type Input)> GetInputTypes(Type resolver) =>
        resolver
            .GetMethods(BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly)
            .SelectMany(m => m.GetParameters())
            .Select(p => p.ParameterType)
            .Where(t => string.Equals(t.Namespace, resolver.Namespace, StringComparison.Ordinal))
            .Select(t => (resolver, t));

    private static bool HasValidatorInSlice(Type resolver, Type inputType)
    {
        var validatorBase = typeof(AbstractValidator<>).MakeGenericType(inputType);

        return RootAssembly
            .GetTypes()
            .Any(t => t is { IsAbstract: false } &&
                      string.Equals(t.Namespace, resolver.Namespace, StringComparison.Ordinal) &&
                      validatorBase.IsAssignableFrom(t));
    }

    private static List<TypeDefinition> GetResolverDefinitions()
    {
        var resolverNames = Resolvers
            .Select(t => t.FullName)
            .OfType<string>()
            .ToHashSet(StringComparer.Ordinal);

        return LazyRootModule.Value
            .GetTypes()
            .Where(td => resolverNames.Contains(td.FullName))
            .ToList();
    }

    /// <remarks>
    /// The assembly's own directory has to be a search directory or <see cref="TypeReference.Resolve"/>
    /// can't follow a base type into EF Core, and the DbContext check silently sees nothing.
    /// </remarks>
    private static ModuleDefinition ReadRootModule()
    {
        var resolver = new DefaultAssemblyResolver();
        resolver.AddSearchDirectory(Path.GetDirectoryName(RootAssembly.Location)!);

        return ModuleDefinition.ReadModule(RootAssembly.Location, new ReaderParameters { AssemblyResolver = resolver });
    }

    /// <summary>
    /// Every <see cref="DbContext"/>-derived type a resolver reaches for.
    /// </summary>
    /// <remarks>
    /// Reads IL rather than reflecting over members, because a signature is only one of the ways a
    /// resolver can get hold of a service — it can also resolve one from the request's service
    /// provider, which names the service only in the generic argument of the call site.
    /// <para>
    /// Deliberately ignores the declaring type of called methods: <c>SaveChangesAsync</c> is declared on
    /// <c>DbContext</c>, so counting call targets would flag every correct resolver.
    /// </para>
    /// </remarks>
    private static List<TypeReference> GetDbContextDependencies(TypeDefinition resolver)
    {
        var references = new List<TypeReference>();

        // Nested types carry the real body of every async method: the resolver compiles down to a
        // state machine in a nested type, and the method body only starts it. Skip them and the scan
        // sees nothing a resolver actually does.
        foreach (var type in WithNestedTypes(resolver))
        {
            references.AddRange(type.Fields.Select(f => f.FieldType));
            references.AddRange(type.Properties.Select(p => p.PropertyType));

            foreach (var method in type.Methods)
            {
                references.AddRange(method.Parameters.Select(p => p.ParameterType));

                if (!method.HasBody)
                    continue;

                references.AddRange(method.Body.Variables.Select(v => v.VariableType));
                references.AddRange(method.Body.Instructions
                    .Select(i => i.Operand)
                    .OfType<GenericInstanceMethod>()
                    .SelectMany(m => m.GenericArguments));
            }
        }

        return references
            .Where(IsDbContext)
            .DistinctBy(r => r.FullName, StringComparer.Ordinal)
            .ToList();
    }

    /// <remarks>
    /// AwesomeAssertions' <c>BeEmpty</c> only prints the first offending item, so the full list goes in
    /// the reason — one CI run should name everything that needs fixing.
    /// </remarks>
    private static string Describe(IEnumerable<Type> types) => string.Join(", ", types.Select(t => t.FullName));

    private static IEnumerable<TypeDefinition> WithNestedTypes(TypeDefinition type)
    {
        yield return type;

        foreach (var nested in type.NestedTypes.SelectMany(WithNestedTypes))
            yield return nested;
    }

    /// <remarks>
    /// A reference that fails to resolve throws rather than being treated as "not a DbContext" — a rule
    /// that quietly skips what it can't read is the failure mode these tests exist to avoid.
    /// </remarks>
    private static bool IsDbContext(TypeReference reference)
    {
        for (var type = reference.Resolve(); type is not null; type = type.BaseType?.Resolve())
        {
            if (string.Equals(type.FullName, DbContextFullName, StringComparison.Ordinal))
                return true;
        }

        return false;
    }

    /// <summary>
    /// Matches a namespace exactly, or any namespace nested under it.
    /// </summary>
    /// <remarks>
    /// NetArchTest's <c>ResideInNamespace</c> matches on a raw string prefix, so a slice named
    /// <c>GetTeam</c> would also capture types in a <c>GetTeamMembers</c> slice and report that slice's
    /// own types as cross-slice dependencies. Anchoring the pattern keeps each slice's type set exact.
    /// </remarks>
    private static string ExactNamespaceOrChildPattern(string ns) => $"^{Regex.Escape(ns)}(\\..+)?$";
}
