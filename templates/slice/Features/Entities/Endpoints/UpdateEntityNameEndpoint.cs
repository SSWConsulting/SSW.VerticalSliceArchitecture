using SSW.VerticalSliceArchitecture.Common.Domain.Entities;

namespace SSW.VerticalSliceArchitecture.Features.Entities.Endpoints;

public record UpdateEntityNameRequest(string Name);

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
        var entityNameId = EntityNameId.From(request.EntityNameId);
        var entityName = await dbContext.Entities
            .FirstOrDefaultAsync(h => h.Id == entityNameId, cancellationToken);

        if (entityName is null)
            return EntityNameErrors.NotFound;

        entityName.Name = request.Name;

        await dbContext.SaveChangesAsync(cancellationToken);

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