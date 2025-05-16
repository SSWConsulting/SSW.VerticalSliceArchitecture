using VerticalSliceArchitectureTemplate.Common.Domain.Heroes;
using VerticalSliceArchitectureTemplate.Common.Domain.Teams;

namespace VerticalSliceArchitectureTemplate.UnitTests.Features.Teams;

public class TeamTests
{
    [Theory]
    [InlineData("c8ad9974-ca93-44a5-9215-2f4d9e866c7a", "cc3431a8-4a31-4f76-af64-e8198279d7a4", false)]
    [InlineData("c8ad9974-ca93-44a5-9215-2f4d9e866c7a", "c8ad9974-ca93-44a5-9215-2f4d9e866c7a", true)]
    public void TeamId_ShouldBeComparable(string stringGuid1, string stringGuid2, bool isEqual)
    {
        // Arrange
        Guid guid1 = Guid.Parse(stringGuid1);
        Guid guid2 = Guid.Parse(stringGuid2);
        TeamId id1 = TeamId.From(guid1);
        TeamId id2 = TeamId.From(guid2);

        // Act
        var areEqual = id1 == id2;

        // Assert
        areEqual.Should().Be(isEqual);
        id1.Value.Should().Be(guid1);
        id2.Value.Should().Be(guid2);
    }

    [Fact]
    public void Create_WithValidNameAndAlias_ShouldSucceed()
    {
        // Arrange
        var name = "name";

        // Act
        var team = Team.Create(name);

        // Assert
        team.Should().NotBeNull();
        team.Name.Should().Be(name);
    }

    [Fact]
    public void Create_WithNullNameAndAlias_ShouldThrow()
    {
        // Arrange
        string? name = null;

        // Act
        Action act = () => Team.Create(name!);

        // Assert
        act.Should().Throw<ArgumentException>().WithMessage("Value cannot be null. (Parameter 'name')");
    }

    [Fact]
    public void AddHero_ShouldUpdateTeamPowerLevel()
    {
        // Arrange
        var hero1 = Hero.Create("hero1", "alias1");
        var hero2 = Hero.Create("hero2", "alias2");
        var power1 = new Power("Foo", 10);
        var power2 = new Power("Bar", 4);
        hero1.UpdatePowers([power1]);
        hero2.UpdatePowers([power2]);
        var team = Team.Create("name");

        // Act
        team.AddHero(hero1);
        team.AddHero(hero2);

        // Assert
        team.TotalPowerLevel.Should().Be(14);
    }

    [Fact]
    public void RemoveHero_ShouldUpdateTeamPowerLevel()
    {
        // Arrange
        var hero1 = Hero.Create("hero1", "alias1");
        var hero2 = Hero.Create("hero2", "alias2");
        var power1 = new Power("Foo", 10);
        var power2 = new Power("Bar", 4);
        hero1.UpdatePowers([power1]);
        hero2.UpdatePowers([power2]);
        var team = Team.Create("name");
        team.AddHero(hero1);
        team.AddHero(hero2);

        // Act
        team.RemoveHero(hero1);

        // Assert
        team.TotalPowerLevel.Should().Be(4);
    }

    [Fact]
    public void ExecuteMission_ShouldUpdateTeamStatus()
    {
        // Arrange
        var team = Team.Create("name");
        team.AddHero(Hero.Create("hero1", "alias1"));

        // Act
        team.ExecuteMission("Mission");

        // Assert
        team.Status.Should().Be(TeamStatus.OnMission);
        team.Missions.Should().HaveCount(1);
        team.Missions.Should().ContainSingle(x => x.Description == "Mission");
    }

    [Fact]
    public void ExecuteMission_WhenTeamNotAvailable_ShouldError()
    {
        // Arrange
        var team = Team.Create("name");
        team.AddHero(Hero.Create("hero1", "alias1"));
        team.ExecuteMission("Mission1");

        // Act
        var result = team.ExecuteMission("Mission2");

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Should().Be(TeamErrors.NotAvailable);
    }

    [Fact]
    public void CompleteCurrentMission_ShouldUpdateTeamStatus()
    {
        // Arrange
        var team = Team.Create("name");
        team.ExecuteMission("Mission");

        // Act
        team.CompleteCurrentMission();

        // Assert
        team.Status.Should().Be(TeamStatus.Available);
    }

    [Fact]
    public void CompleteCurrentMission_WhenNoMissionHasBeenExecuted_ShouldThrow()
    {
        // Arrange
        var team = Team.Create("name");

        // Act
        var result = team.CompleteCurrentMission();

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Should().Be(TeamErrors.NotOnMission);
    }

    [Fact]
    public void CompleteCurrentMission_WhenNotOnMission_ShouldError()
    {
        // Arrange
        var team = Team.Create("name");
        team.ExecuteMission("Mission1");
        team.CompleteCurrentMission();

        // Act
        var result = team.CompleteCurrentMission();

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Should().Be(TeamErrors.NotOnMission);
    }

    [Fact]
    public void ExecuteMission_WhenNoHeroes_ShouldError()
    {
        // Arrange
        var team = Team.Create("name");

        // Act
        var result = team.ExecuteMission("Mission");

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Should().Be(TeamErrors.NoHeroes);
    }

    [Fact]
    public void ExecuteMission_AfterAddingHero_ShouldSucceed()
    {
        // Arrange
        var team = Team.Create("name");
        var hero = Hero.Create("hero1", "alias1");
        var power1 = new Power("Foo", 10);
        hero.UpdatePowers([power1]);
        team.AddHero(hero);

        // Act
        var result = team.ExecuteMission("Mission");

        // Assert
        result.IsError.Should().BeFalse();
    }
}