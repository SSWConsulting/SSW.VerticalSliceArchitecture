# Remove the Vertical Slice item template

- Status: accepted
- Deciders: Daniel Mackay, Anton Polkanov
- Date: 2026-05-15
- Tags: dotnet, templates, tooling, ai, developer-experience

## Context and Problem Statement

This repository is itself a `dotnet new` template package. It has shipped two templates: the `ssw-vsa` solution template, which scaffolds a whole Vertical Slice Architecture solution, and the `ssw-vsa-slice` item template under `templates/slice/`, which scaffolds a single feature slice (domain entity, specification, errors, EF configuration, `DbContext` partial, feature class, and Create/Update/GetAll endpoints) into an existing solution.

The `ssw-vsa-slice` template is a second copy of the slice structure. Every architectural change to a slice has to be applied twice: once to the real `Heroes`/`Teams` features and again to the placeholder files under `templates/slice/`. AI coding agents can now scaffold a slice directly from the conventions written down in `AGENTS.md`. Given that, is it still worth maintaining the item template?

## Decision Drivers

- **Drift risk**: the template duplicates the slice structure, so it silently goes stale whenever the architecture moves. The FastEndpoints adoption and the .NET 10 upgrade are both recent changes the template would have needed.
- **Single source of truth**: slice conventions are already documented in `AGENTS.md` for AI agents to follow.
- **CI surface**: `meta-test.yml` ran a dedicated job to generate and build a slice from the template.
- **Protect the core deliverable**: the `ssw-vsa` solution template is the repository's main product and must stay.
- **Limited functionality**: static templates are limited, especially compared to AI agents.

## Considered Options

1. Keep the `ssw-vsa-slice` item template and maintain it alongside the code.
2. Remove the `ssw-vsa-slice` item template and rely on AI agents scaffolding from `AGENTS.md`.
3. Keep the template but drop its CI coverage.

## Decision Outcome

Chosen option: **"Remove the `ssw-vsa-slice` item template"**, because AI coding agents now scaffold slices from the documented conventions, which makes the template redundant duplication that drifts from the real code. Removing it deletes a maintenance burden and a class of "the template is out of date" bugs. Slice-creation guidance now lives only in `AGENTS.md`. The `ssw-vsa` solution template is unaffected.

### Consequences

- ✅ One fewer artifact to keep in sync when the slice structure changes.
- ✅ Slice-creation guidance sits in one place, `AGENTS.md`, next to the rest of the agent instructions.
- ✅ `meta-test.yml` is shorter: it no longer generates and builds a slice from the template.
- ❌ Developers without an AI agent lose the one-command `dotnet new ssw-vsa-slice` scaffold and copy an existing feature such as `Heroes` instead.
- ❌ The slice structure is now described in prose rather than enforced by an executable template, so the docs have to be kept accurate by hand.

## Pros and Cons of the Options

### Keep the `ssw-vsa-slice` item template and maintain it

The existing approach: a `dotnet new` item template that mirrors the slice structure.

- ✅ One command scaffolds a complete, buildable slice with no AI agent required.
- ✅ CI can prove the scaffold still compiles.
- ❌ The template is a second copy of the slice structure that drifts from the real code.
- ❌ Every architectural change has to be made twice, and a missed update is invisible until someone runs the template.
- ❌ Templated placeholder files (`EntityName`, `Entities`) are harder to read and review than real example code.

### Remove the `ssw-vsa-slice` item template and rely on AI agents

Delete `templates/`; document the slice structure in `AGENTS.md` for agents to follow.

- ✅ Removes the duplication and the drift it causes.
- ✅ Slice conventions live in one maintained place.
- ✅ Agents scaffold from the actual `Heroes` feature, which is always current.
- ❌ Loses the single-command scaffold for developers not using an AI agent.
- ❌ Relies on the `AGENTS.md` documentation staying accurate.

### Keep the template but drop its CI coverage

Leave `templates/slice/` in place but stop generating a slice in `meta-test.yml`.

- ✅ Shortens CI.
- ❌ Keeps the duplication and the maintenance cost.
- ❌ Removes the only signal that the template still works, so it would rot unnoticed.

## Links

- [Vertical Slice Architecture Template](https://github.com/SSWConsulting/SSW.VerticalSliceArchitecture)
- Slice-creation guidance: `AGENTS.md`, section "Adding New Features"
