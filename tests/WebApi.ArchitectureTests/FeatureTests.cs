using System.Reflection;
using System.Text.RegularExpressions;
using FastEndpoints;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using SSW.VerticalSliceArchitecture.ArchitectureTests.Common;
using SSW.VerticalSliceArchitecture.Common.Persistence;

namespace SSW.VerticalSliceArchitecture.ArchitectureTests;

public class FeatureTests : TestBase
{
    /// <remarks>
    /// Endpoints are found by walking the type hierarchy rather than with NetArchTest's
    /// <c>Inherit()</c>, because every FastEndpoints base type is an open generic
    /// (<c>Endpoint&lt;TRequest, TResponse&gt;</c>) and <c>Inherit()</c> does not follow those reliably.
    /// </remarks>
    private static readonly IReadOnlyList<Type> Endpoints = RootAssembly
        .GetTypes()
        .Where(t => t is { IsAbstract: false, IsGenericTypeDefinition: false } && typeof(BaseEndpoint).IsAssignableFrom(t))
        .ToList();

    private static readonly IReadOnlyList<Type> DbContexts = RootAssembly
        .GetTypes()
        .Where(t => t is { IsAbstract: false } && typeof(DbContext).IsAssignableFrom(t))
        .ToList();

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
            .Where(t => !t.Name.EndsWith("Endpoint", StringComparison.Ordinal) ||
                        t.Namespace is null ||
                        !SliceNamespaces.Contains(t.Namespace, StringComparer.Ordinal))
            .ToList();

        // Assert
        Endpoints.Should().NotBeEmpty();
        invalidTypes.Should().BeEmpty(
            "every endpoint must be named {0} and live in a {1}.{{Feature}}.{{Slice}} namespace",
            "*Endpoint",
            FeaturesNamespace);
    }

    [Fact]
    public void EndpointsWithARequest_Should_HaveAMatchingValidator()
    {
        // Arrange
        var endpointsWithRequest = Endpoints
            .Select(t => (Endpoint: t, Request: GetRequestType(t)))
            .Where(x => x.Request is not null && x.Request != typeof(EmptyRequest))
            .ToList();

        endpointsWithRequest.Select(x => x.Endpoint).Dump(_output);

        // Act
        var missingValidators = endpointsWithRequest
            .Where(x => !HasValidatorFor(x.Request!))
            .Select(x => x.Endpoint)
            .ToList();

        // Assert
        endpointsWithRequest.Should().NotBeEmpty();
        missingValidators.Should().BeEmpty(
            "every endpoint with a request must have a matching Validator<TRequest> in its slice");
    }

    [Fact]
    public void Slices_Should_NotDependOnOtherSlices()
    {
        // Assert on the discovered slices first — the per-slice loop proves nothing if this is empty
        SliceNamespaces.Should().NotBeEmpty();

        foreach (var slice in SliceNamespaces)
        {
            // Arrange
            var otherSlices = SliceNamespaces
                .Where(ns => !string.Equals(ns, slice, StringComparison.Ordinal))
                .ToArray();

            var sliceTypes = Types
                .InAssembly(RootAssembly)
                .That()
                .ResideInNamespaceMatching(ExactNamespaceOrChildPattern(slice));

            sliceTypes.GetTypes().ToList().Dump(_output);

            // Act
            var result = sliceTypes
                .ShouldNot()
                .HaveDependencyOnAny(otherSlices)
                .GetResult();

            result.DumpFailingTypes(_output);

            // Assert
            sliceTypes.GetTypes().Should().NotBeEmpty("slice {0} was discovered, so it must contain types", slice);
            result.Should().BeSuccessful();
        }
    }

    [Fact]
    public void Endpoints_Should_OnlyDependOnApplicationDbContext()
    {
        // Arrange
        var endpointsUsingADbContext = Endpoints
            .Select(t => (Endpoint: t, DbContextTypes: GetDbContextDependencies(t)))
            .Where(x => x.DbContextTypes.Count > 0)
            .ToList();

        endpointsUsingADbContext.Select(x => x.Endpoint).Dump(_output);

        // Act
        var invalidTypes = endpointsUsingADbContext
            .Where(x => x.DbContextTypes.Any(dbContext => dbContext != typeof(ApplicationDbContext)))
            .Select(x => x.Endpoint)
            .ToList();

        // Assert
        DbContexts.Should().NotBeEmpty();
        endpointsUsingADbContext.Should().NotBeEmpty();
        invalidTypes.Should().BeEmpty(
            "endpoints must take {0}, not the DbContext base type or a second DbContext",
            nameof(ApplicationDbContext));
    }

    /// <summary>
    /// The request type an endpoint handles, or <c>null</c> for endpoints that don't derive from
    /// <c>Endpoint&lt;TRequest, TResponse&gt;</c>.
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

    private static bool HasValidatorFor(Type requestType)
    {
        var validatorInterface = typeof(IValidator<>).MakeGenericType(requestType);

        return RootAssembly
            .GetTypes()
            .Any(t => t is { IsAbstract: false } && validatorInterface.IsAssignableFrom(t));
    }

    /// <summary>
    /// Every <see cref="DbContext"/>-derived type an endpoint takes as a dependency.
    /// </summary>
    /// <remarks>
    /// Fields are inspected as well as constructor parameters because a primary constructor's captured
    /// parameter — the shape every endpoint in this template uses — surfaces as a compiler-generated field.
    /// </remarks>
    private static List<Type> GetDbContextDependencies(Type endpoint)
    {
        const BindingFlags members = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;

        var dependencies = endpoint
            .GetConstructors(members)
            .SelectMany(c => c.GetParameters().Select(p => p.ParameterType))
            .Concat(endpoint.GetFields(members).Select(f => f.FieldType))
            .Concat(endpoint.GetProperties(members).Select(p => p.PropertyType));

        return dependencies
            .Where(t => typeof(DbContext).IsAssignableFrom(t))
            .Distinct()
            .ToList();
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
