using FastEndpoints.Swagger;
using SSW.VerticalSliceArchitecture.Common.Interfaces;
using SSW.VerticalSliceArchitecture.Common.Services;

namespace SSW.VerticalSliceArchitecture.Host;

public static class DependencyInjection
{
    public static void AddWebApi(this IHostApplicationBuilder builder)
    {
        var services = builder.Services;
        
        services.AddHttpContextAccessor();
        
        services.AddScoped<ICurrentUserService, CurrentUserService>();
        
        services.AddOpenApi();

        services.AddFastEndpoints();

        builder.Services.SwaggerDocument();
    }
}