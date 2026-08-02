# Convention checklist

Each check names the failure mode and the rule behind it. Work the passes in order — a domain problem usually explains the slice problem downstream of it.

Only report what you've confirmed by reading the file. Several of these are about something *absent*, which a diff can't show you.

---

## 1. Domain

Rule: [`.claude/rules/domain.md`](../../../rules/domain.md)

| Check | Why it matters | Severity |
|---|---|---|
| Entity inherits `Entity<TId>`, or `AggregateRoot<TId>` if it raises domain events | `DomainTests.DomainModel_Should_InheritsBaseClasses` fails otherwise | Blocker (CI red) |
| Private parameterless constructor present | EF can't materialise it, and `DomainTests` fails the build | Blocker (CI red) |
| Static `Create(...)` factory, no public constructor | Entities are created through the domain, not assembled by callers | Violation |
| Guards live in the property setter using `field`, not in `Create` | The setter is the only path every assignment goes through, including EF materialisation and later mutation. A guard in the factory is bypassed the moment anything else assigns | Violation |
| Every string property has a `public const int {Property}MaxLength` | The EF configuration reads the same constant, so the column and the guard can't drift | Violation |
| IDs use `Guid.CreateVersion7()` | Time-ordered, so it doesn't fragment the clustered index | Violation |
| Value objects are `record` and implement `IValueObject` | Structural equality is the point, and `IValueObject` is what the architecture tests match on | Blocker (CI red) |
| Errors are `Error` constants in `{Entity}Errors`, coded `{Entity}.{Condition}` | The code is part of the API contract, not a log message | Violation |
| Behaviour that can legitimately fail returns `ErrorOr<Success>` rather than throwing | An unavailable team is an expected outcome, not an exception | Violation |
| A new query on an aggregate is a factory method on the existing spec, not a new spec class | One spec per aggregate keeps its queries discoverable in one place | Violation |
| Domain code has no `using` from `Features.*` | The domain must not know about slices | Blocker (design) |

---

## 2. Persistence

Rule: [`.claude/rules/database.md`](../../../rules/database.md)

| Check | Why it matters | Severity |
|---|---|---|
| **Every new strongly typed ID has `[EfCoreConverter<...>]` in `VogenEfCoreConverters`** | The app throws at startup when EF maps a property with no converter. Compiles clean, tests clean | **Blocker** |
| Configuration inherits `AuditableConfiguration<T>`, not raw `IEntityTypeConfiguration<T>` | The base class maps the audit columns before delegating to `PostConfigure` | Violation |
| `HasMaxLength` reads the entity's `const`, never a literal | A literal is how the column and the guard drift apart | Violation |
| New aggregate root has an `ApplicationDbContext.{Entities}.cs` partial exposing `DbSet<T>` via `AggregateRootSet<T>()` | Without it EF never sees the entity, and the migration comes out empty | Blocker |
| Child entities have **no** `DbSet` | They're reached through their aggregate; a `DbSet` breaks the transactional boundary | Violation |
| A migration exists for every model change | The app migrates on startup, so a missing migration means a runtime schema mismatch | Blocker |
| Existing migrations are unmodified | Already-applied migrations are immutable; roll forward instead | Blocker |
| The generated migration matches the intent — no empty `Up()`, no `nvarchar(max)` where a max length was configured, no unexpected drops | An empty `Up()` means EF never saw the entity | Blocker |
| Seeding stays idempotent (`if (DbContext.X.Any()) return;`) | The initializer runs on every startup, not just the first | Violation |

The Vogen check is the one to run first and report loudest. To verify:

```bash
# every strongly typed ID that exists
grep -rn "readonly partial struct" src/WebApi/Common/Domain
# every ID that's registered
grep -n "EfCoreConverter" src/WebApi/Common/Persistence/VogenEfCoreConverters.cs
```

Every ID in the first list must appear in the second — child entity IDs included. Grep for the struct declaration rather than the `[ValueObject<Guid>]` attribute, because the attribute sits on the line above and doesn't name the type.

---

## 3. Slice

Rule: [`.claude/rules/architecture.md`](../../../rules/architecture.md)

