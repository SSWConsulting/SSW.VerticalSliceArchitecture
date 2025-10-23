namespace SSW.VerticalSliceArchitecture.Features.Entities.Endpoints;

public record GetAllEntitiesResponse(List<GetAllEntitiesResponse.EntityNameDto> Entities)
{
    public record EntityNameDto(Guid Id, string Name);
}

public class GetAllEntitiesEndpoint(ApplicationDbContext dbContext)
    : EndpointWithoutRequest<GetAllEntitiesResponse>
{
    public override void Configure()
    {
        Get("/");
        Group<EntitiesGroup>();
        Description(x => x.WithName("GetAllEntities"));
    }

    public async override Task HandleAsync(CancellationToken ct)
    {
        var entities = await dbContext.Entities
            .Select(h => new GetAllEntitiesResponse.EntityNameDto(h.Id.Value, h.Name))
            .ToListAsync(ct);

        await Send.OkAsync(new GetAllEntitiesResponse(entities), ct);
    }
}