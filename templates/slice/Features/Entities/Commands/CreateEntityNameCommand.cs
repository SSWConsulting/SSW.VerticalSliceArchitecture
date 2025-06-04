using MediatR;
using SSW.VerticalSliceArchitecture.Common.Domain.Entities;
using SSW.VerticalSliceArchitecture.Host.Extensions;

namespace SSW.VerticalSliceArchitecture.Features.Entities.Commands;

public static class CreateEntityNameCommand
{
    public record Request(string Name) : IRequest<ErrorOr<Guid>>;
    
    public class Endpoint : IEndpoint
    {
        public static void MapEndpoint(IEndpointRouteBuilder endpoints)
        {
            endpoints
                .MapApiGroup(EntitiesFeature.FeatureName)
                .MapPost("/",
                    async (ISender sender, Request request, CancellationToken ct) =>
                    {
                        var result = await sender.Send(request, ct);
                        return result.Match(_ => TypedResults.Created(), CustomResult.Problem);
                    })
                .WithName("CreateEntityName")
                .ProducesPost();
        }
    }
    
    internal sealed class Validator : AbstractValidator<Request>
    {
        public Validator()
        {
            RuleFor(v => v.Name)
                .NotEmpty();
        }
    }
    
    internal sealed class Handler(ApplicationDbContext dbContext)
        : IRequestHandler<Request, ErrorOr<Guid>>
    {
        public async Task<ErrorOr<Guid>> Handle(Request request, CancellationToken cancellationToken)
        {
            var entityName = EntityName.Create(request.Name);

            await dbContext.Entities.AddAsync(entityName, cancellationToken);
            await dbContext.SaveChangesAsync(cancellationToken);

            return entityName.Id.Value;
        }
    }
}