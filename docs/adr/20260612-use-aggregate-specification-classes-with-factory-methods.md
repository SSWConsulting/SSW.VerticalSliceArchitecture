# Use Aggregate Specification Classes with Factory Methods

- Status: accepted
- Deciders: Daniel Mackay, Anton Polkanov
- Date: 2026-06-12
- Tags: domain, specifications

## Context and Problem Statement

We started with one class file per specification — `TeamByIdSpec`, `HeroByIdSpec`, and a new file for every query after that. Each file holds a single constructor that configures one query.

That falls apart as the query count grows. The files pile up in the Domain layer, and you can only find a spec if you already know its class name. There is no single place to see every query an aggregate supports, so each new query is a guess at whether one already exists.

Should we keep a file per specification, or group an aggregate's queries together?

## Decision Drivers

- Discoverability of the queries available for an aggregate
- Fewer files in the Domain layer
- Specs co-located with their aggregate

## Considered Options

1. One class per specification (the previous approach)
2. One class per aggregate with static factory methods

## Decision Outcome

Chosen option: **Option 2 — one class per aggregate with static factory methods**, because it puts every query for an aggregate in one place and cuts the file count without losing any clarity.

The class extends `SingleResultSpecification<T>` and each query is a static factory method that configures an instance through `spec.Query`. The naming convention is `{Aggregate}Spec`.

```csharp
public sealed class HeroSpec : SingleResultSpecification<Hero>
{
    public static HeroSpec ById(HeroId heroId)
    {
        var spec = new HeroSpec();
        spec.Query.Where(h => h.Id == heroId);
        return spec;
    }
}
```

Usage:

```csharp
dbContext.Heroes.WithSpecification(HeroSpec.ById(heroId)).FirstOrDefault();
```

### Consequences

- ✅ Every query for an aggregate lives in one file (`HeroSpec.cs`, `TeamSpec.cs`)
- ✅ Factory method names say what the query does (`HeroSpec.ById(...)`)
- ✅ Typing `HeroSpec.` surfaces every available query in IntelliSense
- ✅ No inner classes or extra indirection
- ❌ A little more boilerplate per query — a factory method instead of a constructor-only class

## Pros and Cons of the Options

### Option 1 — One class per specification

- ✅ Least boilerplate for a single query
- ❌ Files multiply as an aggregate gains queries
- ❌ No single place to see every query for an aggregate
- ❌ You have to know the class name to find the right spec

### Option 2 — One class per aggregate with static factory methods

- ✅ Every query browsable through IntelliSense on the aggregate's spec class
- ✅ Fewer files in the Domain layer
- ✅ Consistent `{Aggregate}Spec` naming
- ✅ No inner-class boilerplate
- ❌ A little more per-query boilerplate
