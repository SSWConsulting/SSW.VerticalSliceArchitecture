using SSW.VerticalSliceArchitecture.Common.Domain.Entities;

namespace SSW.VerticalSliceArchitecture.Features.Entities.Endpoints;

public record CreatEntityRequest(string Name);

public record CreateEntityResponse(Guid Id);

public class CreateEntityEndpoint(ApplicationDbContext dbContext)
    : Endpoint<CreateEntityRequest, CreateEntityResponse>
{
    public override void Configure()
    {
        Post("/");
        Group<EntitiesGroup>();
        Description(x => x.WithName("CreateEntity"));
    }

    public override async Task HandleAsync(CreateEntityRequest req, CancellationToken ct)
    {
        var entityName = Entity.Create(req.Name);

        await dbContext.Entities.AddAsync(entityName, ct);
        await dbContext.SaveChangesAsync(ct);

        await Send.OkAsync(new CreateEntityResponse(entityName.Id.Value), ct);
    }
}

public class CreateEntityRequestValidator : Validator<CreateEntityRequest>
{
    public CreateEntityRequestValidator()
    {
        RuleFor(v => v.Name)
            .NotEmpty();
    }
}