namespace SSW.VerticalSliceArchitecture.Common.GraphQL.Errors;

/// <summary>The request is well formed, but the aggregate's state does not allow it.</summary>
public sealed class ConflictError(ConflictException exception)
{
    public string Code { get; } = exception.Error.Code;

    public string Message { get; } = exception.Error.Description;
}
