using System.Reflection;

namespace VerticalSliceArchitecture.ArchitectureTests.Common;

public static class NetArchTestExtensions
{
    public static IEnumerable<Type> WithNestedTypes(this IEnumerable<Type> types)
    {
        return types
            .SelectMany(t => new[] { t }.Concat(GetAllNestedTypes(t)))
            .ToArray();

        static IEnumerable<Type> GetAllNestedTypes(Type type)
        {
            foreach (var nested in type.GetNestedTypes(BindingFlags.Public | BindingFlags.NonPublic))
            {
                yield return nested;

                foreach (var deeper in GetAllNestedTypes(nested))
                    yield return deeper;
            }
        }
    }
}