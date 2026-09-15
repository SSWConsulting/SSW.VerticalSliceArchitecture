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

So a green build isn't enough. You have to hit the surface that actually loads
the bumped assembly, which means knowing which stack owns it.

Almost everything the API serves goes through one endpoint, `POST /graphql`, so
most runtime checks are a query against it. Two caveats. A pin under
`HotChocolate.Types.Analyzers` is compile-time only — it is an analyzer, and the
proof it still works is that the generated resolvers appear in `schema.graphql`,
not that a request succeeds. And a pin under a middleware you have not exercised
proves nothing: filtering, sorting, paging, subscriptions and DataLoaders each
load different assemblies, so the query has to use the one you bumped.

Pins that never reach a request don't need a runtime check. The `MessagePack`
pin, for instance, backs Aspire tooling. Match the check to whatever surface
really loads the assembly, and if nothing does, the pin is just satisfying the
audit.
