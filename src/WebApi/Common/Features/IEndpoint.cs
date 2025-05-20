namespace SSW.VerticalSliceArchitecture.Common.Features;

public interface IEndpoint
{
    static abstract void MapEndpoint(IEndpointRouteBuilder endpoints);
}