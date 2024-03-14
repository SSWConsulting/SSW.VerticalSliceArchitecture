namespace VerticalSliceArchitectureTemplate.Common.Features;

public static class OpenApiConvenienceExtensions
{
    /// <summary>
    ///     Used for GET endpoints that return one or more items.
    /// </summary>
    public static RouteHandlerBuilder ProducesGet<T>(this RouteHandlerBuilder builder) =>
        builder
            .Produces<T>()
            .Produces(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status500InternalServerError);

    public static RouteHandlerBuilder MapGetWithOpenApi<T>(
        this IEndpointRouteBuilder endpoints,
        string pattern,
        Delegate handler) =>
        endpoints.MapGet(pattern, handler)
            .ProducesGet<T>();

    /// <summary>
    ///     Used for POST endpoints that creates a single item.
    /// </summary>
    public static RouteHandlerBuilder ProducesPost(this RouteHandlerBuilder builder) =>
        builder
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status500InternalServerError);

    public static RouteHandlerBuilder MapPostWithCreatedOpenApi(
        this IEndpointRouteBuilder endpoints,
        string pattern,
        Delegate handler) =>
        endpoints.MapPost(pattern, handler)
            .ProducesPost()
            .Produces(StatusCodes.Status201Created);

    public static RouteHandlerBuilder MapPostWithUpdatedOpenApi(
        this IEndpointRouteBuilder endpoints,
        string pattern,
        Delegate handler) =>
        endpoints.MapPost(pattern, handler)
            .ProducesPost()
            .Produces(StatusCodes.Status204NoContent);

    /// <summary>
    ///     Used for PUT endpoints that updates a single item.
    /// </summary>
    public static RouteHandlerBuilder ProducesPut(this RouteHandlerBuilder builder) =>
        builder
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status404NotFound)
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status500InternalServerError);

    public static RouteHandlerBuilder MapPutWithOpenApi(
        this IEndpointRouteBuilder endpoints,
        string pattern,
        Delegate handler) =>
        endpoints.MapPut(pattern, handler)
            .ProducesPut();

    /// <summary>
    ///     Used for DELETE endpoints that deletes a single item.
    /// </summary>
    public static RouteHandlerBuilder ProducesDelete(this RouteHandlerBuilder builder) =>
        builder
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status404NotFound)
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status500InternalServerError);

    public static RouteHandlerBuilder MapDeleteWithOpenApi(
        this IEndpointRouteBuilder endpoints,
        string pattern,
        Delegate handler) =>
        endpoints.MapDelete(pattern, handler)
            .ProducesDelete();
}