namespace SSW.VerticalSliceArchitecture.Common.GraphQL.Errors;

/// <summary>Something the caller asked for does not exist.</summary>
public sealed class NotFoundError(NotFoundException exception)
{
    public string Code { get; } = exception.Error.Code;

    public string Message { get; } = exception.Error.Description;
}
