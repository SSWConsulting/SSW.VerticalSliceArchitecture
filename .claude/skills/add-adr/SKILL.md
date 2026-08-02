---
name: add-adr
description: Write an Architectural Decision Record in this repo's docs/adr/ following its Log4brains conventions — filename, title, metadata, and the sections that are required versus optional. Use when the user says "add an ADR", "record this decision", "write an ADR", "document why we chose X", or when a discussion settles an architecturally significant choice that nothing currently writes down.
---

# Add an ADR

An ADR captures one architecturally significant decision — what was decided, why,
and what was rejected. The set lives in `docs/adr/`, is managed by
[Log4brains](https://github.com/thomvaill/log4brains) (`.log4brains.yml`), and is
published as a static site from `main`.

**ADRs are immutable.** Once merged, the only thing that changes is the status. A
decision that gets reversed doesn't get edited — it gets a new ADR, and the old one
is marked `superseded by [link]`. Write it as a record of what was true on the date,
not as living documentation.

## When it earns an ADR

Write one when the decision constrains future work and someone will later ask "why
is it like this?" — a framework or library choice, a persistence or hosting pattern,
a convention every slice has to follow. Skip it for reversible implementation
details, bug fixes, and anything a code comment covers.

## 1. Filename

`YYYYMMDD-<kebab-slug>.md`, where the date is **when the decision was made**, not
when the record was written.

The slug usually leads with the category, but it isn't mandatory — both shapes exist
in the repo:

```
20251018-api-use-fastendpoints-instead-of-minimal-apis.md
20260515-vertical-slices-use-a-folder-per-slice.md
20260612-use-aggregate-specification-classes-with-factory-methods.md
```

## 2. Start from the template

Copy `docs/adr/template.md`. It carries the full section structure with inline
`<!-- -->` guidance on what each part is for and which are optional. Don't hand-roll
the skeleton — the template is the source of truth for the shape.

`docs/adr/index.md` is the Log4brains site homepage, not a manual index. There is
nothing to register — dropping the file in `docs/adr/` is enough.

## 3. Title

H1, pattern `[Category] - [Decision]`, or just the decision when no category fits:

- `API - Use FastEndpoints instead of Minimal APIs`
- `Database - Use SQL Temporal Tables when data auditing is required`
- `Use Aggregate Specification Classes with Factory Methods`

Categories in use: `API`, `Project`, `Vertical Slices`. Introduce a new one when
the decision genuinely doesn't fit — don't force it into an existing category.

## 4. Metadata

```markdown
- Status: accepted
- Deciders: Daniel Mackay, Anton Polkanov
- Date: 2026-06-12
- Tags: domain, specifications

Technical Story: <GitHub issue link>   <!-- optional -->
```

`Status` is one of `draft`, `proposed`, `accepted`, `rejected`, `deprecated`, or
`superseded by [xxx](yyyymmdd-xxx.md)`. Don't invent a deciders list — ask who was
involved, or leave the placeholder for the PR author to fill in.

Tags are free-form and lowercase. Existing ones cluster around technology (`dotnet`,
`sql`, `azure`), architecture (`vsa`, `clean-architecture`), domain (`api`,
`database`, `security`), and process (`testing`, `deployment`, `observability`).

## 5. Sections

Required: **Context and Problem Statement**, **Considered Options**, **Decision
Outcome**. Everything else in the template is optional — include it when it adds
something.

- **Context and Problem Statement** — the situation that forced a choice. Enough for
  a reader who wasn't there. Often lands best as a question.
- **Considered Options** — every option genuinely evaluated, including the status quo
  when "do nothing" was on the table. One that lists a single option isn't a decision
  record, it's an announcement.
- **Decision Outcome** — lead with `Chosen option: "X", because <reasoning>`, then the
  consequences.
- **Pros and Cons** / **Consequences** — `✅` for positive, `❌` for negative. Every
  option gets honest cons; an option with no downsides means the analysis is thin.

## 6. Writing style

- Write for a team member who joins in two years and wasn't in the room.
- Active voice, concrete nouns, no jargon you haven't defined.
- Use the vocabulary from [`CONTEXT.md`](../../../CONTEXT.md) and avoid the synonyms
  it lists under _Avoid_ — a slice is a slice, not a module or a handler.
- Link to real code (`src/WebApi/Features/Heroes/CreateHero/`) and to related ADRs by
  relative path. Images go in `docs/adr/l4b-static/` — Log4brains serves that folder
  at the site root, so reference them as `/l4b-static/<file>` with alt text. The
  folder doesn't exist yet; create it with the first image.
- Keep code examples minimal and focused on the decision, not the implementation.

## Repo context worth referencing

The decisions that keep recurring here, and where the existing patterns live:

- **Slice organisation** — one folder per use case under `src/WebApi/Features/{Feature}/`,
  with `CreateHero` as the reference shape. See ADR `20260515-vertical-slices-use-a-folder-per-slice`.
- **API surface** — FastEndpoints with typed request/response, FluentValidation
  running automatically, OpenAPI via `Summary` classes. See ADR `20251018-api-use-fastendpoints-instead-of-minimal-apis`.
- **Persistence** — EF Core on SQL Server, strongly typed IDs via Vogen, queries
  through Ardalis.Specification factory methods. See ADR `20260612-use-aggregate-specification-classes-with-factory-methods`.
- **Domain modelling** — aggregate roots, domain events, value objects, `Guid.CreateVersion7()`.
- **Testing** — unit, integration (Testcontainers + Respawn), architecture (NetArchTest).
- **Orchestration** — .NET 10 with Aspire: AppHost, ServiceDefaults, MigrationService.

If the new ADR contradicts an existing one, say so explicitly rather than quietly
overriding it — name the ADR and argue why it's worth reopening.
