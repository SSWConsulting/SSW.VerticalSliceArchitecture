# Query slice template

One file in `src/WebApi/Features/{Feature}/{UseCase}/`, plus the allow-lists at feature level for a
list query.

Reference: `src/WebApi/Features/Heroes/GetAllHeroes/` and `src/WebApi/Features/Teams/GetTeam/`.

---

## Single item

`{UseCase}Query.cs`

```csharp
using SSW.VerticalSliceArchitecture.Domain.{Aggregate};

namespace SSW.VerticalSliceArchitecture.Features.{Feature}.{UseCase};

[QueryType]
public static partial class {UseCase}Query
{
    /// <summary>One {aggregate} by id, or null when no {aggregate} has that id.</summary>
    public static async Task<{Aggregate}?> Get{Aggregate}ByIdAsync(
        {Aggregate}Id {aggregate}Id,
        ApplicationDbContext dbContext,
        CancellationToken cancellationToken)
        => await dbContext.{Aggregates}
            .FirstOrDefaultAsync(x => x.Id == {aggregate}Id, cancellationToken);
}
```

- The field is `{aggregate}ById`: the method name with `Get` and `Async` removed.
- A nullable return is the right answer for "not found". It is not an error, and a client that asked
  for a missing thing gets `null` with a successful request.
- No `Include`. Related collections are resolved by their own DataLoaders on the object type, and
  only when the client asks for them.
- Plain arguments, not an input record. Inputs are a mutation convention.

---

## List

`{UseCase}Query.cs`

```csharp
using SSW.VerticalSliceArchitecture.Domain.{Aggregate};

namespace SSW.VerticalSliceArchitecture.Features.{Feature}.{UseCase};

[QueryType]
public static partial class {UseCase}Query
{
    /// <summary>A page of {aggregates}, as a Relay connection.</summary>
    [UsePaging]
    [UseFiltering<{Aggregate}FilterType>]
    [UseSorting<{Aggregate}SortType>]
    public static IQueryable<{Aggregate}> Get{Aggregates}(ApplicationDbContext dbContext) =>
        dbContext.{Aggregates};
}
```

Four things about this method are load-bearing.

- **Attribute order.** Paging runs last, so filtering and sorting shape the query before it is
  sliced. Reversing them pages the whole table and then filters the page.
- **No `OrderBy`.** The sorting middleware steps aside when the resolver already ordered the
  queryable, so an `OrderBy` here silently disables the `order` argument. Clients that page through
  the whole list should pass `order`, which is also what makes the cursors stable.
- **Return `IQueryable`, not a list.** Everything above works by rewriting the expression tree; a
  materialised list means filtering and paging happen in memory over the whole table.
- **Typed filter and sort arguments.** `[UseFiltering]` with no type argument binds every property.

The connection shape, the default page size of 10, the maximum of 50 and `totalCount` all come from
`ModifyPagingOptions` in `Host/Extensions/GraphQlExt.cs`. Don't set them per field unless this field
genuinely differs.

---

## Filter and sort allow-lists

`src/WebApi/Features/{Feature}/{Aggregate}FilterType.cs`

```csharp
using HotChocolate.Data.Filters;
using SSW.VerticalSliceArchitecture.Domain.{Aggregate};

namespace SSW.VerticalSliceArchitecture.Features.{Feature};

public sealed class {Aggregate}FilterType : FilterInputType<{Aggregate}>
{
    protected override void Configure(IFilterInputTypeDescriptor<{Aggregate}> descriptor)
    {
        descriptor.BindFieldsExplicitly();

        descriptor.Field(x => x.Name);
    }
}
```

`src/WebApi/Features/{Feature}/{Aggregate}SortType.cs`

```csharp
using HotChocolate.Data.Sorting;
using SSW.VerticalSliceArchitecture.Domain.{Aggregate};

namespace SSW.VerticalSliceArchitecture.Features.{Feature};

public sealed class {Aggregate}SortType : SortInputType<{Aggregate}>
{
    protected override void Configure(ISortInputTypeDescriptor<{Aggregate}> descriptor)
    {
        descriptor.BindFieldsExplicitly();

        descriptor.Field(x => x.Name);
    }
}
```

`BindFieldsExplicitly` is what makes these allow-lists. The default binds every property, which
publishes the audit columns and the strongly typed id, and lets a client write a query no index
supports. Add a field here deliberately, one at a time.
