---
paths:
  - "Directory.Packages.props"
  - "Directory.Build.props"
  - "**/*.props"
---

# Dependencies & NuGet Audit

This is a template. `Directory.Packages.props` and `.claude/` both ship in the
package, so pins and this guidance flow through to every generated project.

## When a Release build suddenly fails with NU1903

Release builds set `TreatWarningsAsErrors=true` (`Directory.Build.props`), and
NuGetAudit raises `NU1903` for any package with a known-vuln advisory. When an
advisory is published against a **transitive** dependency, the Release build —
and CI on every open PR — breaks with no code change on your side.

That break is environmental. Don't bisect the PR's own diff; the PR didn't cause
it.

### The fix

Central transitive pinning is enabled (`CentralPackageTransitivePinningEnabled`),
so add a `<PackageVersion>` for the affected transitive package in
`Directory.Packages.props` at the first patched version (the GHSA lists its
patched range), with a comment naming the parent package and the GHSA id. The
`Microsoft.OpenApi` and `MessagePack` entries show the established shape.

Pin the transitive package rather than bumping its parent or turning off the
audit.

The comment isn't decoration. Naming the parent package records *why* the pin
exists, so a later package upgrade can revisit it: once the parent resolves a
patched version on its own, the pin is dead weight and should go. Every transitive
pin is a manual override of NuGet's resolution, so keep them minimal: pin only
what an advisory or conflict actually forces, and scan these comments for pins to
retire whenever you bump the parents.

### Land it fast

The failure sits on the shared baseline, not your branch, so every open PR stays
red until the pin reaches `main`. Merge or cherry-pick it ahead of feature work.

## Verify the pin at runtime when it sits on a request path

Forcing a transitive package above the version a **compiled** consumer was built
against is a binary-compatibility bet, and neither restore nor build can prove it.
A member that moved or changed signature throws `MissingMethodException` or
`TypeLoadException`, but only when that code actually runs.

So a green build isn't enough. You have to hit the endpoint that actually loads
the bumped assembly, which means knowing which stack owns it. There's a trap here:
this template runs two separate OpenAPI stacks. `Microsoft.OpenApi` belongs to
`Microsoft.AspNetCore.OpenApi` (`AddOpenApi()` / `MapOpenApi()`, served at
`/openapi/v1.json`). The `/swagger` UI and `/swagger/v1/swagger.json` come from
FastEndpoints.Swagger, which uses NSwag and never touches `Microsoft.OpenApi`. A
200 from `swagger.json` therefore tells you nothing about a `Microsoft.OpenApi`
pin.

As shipped, the WebApi calls `AddOpenApi()` but never `MapOpenApi()`, so the
`Microsoft.OpenApi` path isn't mapped and can't be exercised at all, so the pin
here only clears the audit. Once something maps it (`aspire start --isolated`, then
`GET /openapi/v1.json` expecting a valid document), that's the check to run.

Pins that never reach a request don't need a runtime check. The `MessagePack`
pin, for instance, backs Aspire tooling. Match the check to whatever surface
really loads the assembly, and if nothing does, the pin is just satisfying the
audit.
