# SSW Vertical Slice Architecture — Agent Instructions

## Project Overview

A template for **.NET 10 + Vertical Slice Architecture + Aspire**. Each use case is a self-contained vertical slice in its own folder under `src/WebApi/Features/{Feature}/`, shared domain types sit in `src/WebApi/Common/Domain/`, and infrastructure (EF Core, middleware, services) lives in `src/WebApi/Common/`. Terms are defined in `CONTEXT.md`.

## Technology Stack

- **.NET 10**, ASP.NET Core, EF Core (SQL Server)
- **FastEndpoints** — HTTP endpoints with strongly-typed request/response
- **Aspire** — local orchestration, observability, service discovery
- **Vogen** — strongly typed IDs
- **Ardalis.Specification** — query specs
- **ErrorOr** + **FluentValidation** — result & input handling
- **Bogus** — dev seed data
- **xUnit** + **Testcontainers** + **Respawn** — integration tests against a real SQL Server

## Rules

Detailed conventions are in `.claude/rules/` (auto-loaded by Claude Code when matching files are in scope):

| File | Covers |
|---|---|
| `architecture.md` | VSA slice layout, FastEndpoints conventions, groups, error handling |
| `domain.md` | entities, aggregates, value objects, specs, strongly typed IDs, domain events |
| `database.md` | adding entities, migration commands, seeding |
| `testing.md` | unit, integration, and architecture test projects |
| `dependencies.md` | NuGet audit (NU1903) failures, transitive pinning, verifying pins |
| `verification.md` | what "done" means: Debug + Release builds, all test projects, Aspire boot + health, REST smoke checks |

## Skills

The rules describe the conventions; the skills in `.claude/skills/` run them. Invoke one by name in Claude Code (`/add-slice`) or read its `SKILL.md` directly — the `references/` folder beside each holds the code templates.

| Skill | Does |
|---|---|
| `add-entity` | Domain object, strongly typed ID, errors, spec, EF configuration, `DbSet`, the `VogenEfCoreConverters` registration, and the migration |
| `add-slice` | One use case in its own folder — endpoint, request, response, validator, summary — plus the Feature and Group when it's the first slice, and its tests |
| `add-adr` | An Architectural Decision Record in `docs/adr/` following the repo's Log4brains conventions |
| `aspire` | Operating the AppHost through the Aspire CLI — start, wait, inspect resources, read logs and traces |
| `bump-version` | Cuts a template release by bumping the version in `VerticalSliceArchitecture.nuspec` |

Run `/add-entity` before `/add-slice` when the use case needs a domain type that doesn't exist yet.

`add-slice` scaffolds a **slice**, not a Feature: a slice is one use case, a Feature is the group of slices over an aggregate. See `CONTEXT.md`.

The templates in `references/` are copies of the shapes in `Features/Heroes/` and `Common/Domain/Heroes/`, so they drift when those change. Fix the template as part of whatever change made it stale.

## Running the App

```bash
aspire start
```

Aspire provisions SQL Server (Docker/Podman), runs migrations and seeds via `tools/MigrationService`, then exposes the API at `https://localhost:7255/swagger` (FastEndpoints Swagger UI). The Aspire Dashboard opens automatically for traces and logs.

## Reference Slice

`src/WebApi/Features/Heroes/CreateHero/` is the canonical example. Copy its shape when adding a new use case.

## Not Included (by design)

- **Auth** — add the auth scheme your project needs.
- **Per-feature DI** — most slices don't need `IFeature.ConfigureServices()`. Add it only when a slice has its own services.

## Agent skills

### Issue tracker

GitHub Issues on `SSWConsulting/SSW.VerticalSliceArchitecture`, driven with the `gh` CLI. See `docs/agents/issue-tracker.md`.

### Triage labels

The five canonical triage labels, unrenamed. See `docs/agents/triage-labels.md`.

### Domain docs

Single-context — `CONTEXT.md` and `docs/adr/` at the repo root. See `docs/agents/domain.md`.
