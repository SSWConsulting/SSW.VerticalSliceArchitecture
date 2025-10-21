using SSW.VerticalSliceArchitecture.Common.Domain.Entities;

namespace SSW.VerticalSliceArchitecture.Features.Entities.Endpoints;

public record UpdateEntityNameRequest(Guid EntityNameId, string Name);

public class UpdateEntityNameEndpoint(ApplicationDbContext dbContext)
    : Endpoint<UpdateEntityNameRequest>
{
    public override void Configure()
    {
        Put("/{entityNameId}");
        Group<EntitiesGroup>();
        Description(x => x
            .WithName("UpdateEntityName")
            .Produces(StatusCodes.Status404NotFound));
    }

    public override async Task HandleAsync(UpdateEntityNameRequest req, CancellationToken ct)
    {
        var entityNameId = EntityNameId.From(req.EntityNameId);
        var entityName = await dbContext.Entities
            .FirstOrDefaultAsync(h => h.Id == entityNameId, ct);

        if (entityName is null)
        {
            await Send.NotFoundAsync(ct);
            return;
        }

        entityName.Name = req.Name;

        await dbContext.SaveChangesAsync(ct);

        await Send.NoContentAsync(ct);
    }
}

public class UpdateEntityNameRequestValidator : Validator<UpdateEntityNameRequest>
{
    public UpdateEntityNameRequestValidator()
    {
        RuleFor(v => v.EntityNameId)
            .NotEmpty();

        RuleFor(v => v.Name)
            .NotEmpty();
    }
}