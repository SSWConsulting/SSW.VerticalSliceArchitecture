using HotChocolate.Data.Filters;
using SSW.VerticalSliceArchitecture.Domain.Heroes;

namespace SSW.VerticalSliceArchitecture.Features.Heroes;

/// <summary>
/// The fields a caller may filter heroes by.
/// </summary>
/// <remarks>
/// <c>BindFieldsExplicitly</c> makes this an allow-list. The default binds every property, which
/// puts the audit columns and the strongly typed id into the public schema and lets a client write
/// a query no index supports.
/// </remarks>
public sealed class HeroFilterType : FilterInputType<Hero>
{
    protected override void Configure(IFilterInputTypeDescriptor<Hero> descriptor)
    {
        descriptor.BindFieldsExplicitly();

        descriptor.Field(h => h.Name);
        descriptor.Field(h => h.Alias);
        descriptor.Field(h => h.PowerLevel);
    }
}
