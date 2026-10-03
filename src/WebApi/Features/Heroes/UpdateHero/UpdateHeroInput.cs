using SSW.VerticalSliceArchitecture.Domain.Heroes;

namespace SSW.VerticalSliceArchitecture.Features.Heroes.UpdateHero;

public sealed record UpdateHeroInput(
    HeroId HeroId,
    string Name,
    string Alias,
    IReadOnlyList<UpdateHeroPowerInput> Powers);

public sealed record UpdateHeroPowerInput(string Name, int PowerLevel);
