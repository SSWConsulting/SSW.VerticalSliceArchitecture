using System.Reflection;

namespace SSW.VerticalSliceArchitecture.ArchitectureTests.Common;

public abstract class TestBase
{
    protected const string DomainAssemblyName = "Domain";

    protected const string FeaturesNamespace = "SSW.VerticalSliceArchitecture.Features";

    protected static readonly Assembly RootAssembly = typeof(SSW.VerticalSliceArchitecture.Program).Assembly;

    /// <remarks>
    /// Lazy so that a type-load failure surfaces in the test that actually reflects over the assembly,
    /// rather than as a <see cref="TypeInitializationException"/> that takes every derived test class
    /// down with it — including ones that never needed loadable types.
    /// </remarks>
    private static readonly Lazy<IReadOnlyList<string>> LazySliceNamespaces = new(() => RootAssembly
        .GetTypes()
        .Select(t => t.Namespace)
        .Where(IsSliceNamespace)
        .OfType<string>()
        .Distinct(StringComparer.Ordinal)
        .Order(StringComparer.Ordinal)
        .ToList());

    /// <summary>
    /// Every slice namespace in the app, e.g. <c>SSW.VerticalSliceArchitecture.Features.Heroes.CreateHero</c>.
    /// </summary>
    /// <remarks>
    /// Derived from the assembly rather than hard-coded, so a new slice is covered by the architecture
    /// rules the moment it is added. Because it is derived, it says nothing about whether any given
    /// namespace <em>should</em> exist — use <see cref="IsSliceNamespace"/> to assert shape.
    /// </remarks>
    protected static IReadOnlyList<string> SliceNamespaces => LazySliceNamespaces.Value;

    /// <summary>
    /// Whether a namespace has the shape of a slice: a feature segment and a use-case segment below
    /// <see cref="FeaturesNamespace"/>, and nothing deeper.
    /// </summary>
    /// <remarks>
    /// Excludes feature-level types such as <c>HeroesGroup</c> and <c>HeroesFeature</c>, which sit one
    /// segment up.
    /// </remarks>
    protected static bool IsSliceNamespace(string? ns) =>
        ns is not null &&
        ns.StartsWith(FeaturesNamespace + ".", StringComparison.Ordinal) &&
        ns[(FeaturesNamespace.Length + 1)..].Count(c => c == '.') == 1;
}
