using SSW.VerticalSliceArchitecture.Domain.Heroes;
using SSW.VerticalSliceArchitecture.Features.Heroes.CreateHero;

namespace SSW.VerticalSliceArchitecture.UnitTests.Features.Heroes;

public class CreateHeroInputValidatorTests
{
    private readonly CreateHeroInputValidator _validator = new();

    // The domain setters throw on over-length input, so anything the validator lets through
    // surfaces as a 500 instead of a 400. These boundaries are the contract between the two.
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
            result.Errors.Should().Contain(e => e.PropertyName == nameof(CreateHeroInput.Name));
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
            result.Errors.Should().Contain(e => e.PropertyName == nameof(CreateHeroInput.Alias));
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
    public void Validator_WithNullPowers_ShouldFail()
    {
        // Arrange — a request that omits "powers" arrives as null, and the resolver enumerates it
        var input = new CreateHeroInput("Clark Kent", "Superman", null!);

        // Act
        var result = _validator.Validate(input);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(CreateHeroInput.Powers));
    }

    [Fact]
    public void Validator_WithValidRequest_ShouldPass()
    {
        // Act
        var result = _validator.Validate(CreateInput());

        // Assert
        result.IsValid.Should().BeTrue();
    }

    private static CreateHeroInput CreateInput(
        string name = "Clark Kent",
        string alias = "Superman",
        string powerName = "Flight",
        int powerLevel = 8) =>
        new(name, alias, [new CreateHeroPowerInput(powerName, powerLevel)]);
}
