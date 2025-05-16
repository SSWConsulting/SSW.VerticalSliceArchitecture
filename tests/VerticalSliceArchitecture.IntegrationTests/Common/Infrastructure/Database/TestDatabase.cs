using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Respawn;
using System.Data.Common;
using VerticalSliceArchitectureTemplate.Common.Persistence;

namespace VerticalSliceArchitectureTemplate.IntegrationTests.Common.Infrastructure.Database;

/// <summary>
/// Manages the schema and data for the database container
/// </summary>
public class TestDatabase : IAsyncDisposable
{
    private readonly SqlServerContainer _sqlServer = new();
    private Respawner _checkpoint = null!;
    private string _connectionString = null!;

    /// <summary>
    /// Create and seed a database
    /// </summary>
    public async Task InitializeAsync()
    {
        await _sqlServer.InitializeAsync();

        var builder = new SqlConnectionStringBuilder(_sqlServer.Connection!.ConnectionString)
        {
            InitialCatalog = "CleanArchitecture-IntegrationTests"
        };

        _connectionString = builder.ConnectionString;

        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseSqlServer(_connectionString)
            .Options;

        using var dbContext = new ApplicationDbContext(options);
        await dbContext.Database.MigrateAsync();

        _checkpoint = await Respawner.CreateAsync(_connectionString,
            new RespawnerOptions { TablesToIgnore = ["__EFMigrationsHistory"] });
    }

    public DbConnection DbConnection => new SqlConnection(_connectionString);

    public async Task ResetAsync()
    {
        await _checkpoint.ResetAsync(_connectionString);
    }

    public async ValueTask DisposeAsync()
    {
        await _sqlServer.DisposeAsync();
    }
}