using Bogus;
using VerticalSliceArchitectureTemplate.Common.Domain.Teams;

namespace VerticalSliceArchitectureTemplate.IntegrationTests.Common.Factories;

public static class TeamFactory
{
    private static readonly Faker<Team> TeamFaker = new Faker<Team>().CustomInstantiator(f =>
    {
        var team = Team.Create(
            f.Company.CompanyName()
        );

        return team;
    });

    public static Team Generate() => TeamFaker.Generate();

    public static IReadOnlyList<Team> Generate(int count) => TeamFaker.Generate(count);
}