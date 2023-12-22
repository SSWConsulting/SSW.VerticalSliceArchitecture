namespace VerticalSliceArchitectureTemplate.Features.Todos;

public sealed class TodoModule : IModule
{
    public static void ConfigureServices(IServiceCollection services)
    {
        services.AddScoped<ITodoRepository, TodoRepository>();
    }
}