using MediatR;
using SSW.VerticalSliceArchitecture.Common.Domain.Entities;
using SSW.VerticalSliceArchitecture.Host.Extensions;
using System.Text.Json.Serialization;

namespace SSW.VerticalSliceArchitecture.Features.Entities.Commands;

public static class UpdateEntityNameCommand
{
    public record Request(string Name) : IRequest<ErrorOr<Guid>>
    {
        [JsonIgnore]
        public Guid EntityNameId { get; set; }
    }
    
    public class Endpoint : IEndpoint
    {
        public static void MapEndpoint(IEndpointRouteBuilder endpoints)
        {
            endpoints
                .MapApiGroup(EntitiesFeature.FeatureName)
                .MapPut("/{entityNameId:guid}",
                    async (ISender sender, Guid entityNameId, Request request, CancellationToken cancellationToken) =>
                    {
                        request.EntityNameId = entityNameId;
                        var result = await sender.Send(request, cancellationToken);
                        return result.Match(_ => TypedResults.NoContent(), CustomResult.Problem);
                    })
                .WithName("UpdateEntityName")
                .ProducesPut();
        }
    }
    
    internal sealed class Validator : AbstractValidator<Request>
    {
        public Validator()
        {
            RuleFor(v => v.EntityNameId)
                .NotEmpty();

            RuleFor(v => v.Name)
                .NotEmpty();
        }
    }
    
    internal sealed class Handler(ApplicationDbContext dbContext)
        : IRequestHandler<Request, ErrorOr<Guid>>
    {
        public async Task<ErrorOr<Guid>> Handle(Request request, CancellationToken cancellationToken)
        {
            var entityNameId = EntityNameId.From(request.EntityNameId);
            var entityName = await dbContext.Entities
                .FirstOrDefaultAsync(h => h.Id == entityNameId, cancellationToken);

            if (entityName is null)
                return EntityNameErrors.NotFound;

            entityName.Name = request.Name;

            await dbContext.SaveChangesAsync(cancellationToken);

            return entityName.Id.Value;
        }
    }
}