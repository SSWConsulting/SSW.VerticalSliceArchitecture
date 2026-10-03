using SSW.VerticalSliceArchitecture.Domain.Heroes;
using SSW.VerticalSliceArchitecture.Features.Heroes.UpdateHero;

namespace SSW.VerticalSliceArchitecture.UnitTests.Features.Heroes;

public class UpdateHeroInputValidatorTests
{
    private readonly UpdateHeroInputValidator _validator = new();

    [Theory]
    [InlineData(0, false)]
    [InlineData(1, true)]
    [InlineData(Hero.NameMaxLength, true)]
    [InlineData(Hero.NameMaxLength + 1, false)]
    public void Validator_WithNameOfLength_ShouldMatchDomainLimit(int length, bool expectedValid)
    {
        // Arrange
        var input = CreateInput(name: new string('a', length));

        // Act
        var result = _validator.Validate(input);

        // Assert
        result.IsValid.Should().Be(expectedValid);

        if (!expectedValid)
        {
            result.Errors.Should().Contain(e => e.PropertyName == nameof(UpdateHeroInput.Name));
        }
    }

    [Theory]
    [InlineData(0, false)]
    [InlineData(1, true)]
    [InlineData(Hero.AliasMaxLength, true)]
    [InlineData(Hero.AliasMaxLength + 1, false)]
    public void Validator_WithAliasOfLength_ShouldMatchDomainLimit(int length, bool expectedValid)
    {
        // Arrange
        var input = CreateInput(alias: new string('a', length));

        // Act
        var result = _validator.Validate(input);

        // Assert
        result.IsValid.Should().Be(expectedValid);

        if (!expectedValid)
        {
            result.Errors.Should().Contain(e => e.PropertyName == nameof(UpdateHeroInput.Alias));
        }
    }

    [Theory]
    [InlineData(0, false)]
    [InlineData(1, true)]
    [InlineData(Power.NameMaxLength, true)]
    [InlineData(Power.NameMaxLength + 1, false)]
    public void Validator_WithPowerNameOfLength_ShouldMatchDomainLimit(int length, bool expectedValid)
    {
        // Arrange
        var input = CreateInput(powerName: new string('a', length));

        // Act
        var result = _validator.Validate(input);

        // Assert
        result.IsValid.Should().Be(expectedValid);
    }

    [Theory]
    [InlineData(0, false)]
    [InlineData(1, true)]
    [InlineData(10, true)]
    [InlineData(11, false)]
    public void Validator_WithPowerLevel_ShouldEnforceRange(int powerLevel, bool expectedValid)
    {
        // Arrange
        var input = CreateInput(powerLevel: powerLevel);

        // Act
        var result = _validator.Validate(input);

        // Assert
        result.IsValid.Should().Be(expectedValid);
    }

    [Fact]
    public void Validator_WithEmptyHeroId_ShouldFail()
    {
        // Arrange
        var input = CreateInput(heroId: HeroId.From(Guid.Empty));

        // Act
        var result = _validator.Validate(input);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(UpdateHeroInput.HeroId));
    }

    [Fact]
    public void Validator_WithNullPowers_ShouldFail()
    {
        // Arrange — a request that omits "powers" arrives as null, and the resolver enumerates it
        var input = new UpdateHeroInput(HeroId.From(Guid.CreateVersion7()), "Clark Kent", "Superman", null!);

        // Act
        var result = _validator.Validate(input);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(UpdateHeroInput.Powers));
    }

    [Fact]
    public void Validator_WithValidRequest_ShouldPass()
    {
        // Act
        var result = _validator.Validate(CreateInput());

        // Assert
        result.IsValid.Should().BeTrue();
    }

    private static UpdateHeroInput CreateInput(
        string name = "Clark Kent",
        string alias = "Superman",
        HeroId? heroId = null,
        string powerName = "Flight",
        int powerLevel = 8) =>
        new(
            heroId ?? HeroId.From(Guid.CreateVersion7()),
            name,
            alias,
            [new UpdateHeroPowerInput(powerName, powerLevel)]);
}
