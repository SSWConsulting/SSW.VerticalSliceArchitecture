namespace SSW.VerticalSliceArchitecture.Features.Heroes.CreateHero;

public sealed record CreateHeroInput(
    string Name,
    string Alias,
    IReadOnlyList<CreateHeroPowerInput> Powers);

/// <remarks>
/// A power input per slice, rather than one shared type, keeps the slices independent. The cost is
/// a second structurally identical type in the schema; the benefit is that changing what
/// <c>createHero</c> accepts cannot change what <c>updateHero</c> accepts.
/// </remarks>
public sealed record CreateHeroPowerInput(string Name, int PowerLevel);
