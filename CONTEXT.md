# SSW Vertical Slice Architecture

A published .NET solution template. Two vocabularies live here and must not be mixed: the **template vocabulary**, which is what the repo is actually about, and the **sample domain**, which exists only to demonstrate the template and is deleted by consumers.

## Template language

**Template**:
The packaged artifact consumers install with `dotnet new`, built from this repo by `VerticalSliceArchitecture.nuspec`. "The template" means the shipped thing; "the repo" means this working copy.
_Avoid_: Boilerplate, starter kit, scaffold

**Slice**:
One use case, owning everything it needs from HTTP surface to persistence, living in its own folder under a Feature. `CreateHero` is a slice; so is `PowerLevelUpdated` — a slice may be triggered by a domain event rather than a request.
_Avoid_: Handler, module, command (as a name for the whole use case)

**Feature**:
A group of slices over the same aggregate, sharing a route prefix. `Heroes` is a Feature; it is not itself a slice.
_Avoid_: Module, area, domain (as a name for this grouping)

**Group**:
The route prefix a Feature's slices are registered under, so every endpoint in `Heroes` resolves beneath `/api/heroes`.
_Avoid_: Route group, prefix class

**Endpoint**:
The HTTP entry point of a slice — one route, one request shape, one response shape.
_Avoid_: Controller, action, handler

**Aggregate**:
A cluster of entities and value objects saved and validated as one unit, entered only through its root. `Team` is an aggregate; `Mission` is inside it and cannot be created independently.
_Avoid_: Root object, entity graph

**Entity**:
A domain object with identity and a lifecycle, whose invariants are enforced on the object itself rather than by callers.
_Avoid_: Model, record, POCO

**Value Object**:
A domain object with no identity, compared by its values and immutable once constructed. `Power` is one.
_Avoid_: DTO, struct

**Specification**:
A named, reusable query for an aggregate, defined as a factory method on that aggregate's spec class so every query for it lives in one place.
_Avoid_: Repository, query object, filter

**Strongly Typed ID**:
An identifier wrapped in its own type, so a `HeroId` can never be passed where a `TeamId` is expected.
_Avoid_: Wrapped ID, ID struct, primitive ID

**Domain Event**:
A record of something that has already happened inside an aggregate, raised by the aggregate and handled after the change is saved.
_Avoid_: Message, notification, integration event

**Eventual Consistency Failure**:
A failure in a domain event handler, after the originating aggregate has already been saved. The write stands; the follow-on work did not.
_Avoid_: Side effect error, post-save error

**Migration Service**:
The startup worker that brings the database to the current schema and seeds it before the API serves traffic.
_Avoid_: Seeder, migrator, DB init

## Sample domain language

Demonstration only. Consumers delete this domain; nothing in it is a claim about how a real system should model superheroes.

**Hero**:
A character with an alias and a set of powers, who may belong to one Team at a time.
_Avoid_: Character, user, member

**Alias**:
The public-facing name a Hero is known by, distinct from their name.
_Avoid_: Nickname, callsign, handle

**Power**:
A named ability held by a Hero, carrying its own strength rating.
_Avoid_: Ability, skill, trait

**Power Level**:
A strength rating. On a Power it is that ability's rating; on a Hero it is the sum across their powers.
_Avoid_: Strength, score, rating

**Total Power Level**:
The combined Power Level of every Hero currently on a Team.
_Avoid_: Team power, strength

**Team**:
A group of Heroes that takes on Missions, and which is either Available or already on one.
_Avoid_: Squad, group, roster

**Mission**:
A described piece of work a Team takes on, either in progress or complete. A Mission belongs to exactly one Team and only that Team can start or complete it.
_Avoid_: Task, job, quest

**Available**:
The state of a Team with no Mission in progress — the only state from which a Mission can be started.
_Avoid_: Idle, free, ready
