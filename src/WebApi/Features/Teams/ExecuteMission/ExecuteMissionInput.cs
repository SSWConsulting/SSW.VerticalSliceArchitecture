using SSW.VerticalSliceArchitecture.Domain.Teams;

namespace SSW.VerticalSliceArchitecture.Features.Teams.ExecuteMission;

public sealed record ExecuteMissionInput(TeamId TeamId, string Description);
