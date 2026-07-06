---
paths:
  - "**/*"
---

# Verification

A green Debug build is the weakest signal this template gives you. Real
verification means the code compiles the way CI compiles it, every test project
passes, the app actually boots under Aspire, and the endpoints you touched
return what they should. Work through the steps below before you call a change
done.

## Scale the effort to the change

Not every edit earns the full gauntlet.

- **Runtime changes** (endpoints, DI registration, EF Core mappings or
  migrations, middleware, `ServiceDefaults`, the AppHost) — run everything:
  builds, all tests, the Aspire boot, and REST smoke checks.
- **Domain, spec, or validator changes** with no wiring change — builds plus the
  unit and integration tests. Skip the Aspire loop only if nothing you changed
  can affect a running request.
- **Docs, comments, or test-only changes** — build and the relevant tests are
  enough; the Aspire boot and REST calls prove nothing here.

When in doubt, run the heavier tier. A skipped step you needed costs far more
than a minute of build time.

## 1. Build both configurations

```bash
dotnet build                # Debug
dotnet build -c Release     # Release
```

Debug alone will lie to you. Release sets `TreatWarningsAsErrors=true` and
`CodeAnalysisTreatWarningsAsErrors=true` (`Directory.Build.props`), and Release
is what NuGetAudit fails under with `NU1903`. A warning that Debug shrugs off is
the exact thing that turns CI red on every open PR. Build Release locally so you
find it first. See [dependencies.md](dependencies.md) when a `NU1903` appears
with no code change on your side.

## 2. Run every test project

```bash
dotnet test                 # all projects
```

Three jobs, three projects (see [testing.md](testing.md)):

- **Unit** (`tests/WebApi.UnitTests/`) — domain invariants, value objects,
  factory rules. Fast, no infrastructure.
- **Integration** (`tests/WebApi.IntegrationTests/`) — real SQL Server through
  Testcontainers, reset between tests with Respawn. **Docker or Podman must be
  running**, or these fail to spin up their container rather than fail an
  assertion.
- **Architecture** (`tests/WebApi.ArchitectureTests/`) — naming and layering
  conventions. A failure here means the code broke a rule; fix the code, not the
  test.

`dotnet test` covers all three. Run a single project by path when you want a
faster inner loop, but run the full set before you call the change done.

## 3. Boot the app under Aspire and confirm health

A build says the types line up. It says nothing about whether the app starts,
connects to SQL Server, runs its migrations, and reports its resources healthy.
Only a real boot tells you that.

```bash
aspire start                # aspire start --isolated inside a git worktree
```

Then drive the health check through the **aspire skill** rather than reading raw
logs by hand. It knows how to wait on a resource (`aspire wait <resource>`),
list resources, and read their health and traces. Confirm every resource
(SQL Server, the migration/seed step, the WebApi) reaches a healthy state before
you trust anything downstream. If the environment itself looks wrong,
`aspire doctor` is the first diagnostic.

## 4. Smoke-test the REST API

A healthy resource can still serve a broken endpoint. Once the app is up, call
the surface you actually changed plus a known-good baseline, and read the
responses. A 200 with the wrong body still fails verification.

- Hit the endpoints your change touched, across the status codes that matter
  (the success path and at least one validation or not-found path).
- Call a baseline you didn't touch (a Heroes read, `/swagger`, or
  `/swagger/v1/swagger.json`) to confirm the app is genuinely serving traffic
  and the failure, if any, is scoped to your change.
- The Swagger UI at `https://localhost:7255/swagger` is the quickest way to
  exercise an endpoint by hand; `curl` against the same routes works for a
  scripted check.

One caveat inherited from [dependencies.md](dependencies.md): `/swagger` and
`swagger.json` come from FastEndpoints (NSwag), a different stack from
`Microsoft.AspNetCore.OpenApi`. The two don't share a code path, so a 200 from
Swagger tells you nothing about the OpenAPI side. As shipped the WebApi calls
`AddOpenApi()` but never `MapOpenApi()`, so its `/openapi/v1.json` document isn't
mapped and returns 404 until something maps it; don't smoke-test that route
unless your change is what wired it up. Match the request to the code you
actually changed.