| Check | Why it matters | Severity |
|---|---|---|
| One folder per use case under `Features/{Feature}/{UseCase}/` | The slice boundary is the folder | Violation |
| Namespace mirrors the folder | Keeps the folder and the type's identity in step | Violation |
| One type per file — endpoint, request, response, validator, summary each get their own | They grow | Violation |
| `Group<{Feature}Group>()` in `Configure()` | Without it the endpoint registers at the root instead of `/api/{prefix}` and loses the group's problem-details registration. Compiles clean | **Blocker** |
| `Description(x => x.WithName("{UseCase}"))` | It's the OpenAPI operation ID, so generated clients get a mangled method name without it | Violation |
| Every path out of `HandleAsync` ends in `await Send.*Async(...)` | A bare `return` sends an empty 200 | **Blocker** |
| `return` immediately after `Send.NotFoundAsync(ct)` | `Send` doesn't unwind the handler — execution continues and the next `Send` throws | **Blocker** |
| Every non-200 status the handler sends is declared with `Produces(...)` | Absent from the OpenAPI document means clients don't model it | Violation |
| Input validation is in the `Validator<T>`, not in `HandleAsync` | The validator runs first and auto-returns 400 | Violation |
| Validator max lengths match the entity's `const` | Otherwise an over-long value gets a 500 from the domain guard instead of a 400 | Violation |
| Load-then-mutate goes through `.WithSpecification({Entity}Spec...)` | A bare `FirstOrDefaultAsync` returns the aggregate with empty child collections and no error, then quietly writes wrong data | **Blocker** |
| Queries project inside `Select` rather than materialising tracked aggregates | Fetches only the needed columns and skips change tracking | Violation |
| Domain event handlers live in the consuming feature, not the domain | The domain raises events; it doesn't know who reacts | Violation |
| A handler that can't complete throws `EventualConsistencyException` | Handlers run in the same transaction; swallowing the failure leaves inconsistent data | Blocker |
| `{Feature}Feature.cs` exists only when the feature registers services | An empty `ConfigureServices` costs a reflection hit and misleads the next reader | Violation |
| No MVC — no `IActionResult`, no `[HttpGet]`, no controllers | See [`docs/adr/20251018-api-use-fastendpoints-instead-of-minimal-apis.md`](../../../../docs/adr/20251018-api-use-fastendpoints-instead-of-minimal-apis.md) | Blocker (design) |

---

## 4. Boundaries

The checks that make it Vertical Slice Architecture rather than layers with extra folders.

| Check | Why it matters | Severity |
|---|---|---|
| **No slice imports another slice** — no `using ...Features.{OtherFeature}.{OtherUseCase};`, no reference to another slice's request, response, or DTO types | Two slices sharing a type means neither can change independently, which is the whole point. Shared code moves to `Common/` or the domain; otherwise duplicate it | **Blocker (design)** |
| **Business rules live on the aggregate, not in the endpoint** — an `if` about domain state in `HandleAsync` is the tell | Rules on the entity are testable without HTTP and can't be bypassed by the next caller | **Blocker (design)** |
| Cross-aggregate reads go through the other aggregate's spec, not by reaching into its slice | `AddHeroToTeam` is the reference | Violation |
| No shared "DTO" or "Models" folder under `Features/` | Duplication between slices is the design | Violation |
| Nothing in `Common/` depends on `Features/` | Dependencies point inward | Blocker (design) |

To find cross-slice imports:

```bash
grep -rn "using SSW.VerticalSliceArchitecture.Features" src/WebApi/Features
```

On an unmodified template this returns **nothing at all** — a slice needs no `using` for its own feature, because it's already in that namespace. So any hit is a slice reaching sideways and is worth reading before you report it.

Adjust the namespace root to match the project if it was renamed on template instantiation — read `src/WebApi/GlobalUsings.cs`.

---

## 5. Tests

Rule: [`.claude/rules/testing.md`](../../../rules/testing.md)

| Check | Why it matters | Severity |
|---|---|---|
| Every new slice has an integration test | It's the only thing that proves the route, the binding, and the persistence actually work together | Gap |
| Commands with a 404 or domain-error path have a test for that path | The success case passes fine against an endpoint that dropped its not-found branch | Gap |
| Every new domain rule or invariant has a unit test | Guards and `ErrorOr` branches are cheap to test and expensive to get wrong | Gap |
| A new entity has a Bogus factory in `tests/WebApi.IntegrationTests/Common/Factories/` | Keeps a factory-signature change to one place | Gap |
| Integration tests use the typed client (`POSTAsync<TEndpoint, ...>`) rather than a raw URL | A route change can't leave the test passing against a stale URL | Violation |
| Unit tests touch no `DbContext` | If a domain test needs one, the logic is in the wrong layer | Violation |
| Assertions on domain errors compare the `Error` constant, not the message string | Messages get reworded; the constant is the contract | Violation |
| Architecture tests still pass | A failure means the code broke a rule — fix the code, not the test | Blocker (CI red) |

---

## Severity, decided by consequence

- **Blocker** — breaks at runtime, or turns CI red. The Vogen registration, a missing `Group<>`, a missing `Send`, a load-then-mutate without its spec, a failing architecture test. Also design breaks bad enough to be worth stopping for: a slice importing another slice, business logic in an endpoint.
- **Violation** — works and merges, but breaks a documented convention. Costs the next reader.
- **Gap** — nothing wrong with what's there; something expected is absent. Almost always tests.

When a check isn't in this list and you're unsure, read the rule file before calling it. `.claude/rules/` is the source of truth — this checklist is a working index of it, and when the two disagree, the rule wins and this file needs updating.
