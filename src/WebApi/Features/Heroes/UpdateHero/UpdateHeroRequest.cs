namespace SSW.VerticalSliceArchitecture.Features.Heroes.UpdateHero;

public record UpdateHeroRequest(
    string Name,
    string Alias,
    Guid HeroId,
    IEnumerable<UpdateHeroRequest.HeroPowerDto> Powers)
{
    public record HeroPowerDto(string Name, int PowerLevel);
}
