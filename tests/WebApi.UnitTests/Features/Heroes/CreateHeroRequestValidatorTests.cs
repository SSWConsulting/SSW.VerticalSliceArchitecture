using SSW.VerticalSliceArchitecture.Common.Domain.Heroes;
using SSW.VerticalSliceArchitecture.Features.Heroes.CreateHero;

namespace SSW.VerticalSliceArchitecture.UnitTests.Features.Heroes;

public class CreateHeroRequestValidatorTests
{
    private readonly CreateHeroRequestValidator _validator = new();

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
        var request = CreateRequest(name: new string('a', length));

        // Act
        var result = _validator.Validate(request);

        // Assert
        result.IsValid.Should().Be(expectedValid);

        if (!expectedValid)
        {
            result.Errors.Should().Contain(e => e.PropertyName == nameof(CreateHeroRequest.Name));
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
            result.Errors.Should().Contain(e => e.PropertyName == nameof(CreateHeroRequest.Alias));
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
    public void Validator_WithValidRequest_ShouldPass()
    {
        // Act
        var result = _validator.Validate(CreateRequest());

        // Assert
        result.IsValid.Should().BeTrue();
    }

    private static CreateHeroRequest CreateRequest(
        string name = "Clark Kent",
        string alias = "Superman",
        string powerName = "Flight",
        int powerLevel = 8) =>
        new(name, alias, [new CreateHeroRequest.HeroPowerDto(powerName, powerLevel)]);
}
