using HotChocolate.Data.Sorting;
using SSW.VerticalSliceArchitecture.Domain.Teams;

namespace SSW.VerticalSliceArchitecture.Features.Teams;

/// <summary>The fields a caller may sort teams by.</summary>
public sealed class TeamSortType : SortInputType<Team>
{
    protected override void Configure(ISortInputTypeDescriptor<Team> descriptor)
    {
        descriptor.BindFieldsExplicitly();

        descriptor.Field(t => t.Name);
        descriptor.Field(t => t.TotalPowerLevel);
    }
}
