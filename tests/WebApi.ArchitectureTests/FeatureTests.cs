using System.Text.RegularExpressions;
using FastEndpoints;
using Microsoft.EntityFrameworkCore;
using Mono.Cecil;
using SSW.VerticalSliceArchitecture.ArchitectureTests.Common;
using SSW.VerticalSliceArchitecture.Common.Persistence;

// FastEndpoints ships its own TypeDefinition; this file means Cecil's throughout.
using TypeDefinition = Mono.Cecil.TypeDefinition;

namespace SSW.VerticalSliceArchitecture.ArchitectureTests;

public class FeatureTests : TestBase
{
    private static readonly string DbContextFullName = typeof(DbContext).FullName!;
    private static readonly string ApplicationDbContextFullName = typeof(ApplicationDbContext).FullName!;

    /// <remarks>
    /// <c>IsAssignableFrom</c> walks the base-type chain, so this catches every FastEndpoints base
    /// (<c>Endpoint&lt;TRequest, TResponse&gt;</c> and its aliases) without NetArchTest's
    /// <c>Inherit()</c>, which does not follow open generics reliably.
    /// </remarks>
    private static readonly List<Type> Endpoints = RootAssembly
        .GetTypes()
        .Where(t => t is { IsAbstract: false, IsGenericTypeDefinition: false } && typeof(BaseEndpoint).IsAssignableFrom(t))
        .ToList();

    private static readonly Lazy<ModuleDefinition> LazyRootModule = new(ReadRootModule);

    private readonly ITestOutputHelper _output;

    public FeatureTests(ITestOutputHelper output)
    {
        _output = output;
    }

    [Fact]
    public void Endpoints_Should_BeNamedEndpointAndLiveInASliceNamespace()
    {
        // Arrange
        Endpoints.Dump(_output);

        // Act
        var invalidTypes = Endpoints
            .Where(t => !t.Name.EndsWith("Endpoint", StringComparison.Ordinal) || !IsSliceNamespace(t.Namespace))
            .ToList();

        // Assert
        Endpoints.Should().NotBeEmpty();
        invalidTypes.Should().BeEmpty(
            "every endpoint must be named *Endpoint and sit two segments below {0} — one for the feature, one for the use case — but these do not: {1}",
            FeaturesNamespace,
            Describe(invalidTypes));
    }

    [Fact]
    public void EndpointsWithARequest_Should_HaveAMatchingValidatorInTheirSlice()
    {
        // Arrange
        var requests = Endpoints
            .Select(t => (Endpoint: t, Request: GetRequestType(t)))
            .ToList();

        var endpointsWithRequest = requests
            .Where(x => x.Request is not null && x.Request != typeof(EmptyRequest))
            .ToList();

        endpointsWithRequest.Select(x => x.Endpoint).Dump(_output);

        // Act
        var missingValidators = endpointsWithRequest
            .Where(x => !HasValidatorInSlice(x.Endpoint, x.Request!))
            .Select(x => x.Endpoint)
            .ToList();

        // An endpoint whose request type can't be read isn't exempt, it's unclassified. Letting it fall
        // in with the request-less endpoints is how a rule quietly stops covering part of its subject.
        var unclassified = requests.Where(x => x.Request is null).Select(x => x.Endpoint).ToList();

        // Assert
        endpointsWithRequest.Should().NotBeEmpty();
        unclassified.Should().BeEmpty(
            "every endpoint must derive from Endpoint<TRequest, TResponse> so its request type is known, but these do not: {0}",
            Describe(unclassified));
        missingValidators.Should().BeEmpty(
            "every endpoint with a request must have a Validator<TRequest> in the same slice, but these do not: {0}",
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
    public void Endpoints_Should_OnlyDependOnApplicationDbContext()
    {
        // Arrange
        var endpointDefinitions = GetEndpointDefinitions();

        var endpointsUsingADbContext = endpointDefinitions
            .Select(td => (Endpoint: td, DbContexts: GetDbContextDependencies(td)))
            .Where(x => x.DbContexts.Count > 0)
            .ToList();

        foreach (var (endpoint, dbContexts) in endpointsUsingADbContext)
            _output.WriteLine($"{endpoint.FullName} -> {string.Join(", ", dbContexts.Select(d => d.FullName))}");

        // Act
        var invalidTypes = endpointsUsingADbContext
            .Where(x => x.DbContexts.Any(db => !string.Equals(db.FullName, ApplicationDbContextFullName, StringComparison.Ordinal)))
            .Select(x => x.Endpoint.FullName)
            .ToList();

        // Assert
        endpointDefinitions.Should().HaveCount(Endpoints.Count, "the IL scan must see the same endpoints reflection found");
        endpointsUsingADbContext.Should().NotBeEmpty();
        invalidTypes.Should().BeEmpty(
            "endpoints must take {0}, not the DbContext base type or a second DbContext, but these do not: {1}",
            nameof(ApplicationDbContext),
            string.Join(", ", invalidTypes));
    }

    /// <summary>
    /// The request type an endpoint handles, or <c>null</c> when it can't be determined.
    /// </summary>
    /// <remarks>
    /// The request-less base types are aliases: <c>Endpoint&lt;TRequest&gt;</c> is
    /// <c>Endpoint&lt;TRequest, object&gt;</c> and <c>EndpointWithoutRequest&lt;TResponse&gt;</c> is
    /// <c>Endpoint&lt;EmptyRequest, TResponse&gt;</c>. Callers filter out <c>EmptyRequest</c> to exclude the latter.
    /// </remarks>
    private static Type? GetRequestType(Type endpoint)
    {
        for (var type = endpoint.BaseType; type is not null; type = type.BaseType)
        {
            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Endpoint<,>))
                return type.GetGenericArguments()[0];
        }

        return null;
    }

