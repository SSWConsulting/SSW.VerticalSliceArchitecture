using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using SSW.VerticalSliceArchitecture.Common.Interfaces;
using System.Data.Common;

namespace SSW.VerticalSliceArchitecture.IntegrationTests.Common.Infrastructure.Web;

/// <summary>
/// Host builder (services, DI and configuration) for integration tests
/// </summary>
public class WebApiTestFactory : WebApplicationFactory<IWebApiMarker>
{
    private readonly DbConnection _dbConnection;

    public WebApiTestFactory(DbConnection dbConnection)
    {
        _dbConnection = dbConnection;
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        // Redirect application logging to test output
        builder.ConfigureLogging(_ =>
        {
            // x.ClearProviders();
            // x.AddFilter(level => level >= LogLevel.Information);
            // TODO: This doesnt work anymore - need to investigate
            // x.Services.AddSingleton<ILoggerProvider>(new XUnitLoggerProvider(Output));
        });

        builder.UseSetting("ConnectionStrings:AppDb", _dbConnection.ConnectionString);
    }
}