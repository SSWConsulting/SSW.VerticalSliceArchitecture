namespace VerticalSliceArchitecture.Features.Todos;

public static class DependencyInjection
{
    public static void AddTodoFeature(this IServiceCollection services)
    {
        services.AddScoped<ITodoRepository, TodoRepository>();
    }
}