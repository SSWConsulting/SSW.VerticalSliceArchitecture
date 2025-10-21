# API - Use FastEndpoints instead of Minimal APIs

- Status: accepted
- Deciders: Daniel Mackay, Gordon Beeming, Luke Cook, Anton Polkanov
- Date: 2025-10-21
- Tags: dotnet, api, architecture, vsa, fastendpoints, minimal-apis

## Context and Problem Statement

The SSW.VerticalSliceArchitecture template initially used ASP.NET Core Minimal APIs with a custom `IEndpoint` interface pattern for organizing endpoints. While this approach provided low ceremony and direct control, as the template grew to support multiple features (Heroes, Teams) and needed to demonstrate enterprise patterns, we encountered challenges with structure, validation boilerplate, and testing consistency.

The key question: Should we continue with Minimal APIs and custom patterns, or adopt FastEndpoints to gain opinionated structure and productivity features while maintaining performance?

## Decision Drivers

- **Maintainability at scale**: The template serves as a foundation for enterprise applications that may grow to dozens or hundreds of endpoints
- **Developer productivity**: Reducing boilerplate for validation, OpenAPI metadata, and testing across vertical slices
- **Consistency across teams**: Providing clear conventions that multiple developers can follow
- **Performance**: Maintaining Minimal API-level performance characteristics
- **Learning curve**: Balancing structure with ease of adoption for developers new to the template
- **Vertical Slice Architecture alignment**: The endpoint pattern must support self-contained feature slices
- **Testing support**: First-class support for unit and integration testing of endpoints
- **Microsoft ecosystem alignment**: Long-term compatibility with .NET evolution

## Considered Options

1. **Continue with Minimal APIs + Custom IEndpoint Pattern**
2. **Adopt FastEndpoints Framework**
3. **Use MVC Controllers**

## Decision Outcome

Chosen option: **"Adopt FastEndpoints Framework"**, because it provides the opinionated structure, built-in validation, testing helpers, and conventions that align perfectly with Vertical Slice Architecture principles while maintaining Minimal API-level performance. The framework's REPR (Request-Endpoint-Response) pattern naturally maps to our feature slice organization, and its auto-discovery mechanism reduces registration boilerplate.

### Consequences

- ✅ **Reduced boilerplate**: Built-in validation (FluentValidation), response helpers, and OpenAPI support eliminate repetitive wiring code
- ✅ **Better structure**: REPR pattern provides clear separation of concerns within each feature slice
- ✅ **Improved testing**: FastEndpoints.Testing package provides helpers that simplify endpoint testing
- ✅ **Auto-registration**: Endpoint discovery eliminates manual registration in Program.cs
- ✅ **Performance**: Benchmarks show comparable performance to Minimal APIs, superior to MVC controllers
- ✅ **VSA alignment**: Each endpoint is a class that naturally fits in the feature folder structure
- ✅ **Consistent conventions**: Teams can follow established patterns without inventing their own
- ❌ **Third-party dependency**: Introduces reliance on community-maintained library vs. first-party Microsoft framework
- ❌ **Learning curve**: Developers must learn FastEndpoints idioms and REPR conventions
- ❌ **Framework lock-in**: Switching back to Minimal APIs or other patterns requires significant refactoring

## Pros and Cons of the Options

### Continue with Minimal APIs + Custom IEndpoint Pattern

The existing approach with custom `IEndpoint` interface and manual endpoint registration.

- ✅ Zero external dependencies beyond ASP.NET Core
- ✅ Full control over endpoint behavior and pipeline
- ✅ Extremely low ceremony for simple endpoints
- ✅ Maintained by Microsoft with long-term support
- ✅ Familiar to developers who know ASP.NET Core
- ❌ Growing boilerplate as features multiply (validation, OpenAPI, response mapping)
- ❌ No built-in testing helpers - must implement custom patterns
- ❌ Manual endpoint registration becomes verbose (EndpointDiscovery reflection mitigates this)
- ❌ Teams must invent and enforce their own conventions for DTOs, validation, error handling
- ❌ OpenAPI metadata requires manual attribute decoration or configuration
- ❌ Need additional libraries like MediatR to provider commands and eventing

### Adopt FastEndpoints Framework

Community framework providing structured endpoint pattern with REPR.

- ✅ REPR pattern (Request → Endpoint → Response) aligns naturally with Vertical Slice Architecture
- ✅ Built-in FluentValidation integration - validators automatically discovered and executed
- ✅ FastEndpoints.Testing provides first-class endpoint testing support
- ✅ Auto-discovery and registration of endpoints eliminates boilerplate
- ✅ Performance benchmarks show parity with Minimal APIs
- ✅ Rich OpenAPI/Swagger support with minimal configuration (better than Minimal APIs)
- ✅ Response helpers (`SendAsync`, `SendCreatedAtAsync`, etc.) reduce repetitive code
- ✅ Strong authorization and authentication helpers
- ✅ Active community and regular updates (compatible with .NET 9)
- ✅ Remove reliance on MediatR (which has now gone commercial)
- ❌ Third-party dependency creates ABI/upgrade surface
- ❌ Community-maintained (not Microsoft first-party)
- ❌ Requires learning framework-specific patterns and idioms

### Use MVC Controllers

Traditional ASP.NET Core MVC controller pattern.

- ✅ Well-established pattern with extensive documentation
- ✅ First-party Microsoft framework with long-term support
- ✅ Familiar to most .NET developers
- ✅ Built-in model binding, validation, and filter pipeline
- ❌ Significantly lower performance than Minimal APIs (benchmark studies)
- ❌ Higher ceremony with base classes, routing attributes
- ❌ Less aligned with Vertical Slice Architecture (encourages horizontal layers)
- ❌ More boilerplate for simple CRUD operations
- ❌ Considered legacy pattern in modern .NET development

## Links

- [FastEndpoints Documentation](https://fast-endpoints.com/)
- [FastEndpoints GitHub Repository](https://github.com/FastEndpoints/FastEndpoints)
- [ASP.NET Core Minimal APIs Documentation](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/minimal-apis)
- [Vertical Slice Architecture Template](https://github.com/SSWConsulting/SSW.VerticalSliceArchitecture)
- [FastEndpoints vs Minimal APIs Benchmark](https://fast-endpoints.com/benchmarks)
