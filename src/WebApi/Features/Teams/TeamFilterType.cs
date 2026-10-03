using HotChocolate.Data.Filters;
using SSW.VerticalSliceArchitecture.Domain.Teams;

namespace SSW.VerticalSliceArchitecture.Features.Teams;

/// <summary>The fields a caller may filter teams by. An allow-list — see <see cref="Heroes.HeroFilterType"/>.</summary>
public sealed class TeamFilterType : FilterInputType<Team>
{
    protected override void Configure(IFilterInputTypeDescriptor<Team> descriptor)
    {
        descriptor.BindFieldsExplicitly();

        descriptor.Field(t => t.Name);
        descriptor.Field(t => t.TotalPowerLevel);
        descriptor.Field(t => t.Status);
    }
}
