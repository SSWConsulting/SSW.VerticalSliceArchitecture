namespace SSW.VerticalSliceArchitecture.Common.GraphQL.Errors;

/// <summary>
/// Carries an <see cref="ErrorOr.Error"/> out of a resolver so that HotChocolate's mutation
/// conventions can turn it into a typed entry in the mutation's error union.
/// </summary>
/// <remarks>
/// The domain returns <c>ErrorOr</c> and never throws for a business rule. The throw happens here,
/// at the edge, because an error declared with <c>[Error&lt;T&gt;]</c> only reaches the payload when the
/// resolver throws it. One exception type per GraphQL error type: the conventions pick the error
/// class by the exception its constructor takes, so two error classes cannot share an exception.
/// </remarks>
public abstract class DomainException(Error error) : Exception(error.Description)
{
    /// <remarks>
    /// Internal on purpose. A public property would put <c>ErrorOr.Error</c> in front of the schema
    /// builder, which then tries to register an object type called <c>Error</c> and collides with the
    /// error interface the mutation conventions already own.
    /// </remarks>
    internal Error Error { get; } = error;
}

public sealed class NotFoundException(Error error) : DomainException(error);

public sealed class ConflictException(Error error) : DomainException(error);
