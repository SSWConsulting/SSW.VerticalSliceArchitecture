// using FastEndpoints;
//
// namespace SSW.VerticalSliceArchitecture.Common.FastEndpoints;
//
// /// <summary>
// /// Base endpoint that provides ErrorOr support
// /// </summary>
// public abstract class EndpointBase<TRequest, TResponse> : Endpoint<TRequest, TResponse>
//     where TRequest : notnull
// {
//     /// <summary>
//     /// Send a response based on an ErrorOr result
//     /// </summary>
//     protected async Task SendErrorOrAsync<T>(ErrorOr<T> result, Func<T, Task> onSuccess, CancellationToken ct = default)
//     {
//         if (result.IsError)
//         {
//             await SendProblemsAsync(result.Errors, ct);
//         }
//         else
//         {
//             await onSuccess(result.Value);
//         }
//     }
//
//     /// <summary>
//     /// Send problem details for ErrorOr errors
//     /// </summary>
//     protected async Task SendProblemsAsync(List<Error> errors, CancellationToken ct = default)
//     {
//         var firstError = errors.FirstOrDefault();
//
//         var statusCode = firstError.Type switch
//         {
//             ErrorType.Validation => StatusCodes.Status400BadRequest,
//             ErrorType.NotFound => StatusCodes.Status404NotFound,
//             ErrorType.Conflict => StatusCodes.Status409Conflict,
//             ErrorType.Unauthorized => StatusCodes.Status401Unauthorized,
//             ErrorType.Forbidden => StatusCodes.Status403Forbidden,
//             _ => StatusCodes.Status500InternalServerError
//         };
//
//         HttpContext.Response.StatusCode = statusCode;
//         await HttpContext.Response.WriteAsJsonAsync(new
//         {
//             errors = errors.Select(e => new { e.Code, e.Description }).ToList()
//         }, ct);
//     }
// }
//
// /// <summary>
// /// Base endpoint without request body - inherits from Endpoint with EmptyRequest
// /// </summary>
// public abstract class EndpointBase<TResponse> : Endpoint<EmptyRequest, TResponse>
// {
//     /// <summary>
//     /// Send a response based on an ErrorOr result
//     /// </summary>
//     protected async Task SendErrorOrAsync<T>(ErrorOr<T> result, Func<T, Task> onSuccess, CancellationToken ct = default)
//     {
//         if (result.IsError)
//         {
//             await SendProblemsAsync(result.Errors, ct);
//         }
//         else
//         {
//             await onSuccess(result.Value);
//         }
//     }
//
//     /// <summary>
//     /// Send problem details for ErrorOr errors
//     /// </summary>
//     protected async Task SendProblemsAsync(List<Error> errors, CancellationToken ct = default)
//     {
//         var firstError = errors.FirstOrDefault();
//
//         var statusCode = firstError.Type switch
//         {
//             ErrorType.Validation => StatusCodes.Status400BadRequest,
//             ErrorType.NotFound => StatusCodes.Status404NotFound,
//             ErrorType.Conflict => StatusCodes.Status409Conflict,
//             ErrorType.Unauthorized => StatusCodes.Status401Unauthorized,
//             ErrorType.Forbidden => StatusCodes.Status403Forbidden,
//             _ => StatusCodes.Status500InternalServerError
//         };
//
//         HttpContext.Response.StatusCode = statusCode;
//         await HttpContext.Response.WriteAsJsonAsync(new
//         {
//             errors = errors.Select(e => new { e.Code, e.Description }).ToList()
//         }, ct);
//     }
// }