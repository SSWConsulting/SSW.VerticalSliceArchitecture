using SSW.VerticalSliceArchitecture.Common.Domain.Heroes;
using SSW.VerticalSliceArchitecture.Features.Heroes.UpdateHero;

namespace SSW.VerticalSliceArchitecture.UnitTests.Features.Heroes;

public class UpdateHeroRequestValidatorTests
{
    private readonly UpdateHeroRequestValidator _validator = new();

    [Theory]
    [InlineData(0, false)]
    [InlineData(1, true)]
    [InlineData(Hero.NameMaxLength, true)]
    [InlineData(Hero.NameMaxLength + 1, false)]
    public void Validator_WithNameOfLength_ShouldMatchDomainLimit(int length, bool expectedValid)
    {
        // Arrange
        var request = CreateRequest(name: new string('a', length));

        // Act
        var result = _validator.Validate(request);

        // Assert
        result.IsValid.Should().Be(expectedValid);

        if (!expectedValid)
        {
            result.Errors.Should().Contain(e => e.PropertyName == nameof(UpdateHeroRequest.Name));
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
        var request = CreateRequest(alias: new string('a', length));

        // Act
        var result = _validator.Validate(request);

        // Assert
        result.IsValid.Should().Be(expectedValid);

        if (!expectedValid)
        {
            result.Errors.Should().Contain(e => e.PropertyName == nameof(UpdateHeroRequest.Alias));
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
        var request = CreateRequest(powerName: new string('a', length));

        // Act
        var result = _validator.Validate(request);

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
        var request = CreateRequest(powerLevel: powerLevel);

        // Act
        var result = _validator.Validate(request);

        // Assert
        result.IsValid.Should().Be(expectedValid);
    }

    [Fact]
    public void Validator_WithEmptyHeroId_ShouldFail()
    {
        // Arrange
        var request = CreateRequest(heroId: Guid.Empty);

        // Act
        var result = _validator.Validate(request);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(UpdateHeroRequest.HeroId));
    }

    [Fact]
    public void Validator_WithValidRequest_ShouldPass()
    {
        // Act
        var result = _validator.Validate(CreateRequest());

        // Assert
        result.IsValid.Should().BeTrue();
    }

    private static UpdateHeroRequest CreateRequest(
        string name = "Clark Kent",
        string alias = "Superman",
        Guid? heroId = null,
        string powerName = "Flight",
        int powerLevel = 8) =>
        new(
            name,
            alias,
            heroId ?? Guid.CreateVersion7(),
            [new UpdateHeroRequest.HeroPowerDto(powerName, powerLevel)]);
}
