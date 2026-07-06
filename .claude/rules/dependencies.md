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

So when the bumped assembly is on a runtime path a compiled package calls into
(OpenAPI document generation is the one this repo hit), a green build isn't
enough. Exercise the path. For OpenAPI pins: `aspire start --isolated`, then
`GET /swagger/v1/swagger.json` and confirm HTTP 200 with a valid document.

Pins that only touch build-time or dashboard assemblies don't need this — the
`MessagePack` pin, for instance, backs Aspire tooling and never reaches a
request. Generated projects may wire OpenAPI or Aspire differently, so adapt the
check to whatever surface actually loads the assembly.
