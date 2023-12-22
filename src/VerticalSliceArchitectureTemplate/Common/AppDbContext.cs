using Microsoft.EntityFrameworkCore;
using VerticalSliceArchitectureTemplate.Features.Todos;

namespace VerticalSliceArchitectureTemplate.Common;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<Todo> Todos { get; set; } = null!;
}