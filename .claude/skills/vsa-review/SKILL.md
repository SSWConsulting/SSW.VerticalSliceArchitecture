---
name: vsa-review
description: Review pending changes against this template's Vertical Slice Architecture conventions and report blockers, convention violations, and test gaps. Use when the user says "review this slice", "check this against the conventions", "vsa review", "did I break any rules", or asks for a conventions or architecture review of work in progress. Catches what the build can't — a missing Vogen registration, business logic in an endpoint, a slice importing another slice, or a slice shipped without tests.
---

# VSA Convention Review

`dotnet build` proves the types line up. The architecture tests prove a handful of naming and inheritance rules. Between them sits a wide band of convention breaks that ship green: a strongly typed ID nobody registered, business rules that leaked into an endpoint, a slice reaching into its neighbour, an aggregate loaded without its spec.

This skill reviews the pending diff against `.claude/rules/*` and reports what it finds.

## Boundary — this is not `/code-review`

This skill reviews **conventions**: does the change fit the architecture this template describes. It doesn't hunt for logic bugs, race conditions, performance problems, or security issues. Those are `/code-review` and `/security-review`, and running one doesn't substitute for the other. If you notice a genuine bug while reviewing, mention it once at the end under *Also spotted* and move on — don't turn the review into a bug hunt.

## Gather the change

Review only what's pending, not the whole repo.

```bash
git status --short
git diff --stat origin/main...HEAD    # committed on this branch
git diff --stat                       # unstaged
git diff --stat --cached              # staged
```

Then read the full diff for the files that turned up, plus **the full current contents of every changed C# file** — a diff hunk hides whether the endpoint above it already returned, or whether the ID registration exists somewhere off-hunk. Half of these checks are about something *missing*, and you can't see an absence in a diff.

Fetch first if the branch is stale: `git fetch origin main`.

If nothing is pending, say so and stop. Don't review `main` against itself.

## Review passes

Work through [references/checklist.md](references/checklist.md) — it holds the full itemised checks with the rule behind each. In summary:

1. **Domain** — entity shape, guards in setters, `MaxLength` consts, private constructors, errors, specs.
2. **Persistence** — EF configuration, `DbSet`, migration, and the Vogen registration.
3. **Slice** — folder layout, `Group<>`, `WithName`, `Send.*Async`, `Produces`, validators.
4. **Boundaries** — cross-slice imports, business logic placement, domain purity.
5. **Tests** — is each new slice and each new domain rule covered.

Verify each finding against the file before reporting it. A grep that looks like a violation often isn't — check the actual code. A false positive costs the reader more than a missed nitpick.

## Report format

Three buckets, most severe first. Skip empty buckets rather than printing "none found" three times.

```markdown
## 🚫 Blockers

Things that are broken at runtime or will fail CI.

- **`{Entity}Id` not registered in `VogenEfCoreConverters`** — `src/WebApi/Common/Domain/{Aggregate}/{Entity}.cs:12`
  The app throws on startup when EF tries to map this ID. Add `[EfCoreConverter<{Entity}Id>]` to
  `src/WebApi/Common/Persistence/VogenEfCoreConverters.cs`.

## ⚠️ Convention violations

Working code that breaks the template's architecture.

- **Business rule in the endpoint** — `src/WebApi/Features/{Feature}/{UseCase}/{UseCase}Endpoint.cs:31`
  The status check belongs on the aggregate, returning `ErrorOr<Success>`. See `Team.ExecuteMission`.

## 📋 Gaps

Missing work rather than wrong work — usually tests.

- **No integration test for `{UseCase}`** — every slice gets one; see
  `tests/WebApi.IntegrationTests/Endpoints/Heroes/Commands/CreateHeroCommandTests.cs`.
```

Rules for the report itself:

- **Every finding cites `file:line`.** A finding the reader can't navigate to gets ignored.
- **Every finding names the fix**, or points at the file that demonstrates it. "This is wrong" is not a review.
- **Bucket by consequence, not by feeling.** Blocker means it breaks at runtime or turns CI red. If it works and merges fine, it's a violation.
- **Don't pad.** A clean slice gets "No convention issues found" and a one-line note on what you checked. Inventing a nitpick to look thorough trains people to skim the next one.
- **Don't restate the diff.** The reader wrote it.

## After the report

Offer to fix the blockers, but don't fix them unprompted — the point of a review is that a person decides. If asked to apply fixes, use `/add-entity` and `/add-feature` for anything that means scaffolding, so the fix matches the templates.
