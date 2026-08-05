using System.Linq.Expressions;

// Ardalis ships two SpecificationEvaluator types and the global using of Ardalis.Specification already
// pulls in the in-memory one. Only the EF Core evaluator knows how to translate Include, so name it.
using SpecificationEvaluator = Ardalis.Specification.EntityFrameworkCore.SpecificationEvaluator;

namespace SSW.VerticalSliceArchitecture.Common.Pagination;

public static class PagedQueryableExtensions
{
    /// <summary>
    /// Runs a paged specification and its matching count, and projects the page into <typeparamref name="TResult"/>.
    /// </summary>
    /// <remarks>
    /// Two queries, one specification. The count is evaluated with <c>evaluateCriteriaOnly</c>, which keeps
    /// the spec's filters but drops its <c>Skip</c>/<c>Take</c> — so <see cref="PagedList{T}.TotalCount"/>
    /// describes the whole result set, and adding a filter to the spec later can't leave the two out of step.
    /// <para>
    /// The envelope's page and page size are read back off the specification rather than taken as a second
    /// argument. Passing the window twice would let the body describe a page the query never fetched, and
    /// nothing — not the compiler, not the architecture tests — could catch the two drifting apart.
    /// </para>
    /// </remarks>
    /// <exception cref="ArgumentException">
    /// <paramref name="specification"/> carries no <c>Skip</c>/<c>Take</c>, so it would return the whole
    /// table under an envelope claiming otherwise. Build it with an <c>{Aggregate}Spec.Paged(...)</c> factory.
    /// </exception>
    public static async Task<PagedList<TResult>> ToPagedListAsync<TEntity, TResult>(
        this IQueryable<TEntity> source,
        ISpecification<TEntity> specification,
        Expression<Func<TEntity, TResult>> projection,
        CancellationToken ct)
        where TEntity : class
    {
        ThrowIfNull(source);
        ThrowIfNull(specification);
        ThrowIfNull(projection);

        var paging = PagingParams.FromSpecification(specification);

        var totalCount = await SpecificationEvaluator.Default
            .GetQuery(source, specification, evaluateCriteriaOnly: true)
            .CountAsync(ct);

        var items = await SpecificationEvaluator.Default
            .GetQuery(source, specification)
            .Select(projection)
            .ToListAsync(ct);

        return new PagedList<TResult>(items, paging.Page, paging.PageSize, totalCount);
    }
}
