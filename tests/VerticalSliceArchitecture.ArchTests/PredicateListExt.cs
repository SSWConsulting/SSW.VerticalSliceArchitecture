using NetArchTest.Rules;

namespace VerticalSliceArchitecture.ArchTests;

public static class PredicateListExt
{
    public static string?[] GetNames(this PredicateList list)
    {
        return list.GetTypes().Select(x => x.FullName).ToArray();
    }
}