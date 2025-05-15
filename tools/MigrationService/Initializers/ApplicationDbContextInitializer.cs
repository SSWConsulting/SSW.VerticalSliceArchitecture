using Bogus;
using Microsoft.EntityFrameworkCore;
using VerticalSliceArchitectureTemplate.Common.Domain.Heroes;
using VerticalSliceArchitectureTemplate.Common.Domain.Teams;
using VerticalSliceArchitectureTemplate.Common.Persistence;

namespace MigrationService.Initializers;

public class ApplicationDbContextInitializer(ApplicationDbContext dbContext) : DbContextInitializerBase<ApplicationDbContext>(dbContext)
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
        var strategy = DbContext.Database.CreateExecutionStrategy();
        await strategy.ExecuteAsync(async () =>
        {
            // Seed the database
            await using var transaction = await DbContext.Database.BeginTransactionAsync(cancellationToken);
            var heroes = await SeedHeroes();
            await SeedTeams(heroes);
            // await DbContext.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
        });
    }

    private async Task<List<Hero>> SeedHeroes()
    {
        if (DbContext.Heroes.Any())
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
        await DbContext.Heroes.AddRangeAsync(heroes);
        await DbContext.SaveChangesAsync();

        return heroes;
    }

    private async Task SeedTeams(List<Hero> heroes)
    {
        if (DbContext.Teams.Any())
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
        await DbContext.Teams.AddRangeAsync(teams);
        await DbContext.SaveChangesAsync();
    }
}