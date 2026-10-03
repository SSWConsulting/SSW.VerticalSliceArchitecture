using SSW.VerticalSliceArchitecture.Domain.Heroes;

namespace SSW.VerticalSliceArchitecture.Features.Heroes;

/// <summary>The GraphQL shape of the <see cref="Power"/> value object.</summary>
[ObjectType<Power>]
public static partial class PowerType
{
    static partial void Configure(IObjectTypeDescriptor<Power> descriptor)
    {
        descriptor.BindFieldsExplicitly();

        descriptor.Field(p => p.Name);
        descriptor.Field(p => p.PowerLevel);
    }
}
