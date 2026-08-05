namespace SSW.VerticalSliceArchitecture.Features.Heroes.GetAllHeroes;

/// <remarks>
/// One hero, not the whole list: the endpoint returns <c>PagedList&lt;GetAllHeroesResponse&gt;</c>, so the
/// paging envelope is the same shape on every list endpoint.
/// </remarks>
public record GetAllHeroesResponse(
    Guid Id,
    string Name,
    string Alias,
    int PowerLevel,
    IReadOnlyList<GetAllHeroesResponse.HeroPowerDto> Powers)
{
    public record HeroPowerDto(string Name, int PowerLevel);
}
