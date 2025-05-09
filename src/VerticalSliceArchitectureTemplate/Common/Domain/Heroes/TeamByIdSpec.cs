namespace VerticalSliceArchitectureTemplate.Common.Domain.Heroes;

// For more on the Specification Pattern see: https://www.ssw.com.au/rules/use-specification-pattern/
public sealed class HeroByIdSpec : SingleResultSpecification<Hero>
{
    public HeroByIdSpec(VerticalSliceArchitectureTemplate.Common.Domain.Heroes.HeroId heroId)
    {
        Query.Where(t => t.Id == heroId);
    }
}