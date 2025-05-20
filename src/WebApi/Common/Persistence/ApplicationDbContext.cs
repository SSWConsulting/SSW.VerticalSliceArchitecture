using SSW.VerticalSliceArchitecture.Common.Domain.Base.Interfaces;
using System.Reflection;

namespace SSW.VerticalSliceArchitecture.Common.Persistence;

public partial class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : DbContext(options)
{
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());

        base.OnModelCreating(modelBuilder);
    }

    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
        base.ConfigureConventions(configurationBuilder);

        configurationBuilder.RegisterAllInVogenEfCoreConverters();
    }
    
    private DbSet<T> AggregateRootSet<T>() where T : class, IAggregateRoot => Set<T>();
}