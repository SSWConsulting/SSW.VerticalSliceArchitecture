using Mono.Cecil;

namespace SSW.VerticalSliceArchitecture.ArchitectureTests.Common;

public class IsNotEnumRule : ICustomRule
{
    public bool MeetsRule(TypeDefinition type) => !type.IsEnum;
}