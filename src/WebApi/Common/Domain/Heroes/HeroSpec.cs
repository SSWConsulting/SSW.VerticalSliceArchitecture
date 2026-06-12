namespace SSW.VerticalSliceArchitecture.Common.Domain.Heroes;

// For more on the Specification Pattern see: https://www.ssw.com.au/rules/use-specification-pattern/
public sealed class HeroSpec : SingleResultSpecification<Hero>
{
    public static HeroSpec ById(HeroId heroId)
    {
        var spec = new HeroSpec();
        spec.Query.Where(h => h.Id == heroId);
        return spec;
    }
}