    /// <remarks>
    /// Matches on FastEndpoints' <c>Validator&lt;T&gt;</c> rather than FluentValidation's
    /// <c>IValidator&lt;T&gt;</c> for two reasons. FastEndpoints only binds validators derived from its
    /// own base, so an <c>AbstractValidator&lt;T&gt;</c> would satisfy the interface while never running
    /// against a request. And <c>IValidator&lt;in T&gt;</c> is contravariant, so a base request's
    /// validator would satisfy every request derived from it; class assignability is invariant.
    /// </remarks>
    private static bool HasValidatorInSlice(Type endpoint, Type requestType)
    {
        var validatorBase = typeof(Validator<>).MakeGenericType(requestType);

        return RootAssembly
            .GetTypes()
            .Any(t => t is { IsAbstract: false } &&
                      string.Equals(t.Namespace, endpoint.Namespace, StringComparison.Ordinal) &&
                      validatorBase.IsAssignableFrom(t));
    }

    private static List<TypeDefinition> GetEndpointDefinitions()
    {
        var endpointNames = Endpoints
            .Select(t => t.FullName)
            .OfType<string>()
            .ToHashSet(StringComparer.Ordinal);

        return LazyRootModule.Value
            .GetTypes()
            .Where(td => endpointNames.Contains(td.FullName))
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
    /// Every <see cref="DbContext"/>-derived type an endpoint reaches for.
    /// </summary>
    /// <remarks>
    /// Reads IL rather than reflecting over members, because a signature is only one of the ways an
    /// endpoint can get hold of a service. FastEndpoints also supports handler-method injection, and
    /// <c>Resolve&lt;T&gt;()</c> names its service only in the generic argument of the call site —
    /// an idiom this codebase already uses.
    /// <para>
    /// Deliberately ignores the declaring type of called methods: <c>SaveChangesAsync</c> is declared on
    /// <c>DbContext</c>, so counting call targets would flag every correct endpoint.
    /// </para>
    /// </remarks>
    private static List<TypeReference> GetDbContextDependencies(TypeDefinition endpoint)
    {
        var references = new List<TypeReference>();

        // Nested types carry the real body of every async method: HandleAsync compiles down to a state
        // machine in a nested type, and the endpoint's own method body only starts it. Skip them and the
        // scan sees nothing an endpoint actually does.
        foreach (var type in WithNestedTypes(endpoint))
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
