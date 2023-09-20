namespace VerticalSliceArchitecture.Features.Todo;

public static class DependencyInjection
{
    public static void AddTodoFeature(this IServiceCollection services)
    {
        services.AddScoped<TodoRepository>();
    }
}