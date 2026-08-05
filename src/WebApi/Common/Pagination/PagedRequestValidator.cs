namespace SSW.VerticalSliceArchitecture.Common.Pagination;

/// <summary>
/// Validates the sort inputs of a <see cref="PagedRequest"/> against an aggregate's allow-list.
/// </summary>
/// <remarks>
/// No page or page-size rules: those are clamped by <see cref="PagingParams.From"/>, so there is nothing
/// left to reject. Sort inputs are the opposite — an unknown column has no sensible interpretation, and
/// answering it with a 400 that lists the allowed columns beats guessing.
/// <para>
/// Each slice needs its own concrete subclass in its own namespace: FastEndpoints binds validators by
/// request type, and the architecture tests require a <c>Validator&lt;TRequest&gt;</c> alongside the endpoint.
/// </para>
/// </remarks>
public abstract class PagedRequestValidator<TRequest, TEntity> : Validator<TRequest>
    where TRequest : PagedRequest
{
    protected PagedRequestValidator(SortColumnMap<TEntity> sortColumns)
    {
        ThrowIfNull(sortColumns);

        RuleFor(x => x.SortBy)
            .Must(sortColumns.IsAllowed)
            .WithMessage($"'{{PropertyValue}}' is not a sortable column. Allowed values: {string.Join(", ", sortColumns.AllowedColumns)}.");

        RuleFor(x => x.SortDirection)
            .Must(SortDirections.IsAllowed)
            .WithMessage($"'{{PropertyValue}}' is not a valid sort direction. Allowed values: {string.Join(", ", SortDirections.Allowed)}.");
    }
}
