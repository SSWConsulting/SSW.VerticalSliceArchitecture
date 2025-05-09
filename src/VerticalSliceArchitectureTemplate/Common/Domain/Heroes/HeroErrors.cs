namespace VerticalSliceArchitectureTemplate.Common.Domain.Heroes;

public static class HeroErrors
{
    public static readonly Error NotFound = Error.NotFound(
        "Hero.NotFound",
        "Hero is not found");
}