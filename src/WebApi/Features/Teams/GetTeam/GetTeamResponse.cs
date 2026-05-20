namespace SSW.VerticalSliceArchitecture.Features.Teams.GetTeam;

public record GetTeamResponse(Guid Id, string Name, IEnumerable<GetTeamResponse.GetTeamHeroDto> Heroes)
{
    public record GetTeamHeroDto(Guid Id, string Name, string Alias, int PowerLevel, IEnumerable<GetTeamHeroPowerDto> Powers);

    public record GetTeamHeroPowerDto(string Name, int PowerLevel);
}
