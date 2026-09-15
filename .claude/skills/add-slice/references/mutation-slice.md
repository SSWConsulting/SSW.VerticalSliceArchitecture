# Mutation slice template

Three files, one type each, in `src/WebApi/Features/{Feature}/{UseCase}/`.

Reference: `src/WebApi/Features/Heroes/CreateHero/` and `src/WebApi/Features/Teams/ExecuteMission/`.

Types available without a `using` (from `src/WebApi/GlobalUsings.cs`): `Vogen`, `ErrorOr`,
`Ardalis.Specification`, `FluentValidation`, `Microsoft.EntityFrameworkCore`, the
`SSW.VerticalSliceArchitecture.Common.Persistence` namespace, and HotChocolate's own implicit
usings, which cover `[MutationType]`, `[Error<T>]`, `[UsePaging]` and `IObjectTypeDescriptor`.

---

## Input

`{UseCase}Input.cs`

```csharp
namespace SSW.VerticalSliceArchitecture.Features.{Feature}.{UseCase};

public sealed record {UseCase}Input(
    {Aggregate}Id {Aggregate}Id,
    string Name);
```

One input record per mutation, named `{UseCase}Input`. HotChocolate's mutation conventions turn it
into the `input` argument and name the GraphQL type after it.

A nested collection needs its own record, named for this slice:

```csharp
public sealed record {UseCase}PowerInput(string Name, int PowerLevel);
```

Two slices cannot share one, and two CLR types cannot share a GraphQL name — so
`CreateHeroPowerInput` and `UpdateHeroPowerInput` are separate types on purpose.

---

## Validator

`{UseCase}InputValidator.cs`

```csharp
using SSW.VerticalSliceArchitecture.Common.Validation;
using SSW.VerticalSliceArchitecture.Domain.{Aggregate};

namespace SSW.VerticalSliceArchitecture.Features.{Feature}.{UseCase};

public class {UseCase}InputValidator : AbstractValidator<{UseCase}Input>
{
    public {UseCase}InputValidator()
    {
        RuleFor(v => v.{Aggregate}Id)
            .NotEmptyId(id => id.Value);

        RuleFor(v => v.Name)
            .NotEmpty()
            .MaximumLength({Aggregate}.NameMaxLength);
    }
}
```

`NotEmptyId` rather than `NotEmpty` for a strongly typed ID: `NotEmpty` compares against
`default(TId)`, and an id built from `Guid.Empty` is not that — it is an initialised value object
holding a meaningless value, so the rule would pass.

Length rules read the domain constant, never a literal, so the rule follows the limit when it moves.
Where the domain setter throws on bad input, the validator is what turns that into a typed error
instead of a faulted resolver.

---

## Resolver

`{UseCase}Mutation.cs`

```csharp
using SSW.VerticalSliceArchitecture.Common.GraphQL.Errors;
using SSW.VerticalSliceArchitecture.Domain.{Aggregate};

namespace SSW.VerticalSliceArchitecture.Features.{Feature}.{UseCase};

[MutationType]
public static partial class {UseCase}Mutation
{
    /// <summary>One sentence, which becomes the field's description in the schema.</summary>
    [Error<InputValidationError>]
    [Error<NotFoundError>]
    [Error<ConflictError>]
    public static async Task<{Aggregate}> {UseCase}Async(
        {UseCase}Input input,
        IValidator<{UseCase}Input> validator,
        ApplicationDbContext dbContext,
        CancellationToken cancellationToken)
    {
        await validator.ValidateAndThrowAsync(input, cancellationToken);

        var {aggregate} = await dbContext.{Aggregates}
                              .WithSpecification({Aggregate}Spec.ById(input.{Aggregate}Id))
                              .FirstOrDefaultAsync(cancellationToken)
                          ?? throw new NotFoundException({Aggregate}Errors.NotFound);

        {aggregate}.{Behaviour}(input.Name).ThrowOnError();

        await dbContext.SaveChangesAsync(cancellationToken);

        return {aggregate};
    }
}
```

What each part is doing:

- `static partial class` plus `[MutationType]` is what makes the source generator emit the field.
  Neither is optional, and missing either fails silently.
- The mutation conventions wrap the method: the parameter becomes the `input` argument, the return
  value becomes `{UseCase}Payload.{aggregate}`, and each `[Error<T>]` adds a member to
  `{UseCase}Error`. Services in the signature stay out of the schema.
- Declare only the errors this mutation can actually produce. A mutation with no lookup has no
  `NotFoundError`; one with no `ErrorOr` call has no `ConflictError`.
- Return the entity, not a DTO. The client picks the fields it wants from the object type.
- `ThrowOnError()` maps the domain's `ErrorOr` failure onto the right exception, and therefore onto
  the right member of the error union.

### Creating rather than loading

```csharp
[MutationType]
public static partial class Create{Aggregate}Mutation
{
    [Error<InputValidationError>]
    public static async Task<{Aggregate}> Create{Aggregate}Async(
        Create{Aggregate}Input input,
        IValidator<Create{Aggregate}Input> validator,
        ApplicationDbContext dbContext,
        CancellationToken cancellationToken)
    {
        await validator.ValidateAndThrowAsync(input, cancellationToken);

        var {aggregate} = {Aggregate}.Create(input.Name);

        dbContext.{Aggregates}.Add({aggregate});
        await dbContext.SaveChangesAsync(cancellationToken);

        return {aggregate};
    }
}
```
