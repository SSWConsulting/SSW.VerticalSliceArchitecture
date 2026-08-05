using Bogus;
using Microsoft.EntityFrameworkCore;
using SSW.VerticalSliceArchitecture.Common.Domain.Heroes;
using SSW.VerticalSliceArchitecture.Common.Domain.Teams;
using SSW.VerticalSliceArchitecture.Common.Persistence;

namespace Seeder.Initializers;

public class ApplicationDbContextInitializer(ApplicationDbContext dbContext)
{
    private const int NumHeroes = 20;

    private const int NumTeams = 5;

    private readonly string[] _superHeroNames =
    [
        "Superman",
        "Batman",
        "Wonder Woman",
        "Flash",
        "Aquaman",
        "Cyborg",
        "Green Lantern",
        "Shazam",
        "Captain Marvel",
        "Cyclops",
        "Wolverine",
        "Storm"
    ];

    private readonly string[] _superPowers =
    [
        "Strength",
        "Flight",
        "Invulnerability",
        "Speed",
        "Heat Vision",
        "X-Ray Vision",
        "Hearing",
        "Healing Factor",
        "Agility",
        "Stamina",
        "Breath",
        "Weapons",
        "Intelligence"
    ];

    private readonly string[] _missionNames =
    [
        "Save the world",
        "Rescue the hostages",
        "Defeat the villain",
        "Stop the bomb",
        "Protect the city"
    ];

    private readonly string[] _teamNames =
    [
        "Marvel",
        "Avengers",
        "DC",
        "Justice League",
        "X-Men"
    ];

    public async Task SeedDataAsync(CancellationToken cancellationToken)
    {
        var strategy = dbContext.Database.CreateExecutionStrategy();
        await strategy.ExecuteAsync(async () =>
        {
            // Seed the database
            await using var transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken);
            var heroes = await SeedHeroes();
            await SeedTeams(heroes);
            // await dbContext.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
        });
    }

    private async Task<List<Hero>> SeedHeroes()
    {
        if (dbContext.Heroes.Any())
            return [];

        var faker = new Faker<Hero>()
            .CustomInstantiator(f =>
            {
                var name = f.PickRandom(_superHeroNames);
                var hero = Hero.Create(name, name[..2]);
                var powers = f.PickRandom(_superPowers, f.Random.Number(1, 3))
                    .Select(p => new Power(p, f.Random.Number(1, 10)));
                hero.UpdatePowers(powers);
                return hero;
            });

        var heroes = faker.Generate(NumHeroes);
        await dbContext.Heroes.AddRangeAsync(heroes);
        await dbContext.SaveChangesAsync();

        return heroes;
    }

    private async Task SeedTeams(List<Hero> heroes)
    {
        if (dbContext.Teams.Any())
            return;

        var faker = new Faker<Team>()
            .CustomInstantiator(f =>
            {
                var name = f.PickRandom(_teamNames);
                var team = Team.Create(name);
                var heroesToAdd = f.PickRandom(heroes, f.Random.Number(1, 3));

                foreach (var hero in heroesToAdd)
                    team.AddHero(hero);

                var sendOnMission = f.Lorem.Random.Bool();

                if (sendOnMission)
                {
                    var missionName = f.PickRandom(_missionNames);
                    team.ExecuteMission(missionName);
                }

                return team;
            });

        var teams = faker.Generate(NumTeams);
        await dbContext.Teams.AddRangeAsync(teams);
        await dbContext.SaveChangesAsync();
    }
}