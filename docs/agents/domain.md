# Domain Docs

How the engineering skills should consume this repo's domain documentation when exploring the codebase.

## Before exploring, read these

- **`CONTEXT.md`** at the repo root — the glossary / ubiquitous language.
- **`docs/adr/`** — read ADRs that touch the area you're about to work in. ADRs are managed by [Log4brains](https://github.com/thomvaill/log4brains) and named `YYYYMMDD-<slug>.md`; `docs/adr/index.md` lists them and `docs/adr/template.md` is the starting point for a new one.

If any of these files don't exist, **proceed silently**. Don't flag their absence; don't suggest creating them upfront. The `/domain-modeling` skill (reached via `/grill-with-docs` and `/improve-codebase-architecture`) creates them lazily when terms or decisions actually get resolved.

## File structure

This is a **single-context** repo — one glossary, one ADR directory, both at the root:

```
/
├── CONTEXT.md                                          ← created lazily by /domain-modeling
├── docs/adr/
│   ├── index.md
│   ├── template.md
│   ├── 20251018-api-use-fastendpoints-instead-of-minimal-apis.md
│   └── 20260612-use-aggregate-specification-classes-with-factory-methods.md
└── src/
    ├── WebApi/
    └── ServiceDefaults/
```

Vertical slices under `src/WebApi/Features/` are feature folders, not bounded contexts — they don't get their own `CONTEXT.md` or `docs/adr/`. If this repo ever splits into genuinely separate contexts, switch to a root `CONTEXT-MAP.md` pointing at per-context `CONTEXT.md` files.

## Use the glossary's vocabulary

When your output names a domain concept (in an issue title, a refactor proposal, a hypothesis, a test name), use the term as defined in `CONTEXT.md`. Don't drift to synonyms the glossary explicitly avoids.

If the concept you need isn't in the glossary yet, that's a signal — either you're inventing language the project doesn't use (reconsider) or there's a real gap (note it for `/domain-modeling`).

## Flag ADR conflicts

If your output contradicts an existing ADR, surface it explicitly rather than silently overriding:

> _Contradicts ADR 20251018 (FastEndpoints instead of minimal APIs) — but worth reopening because…_
