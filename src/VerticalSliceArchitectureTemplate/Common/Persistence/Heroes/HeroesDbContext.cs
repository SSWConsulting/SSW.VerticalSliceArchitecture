using VerticalSliceArchitectureTemplate.Common.Domain.Heroes;

// Disable as we want the partial class to be in the same namespace as the original class
// ReSharper disable once CheckNamespace
namespace VerticalSliceArchitectureTemplate.Common.Persistence;

public partial class AppDbContext
{
    public DbSet<Hero> Heroes { get; set; } = null!;
}