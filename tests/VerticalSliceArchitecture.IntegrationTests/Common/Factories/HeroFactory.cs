using SSW.VerticalSliceArchitecture.Common.Domain.Heroes;

namespace SSW.VerticalSliceArchitecture.IntegrationTests.Common.Factories;

public static class HeroFactory
{
    private static readonly Faker<Power> PowerFaker =
        new Faker<Power>().CustomInstantiator(f => new Power(f.Commerce.Product(), f.Random.Number(1, 10)));

    private static readonly Faker<Hero> HeroFaker = new Faker<Hero>().CustomInstantiator(f =>
    {
        var hero = Hero.Create(
            f.Person.FullName,
            f.Person.FirstName
        );

        var powers = PowerFaker
            .Generate(f.Random.Number(1, 5));

        hero.UpdatePowers(powers);

        return hero;
    });

    public static Hero Generate() => HeroFaker.Generate();

    public static IReadOnlyList<Hero> Generate(int count) => HeroFaker.Generate(count);
}