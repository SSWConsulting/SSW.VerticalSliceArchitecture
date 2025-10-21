using SSW.VerticalSliceArchitecture.Common.Domain.Entities;

namespace SSW.VerticalSliceArchitecture.Features.Entities.Endpoints;

public record CreateEntityNameRequest(string Name);

public record CreateEntityNameResponse(Guid Id);

public class CreateEntityNameEndpoint(ApplicationDbContext dbContext)
    : Endpoint<CreateEntityNameRequest, CreateEntityNameResponse>
{
    public override void Configure()
    {
        Post("/");
        Group<EntitiesGroup>();
        Description(x => x.WithName("CreateEntityName"));
    }

    public override async Task HandleAsync(CreateEntityNameRequest req, CancellationToken ct)
    {
        var entityName = EntityName.Create(req.Name);

        await dbContext.Entities.AddAsync(entityName, ct);
        await dbContext.SaveChangesAsync(ct);

        await Send.OkAsync(new CreateEntityNameResponse(entityName.Id.Value), ct);
    }
}

public class CreateEntityNameRequestValidator : Validator<CreateEntityNameRequest>
{
    public CreateEntityNameRequestValidator()
    {
        RuleFor(v => v.Name)
            .NotEmpty();
    }
}