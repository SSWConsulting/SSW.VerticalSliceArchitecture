---
applyTo: '**/*.md'
---

# GitHub Copilot Instructions for Creating ADRs

## Overview

This document provides GitHub Copilot with instructions for creating Architectural Decision Records (ADRs) that follow the established format and conventions used in the SSW.VerticalSliceArchitecture project.

## ADR File Naming Convention

- **Format**: `YYYYMMDD-descriptive-name.md`
- **Date**: Use the date when the decision is made (not when the ADR is written)
- **Name**: Use kebab-case with descriptive terms that clearly indicate the decision
- **Examples**:
    - `20240708-database-use-sql-temporal-tables-when-data-auditing-is-required.md`
    - `20240328-hosting-use-azure-container-registry.md`
    - `20250114-use-record-class-instead-of-record-struct.md`

## ADR Structure Template

### 1. Title (H1)

Follow the pattern: `[Category] - [Decision]`

**Categories commonly used:**

- `Project` - Core project decisions and architecture
- `Hosting` - Infrastructure and deployment decisions
- `Database` - Data storage and management decisions
- `Security` - Authentication, authorization, and security decisions
- `Communications` - Email, messaging, and communication decisions

**Examples:**

- `Database - Use SQL Temporal Tables when data auditing is required`
- `Hosting - Use Azure Container Registry`
- `Project - Use Vertical Slice Architecture as default for new modules`

### 2. Metadata Section

Use bullet points for metadata:

```markdown
- Status: [status] <!-- Required -->
- Deciders: [names] <!-- Required -->
- Date: YYYY-MM-DD <!-- Required -->
- Tags: [comma-separated tags] <!-- Optional but recommended -->

Technical Story: [GitHub issue link or description] <!-- Optional -->
```

**Status Options:**

- `draft` - Being written collaboratively
- `proposed` - Under review/discussion
- `accepted` - Approved and implemented
- `rejected` - Decided against
- `deprecated` - No longer valid
- `superseded by [link]` - Replaced by another ADR

**Tag Categories:**

- Technology: `dotnet`, `azure`, `sql`, `docker`
- Architecture: `modular`, `clean-architecture`, `vsa`, `microservices`
- Domain: `hosting`, `security`, `database`, `api`, `ui`
- Process: `dev-tools`, `testing`, `deployment`, `observability`

### 3. Required Sections

#### Context and Problem Statement (Required)

- Describe the problem or situation requiring a decision
- Explain why this decision is architecturally significant
- Provide sufficient context for future readers

#### Considered Options (Required)

- List all options that were evaluated
- Use bullet points or numbered lists
- Include brief descriptions of each option

#### Decision Outcome (Required)

- State the chosen option clearly: `Chosen option: "[option name]", because [reasoning]`
- Explain the primary reasons for the choice
- Reference the most important decision drivers

### 4. Optional Sections

#### Decision Drivers

- List factors that influenced the decision
- Business requirements, technical constraints, team preferences
- Use bullet points for clarity

#### Pros and Cons of the Options

- Detailed analysis of each considered option
- Use ✅ for pros and ❌ for cons
- Structure as subheadings for each option

#### Consequences

- Expected positive and negative outcomes
- Use ✅ for positive and ❌ for negative consequences
- Can be included in Decision Outcome section instead

#### Implementation

- Technical details about how the decision is implemented
- Code examples, configuration details, or setup instructions
- Links to relevant code files in the repository

#### Links

- References to external documentation
- Related ADRs
- GitHub issues or technical stories
- Official documentation links

## Content Guidelines

### Writing Style

- Use clear, concise language
- Write for future team members who weren't involved in the decision
- Avoid jargon unless necessary (define when used)
- Use active voice when possible

### Code Examples

- Include relevant code snippets when helpful
- Use proper syntax highlighting
- Keep examples focused and minimal
- Link to actual implementation files when possible

### Images and Diagrams

