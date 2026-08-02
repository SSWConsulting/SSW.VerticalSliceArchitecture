using System.Reflection;

namespace SSW.VerticalSliceArchitecture.ArchitectureTests.Common;

public abstract class TestBase
{
    protected const string DomainAssemblyName = "Domain";

    protected const string FeaturesNamespace = "SSW.VerticalSliceArchitecture.Features";

    protected static readonly Assembly RootAssembly = typeof(SSW.VerticalSliceArchitecture.Program).Assembly;

    /// <summary>
    /// Every slice namespace in the app, e.g. <c>SSW.VerticalSliceArchitecture.Features.Heroes.CreateHero</c>.
    /// </summary>
    /// <remarks>
    /// Derived from the assembly rather than hard-coded, so a new slice is covered by the architecture
    /// rules the moment it is added. A slice namespace sits exactly two segments below
    /// <see cref="FeaturesNamespace"/> — feature, then use case — which excludes feature-level types
    /// such as <c>HeroesGroup</c> and <c>HeroesFeature</c>.
    /// </remarks>
    protected static readonly IReadOnlyList<string> SliceNamespaces = RootAssembly
        .GetTypes()
        .Select(t => t.Namespace)
        .Where(ns => ns is not null && ns.StartsWith(FeaturesNamespace + ".", StringComparison.Ordinal))
        .Select(ns => ns!)
        .Where(ns => ns[(FeaturesNamespace.Length + 1)..].Count(c => c == '.') == 1)
        .Distinct(StringComparer.Ordinal)
        .Order(StringComparer.Ordinal)
        .ToList();
}
