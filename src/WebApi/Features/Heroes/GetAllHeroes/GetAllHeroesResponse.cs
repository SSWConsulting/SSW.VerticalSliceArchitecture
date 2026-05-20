namespace SSW.VerticalSliceArchitecture.Features.Heroes.GetAllHeroes;

public record GetAllHeroesResponse(List<GetAllHeroesResponse.HeroDto> Heroes)
{
    public record HeroDto(
        Guid Id,
        string Name,
        string Alias,
        int PowerLevel,
        IReadOnlyList<HeroPowerDto> Powers);

    public record HeroPowerDto(string Name, int PowerLevel);
}
