using SSW.VerticalSliceArchitecture.Common.Domain.Entities;

// Preserve the namespace across partial classes
// ReSharper disable once CheckNamespace
namespace SSW.VerticalSliceArchitecture.Common.Persistence;

public partial class ApplicationDbContext
{
    public DbSet<EntityName> Entities => AggregateRootSet<EntityName>();
}