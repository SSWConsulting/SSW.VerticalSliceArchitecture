namespace SSW.VerticalSliceArchitecture.Common.Interfaces;

public interface IFeature
{
    static abstract void ConfigureServices(IServiceCollection services, IConfiguration config);
}