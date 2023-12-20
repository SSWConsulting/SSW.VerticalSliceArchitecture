using Microsoft.EntityFrameworkCore;
using VerticalSliceArchitecture.Features.Todos;

namespace VerticalSliceArchitecture.Common;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<Todo> Todos { get; set; } = null!;
}