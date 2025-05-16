namespace VerticalSliceArchitecture.ArchitectureTests.Common;

public static class TypeExtensions
{
    public static void Dump(this IEnumerable<Type> types, ITestOutputHelper outputHelper)
    {
        if (!types.Any())
            outputHelper.WriteLine("No types found.");

        foreach (var type in types)
        {
            if (type.FullName is null)
                continue;

            outputHelper.WriteLine(type.FullName);
        }
    }
}