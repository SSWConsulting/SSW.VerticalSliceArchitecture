using VerticalSliceArchitectureTemplate.Common.Services;

namespace VerticalSliceArchitectureTemplate.Features.Todos;

public sealed class TodoFeature : IFeature
{
    public static void ConfigureServices(IServiceCollection services, IConfiguration config)
    {
        // SM: Is this where we should do these things?
        services.AddHttpContextAccessor();
        services.AddScoped<CurrentUserService>();
    }
}