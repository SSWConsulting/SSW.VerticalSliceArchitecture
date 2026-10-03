namespace SSW.VerticalSliceArchitecture.Common.GraphQL.Errors;

public static class ErrorOrExtensions
{
    /// <summary>
    /// Throws the exception that matches the first error, so the mutation's error union receives it.
    /// </summary>
    /// <remarks>
    /// Only the first error is thrown. GraphQL's mutation conventions model one error per exception,
    /// and the domain's operations stop at the first broken rule anyway.
    /// </remarks>
    public static void ThrowOnError(this IErrorOr result)
    {
        ThrowIfNull(result);

        if (!result.IsError || result.Errors is not { Count: > 0 } errors)
            return;

        var error = errors[0];

        throw error.Type switch
        {
            ErrorType.NotFound => new NotFoundException(error),
            ErrorType.Conflict => new ConflictException(error),

            // An unmapped error type would otherwise leave the schema silently, as a generic
            // top-level error with no code. Failing loudly says which mapping is missing.
            _ => new NotSupportedException(
                $"No GraphQL error is mapped for {nameof(ErrorType)}.{error.Type} (error code '{error.Code}').")
        };
    }
}
