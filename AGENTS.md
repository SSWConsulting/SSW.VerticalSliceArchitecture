# SSW Vertical Slice Architecture — Agent Instructions

## Project Overview

A template for **.NET 10 + Vertical Slice Architecture + Aspire**. Each feature is a self-contained vertical slice under `src/WebApi/Features/`, shared domain types sit in `src/WebApi/Common/Domain/`, and infrastructure (EF Core, middleware, services) lives in `src/WebApi/Common/`.

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
