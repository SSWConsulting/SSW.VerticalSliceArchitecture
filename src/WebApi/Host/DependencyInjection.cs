using SSW.VerticalSliceArchitecture.Common.Events;
using SSW.VerticalSliceArchitecture.Common.Interfaces;
using SSW.VerticalSliceArchitecture.Common.Services;
using SSW.VerticalSliceArchitecture.Host.Extensions;

namespace SSW.VerticalSliceArchitecture.Host;

public static class DependencyInjection
{
    public static void AddWebApi(this IHostApplicationBuilder builder)
    {
        var services = builder.Services;

        services.AddHttpContextAccessor();

        services.AddScoped<ICurrentUserService, CurrentUserService>();

        builder.AddCustomGraphQL();
    }

    public static void AddApplication(this IHostApplicationBuilder builder)
    {
        var applicationAssembly = typeof(DependencyInjection).Assembly;
        var services = builder.Services;

        services.AddValidatorsFromAssembly(applicationAssembly, includeInternalTypes: true);
        services.AddDomainEvents(applicationAssembly);
    }
}
