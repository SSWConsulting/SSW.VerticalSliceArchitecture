namespace VerticalSliceArchitectureTemplate.Common.Features;

public interface IFeature
{
    abstract static string FeatureName { get; }
    static abstract void ConfigureServices(IServiceCollection services, IConfiguration config);
}