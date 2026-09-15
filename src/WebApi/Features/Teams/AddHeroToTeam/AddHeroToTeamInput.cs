using SSW.VerticalSliceArchitecture.Domain.Heroes;
using SSW.VerticalSliceArchitecture.Domain.Teams;

namespace SSW.VerticalSliceArchitecture.Features.Teams.AddHeroToTeam;

public sealed record AddHeroToTeamInput(TeamId TeamId, HeroId HeroId);
