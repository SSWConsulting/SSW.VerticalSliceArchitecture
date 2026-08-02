namespace SSW.VerticalSliceArchitecture.Features.Teams.GetAllTeams;

/// <remarks>
/// One team, not the whole list: the endpoint returns <c>PagedList&lt;GetAllTeamsResponse&gt;</c>, so the
/// paging envelope is the same shape on every list endpoint.
/// </remarks>
public record GetAllTeamsResponse(Guid Id, string Name, int TotalPowerLevel);
