using Microsoft.EntityFrameworkCore;
using VerticalSliceArchitecture.Features.Todo;

namespace VerticalSliceArchitecture.Common;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<TodoEntity> Todos { get; set; } = null!;
}