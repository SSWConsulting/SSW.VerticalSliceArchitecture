using SSW.VerticalSliceArchitecture.Domain.Teams;

namespace SSW.VerticalSliceArchitecture.Features.Teams;

/// <summary>The GraphQL shape of the <see cref="Mission"/> entity.</summary>
[ObjectType<Mission>]
public static partial class MissionType
{
    static partial void Configure(IObjectTypeDescriptor<Mission> descriptor)
    {
        descriptor.BindFieldsExplicitly();

        descriptor.Field(m => m.Id);
        descriptor.Field(m => m.Description);
        descriptor.Field(m => m.Status);
        descriptor.Field(m => m.CreatedAt);
        descriptor.Field(m => m.UpdatedAt);
    }
}
