namespace SSW.VerticalSliceArchitecture.Common.GraphQL.Errors;

/// <summary>One or more input fields failed validation.</summary>
/// <remarks>
/// Projects FluentValidation's failures into two plain strings. Exposing
/// <c>ValidationException</c> itself would put <c>Severity</c>, <c>ErrorCode</c> and a dictionary
/// of message placeholders into the public schema.
/// </remarks>
public sealed class InputValidationError(ValidationException exception)
{
    public string Message { get; } = "One or more input fields are invalid.";

    public IReadOnlyList<InputValidationFailure> Failures { get; } = exception.Errors
        .Select(failure => new InputValidationFailure(failure.PropertyName, failure.ErrorMessage))
        .ToList();
}

public sealed record InputValidationFailure(string Field, string Message);
