using HotChocolate.Data.Sorting;
using SSW.VerticalSliceArchitecture.Domain.Heroes;

namespace SSW.VerticalSliceArchitecture.Features.Heroes;

/// <summary>The fields a caller may sort heroes by. An allow-list, as with the filter type.</summary>
public sealed class HeroSortType : SortInputType<Hero>
{
    protected override void Configure(ISortInputTypeDescriptor<Hero> descriptor)
    {
        descriptor.BindFieldsExplicitly();

        descriptor.Field(h => h.Name);
        descriptor.Field(h => h.Alias);
        descriptor.Field(h => h.PowerLevel);
    }
}
