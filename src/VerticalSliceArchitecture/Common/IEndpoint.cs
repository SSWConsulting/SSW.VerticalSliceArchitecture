namespace VerticalSliceArchitecture.Common;

public interface IEndpoint
{
    static abstract void MapEndpoint(IEndpointRouteBuilder endpoints);
}