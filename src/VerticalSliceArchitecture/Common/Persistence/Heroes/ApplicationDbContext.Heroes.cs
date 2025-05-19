using SSW.VerticalSliceArchitecture.Common.Domain.Heroes;

// Preserve the namespace across partial classes
// ReSharper disable once CheckNamespace
namespace SSW.VerticalSliceArchitecture.Common.Persistence;

public partial class ApplicationDbContext
{
    public DbSet<Hero> Heroes => AggregateRootSet<Hero>();
}