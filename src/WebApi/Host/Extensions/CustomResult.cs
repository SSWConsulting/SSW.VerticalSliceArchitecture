// using Microsoft.AspNetCore.Http.HttpResults;
//
// namespace SSW.VerticalSliceArchitecture.Host.Extensions;
//
// public static class CustomResult
// {
//     public static IResult Problem(List<Error> errors)
//     {
//         if (errors.Count is 0)
//         {
//             return TypedResults.Problem();
//         }
//
//         if (errors.All(error => error.Type == ErrorType.Validation))
//         {
//             return ValidationProblem(errors);
//         }
//
//         return Problem(errors[0]);
//     }
//
//     private static ProblemHttpResult Problem(Error error)
//     {
//         var statusCode = error.Type switch
//         {
//             ErrorType.Conflict => StatusCodes.Status409Conflict,
//             ErrorType.Validation => StatusCodes.Status400BadRequest,
//             ErrorType.NotFound => StatusCodes.Status404NotFound,
//             _ => StatusCodes.Status500InternalServerError
//         };
//
//         return TypedResults.Problem(statusCode: statusCode, title: error.Description);
//     }
//
//     private static ValidationProblem ValidationProblem(List<Error> errors)
//     {
//         var validationErrors = new Dictionary<string, string[]>();
//         foreach (var e in errors)
//         {
//             if (validationErrors.Remove(e.Code, out var value))
//                 validationErrors.Add(e.Code, [.. value, e.Description]);
//             else
//                 validationErrors.Add(e.Code, [e.Description]);
//         }
//
//         return TypedResults.ValidationProblem(validationErrors, title: "One or more validation errors occurred.");
//     }
// }