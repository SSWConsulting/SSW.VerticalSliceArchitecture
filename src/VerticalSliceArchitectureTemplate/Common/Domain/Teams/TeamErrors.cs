namespace VerticalSliceArchitectureTemplate.Common.Domain.Teams;

public static class TeamErrors
{
    public static readonly Error NotOnMission = Error.Conflict(
        "Team.NotOnMission",
        "The team is currently not on a mission");

    public static readonly Error NotAvailable = Error.Conflict(
        "Team.NotAvailable",
        "The team is currently not available for a new mission");

    public static readonly Error NotFound = Error.NotFound(
        "Team.NotFound",
        "Team is not found");

    public static readonly Error NoHeroes = Error.Conflict(
        "Team.NoHeroes",
        "The team has no heroes");
}