- Store images in `/l4b-static/images/` directory
- Use descriptive filenames
- Reference with relative paths: `![description](/l4b-static/images/filename.png)`
- Include alt text for accessibility

### Cross-References

- Link to related ADRs using relative paths
- Reference GitHub issues with proper links
- Link to relevant code files in the repository
- Use the format: `[description](relative-path.md)` for internal links

## Example Template

```markdown
# [Category] - [Decision Title]

- Status: accepted
- Deciders: [Name 1], [Name 2], [Name 3]
- Date: YYYY-MM-DD
- Tags: tag1, tag2, tag3

Technical Story: [Link to GitHub issue or description]

## Context and Problem Statement

[Describe the context and problem statement that led to this decision]

## Decision Drivers

- [Factor 1]
- [Factor 2]
- [Factor 3]

## Considered Options

- [Option 1]
- [Option 2]
- [Option 3]

## Decision Outcome

Chosen option: "[Selected option]", because [primary reasoning].

### Consequences

- ✅ [Positive consequence 1]
- ✅ [Positive consequence 2]
- ❌ [Negative consequence 1]

## Pros and Cons of the Options

### [Option 1]

- ✅ [Pro 1]
- ✅ [Pro 2]
- ❌ [Con 1]

### [Option 2]

- ✅ [Pro 1]
- ❌ [Con 1]
- ❌ [Con 2]

## Implementation

[Technical implementation details, code examples, or configuration]

## Links

- [External documentation](https://example.com)
- [Related ADR](relative-path-to-adr.md)
```

## SSW.VerticalSliceArchitecture-Specific Context

### Project Architecture

- This is an **enterprise-ready Vertical Slice Architecture template** for .NET 9
- Uses **Aspire orchestration** for local development and observability
- Built on **.NET 9** with **ASP.NET Core** and **FastEndpoints** and **Entity Framework Core**
- Features are self-contained vertical slices in `src/WebApi/Features/`
- Shared domain models in `Common/Domain/` and infrastructure in `Common/`

### Common Decision Areas

- **Feature Organization**: Vertical slice structure with Commands, Queries, Endpoints, and Handlers
- **Database Patterns**: Entity Framework Core, SQL Server, strongly-typed IDs with Vogen, specifications with Ardalis.Specification
- **API Design**: Minimal APIs with `IEndpoint` pattern, auto-discovery via reflection, `ErrorOr<T>` result pattern
- **Domain Modeling**: Aggregate roots, domain events, value objects, strongly-typed IDs with `Guid.CreateVersion7()`
- **Validation**: FluentValidation with `ValidationErrorOrResultBehavior` MediatR pipeline
- **Testing Strategy**: Unit tests for domain logic, integration tests with TestContainers and Respawn, architecture tests with NetArchTest
- **Orchestration**: .NET Aspire with AppHost, ServiceDefaults, and MigrationService

### References to Existing Patterns

When creating ADRs, reference existing architectural decisions and patterns:

- Link to feature templates in `/templates/` (e.g., `dotnet new ssw-vsa-slice`)
- Reference the slice structure: `{FeatureName}Feature.cs`, Commands, Queries, Endpoints
- Connect to domain layer patterns: strongly-typed IDs, specifications, domain events
- Reference testing approaches in `/tests/` (unit, integration, architecture tests)
- Align with the vertical slice architecture principles and eventual consistency patterns

## Usage Instructions for Copilot

When asked to create an ADR:

1. **Determine the category** based on the type of decision
2. **Use the current date** for the filename and metadata
3. **Follow the established structure** with required and relevant optional sections
4. **Use consistent formatting** with proper markdown syntax
5. **Include appropriate tags** based on the technology and domain
6. **Reference related ADRs** and project patterns when relevant
7. **Provide concrete examples** and implementation details when applicable
8. **Consider the SSW.VerticalSliceArchitecture context** and existing architectural decisions

Remember: ADRs are **immutable documentation** of decisions made at a specific point in time. They should capture not just what was decided, but why it was decided and what alternatives were considered.
