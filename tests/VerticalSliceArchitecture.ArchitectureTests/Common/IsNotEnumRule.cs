using Mono.Cecil;

namespace VerticalSliceArchitecture.ArchTests.Common;

public class IsNotEnumRule : ICustomRule
{
    public bool MeetsRule(TypeDefinition type) => !type.IsEnum;
}