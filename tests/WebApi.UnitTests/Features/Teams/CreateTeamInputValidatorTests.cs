using SSW.VerticalSliceArchitecture.Domain.Teams;
using SSW.VerticalSliceArchitecture.Features.Teams.CreateTeam;

namespace SSW.VerticalSliceArchitecture.UnitTests.Features.Teams;

public class CreateTeamInputValidatorTests
{
    private readonly CreateTeamInputValidator _validator = new();

    [Theory]
    [InlineData(0, false)]
    [InlineData(1, true)]
    [InlineData(Team.NameMaxLength, true)]
    [InlineData(Team.NameMaxLength + 1, false)]
    public void Validator_WithNameOfLength_ShouldMatchDomainLimit(int length, bool expectedValid)
    {
        // Arrange
        var input = new CreateTeamInput(new string('a', length));

        // Act
        var result = _validator.Validate(input);

        // Assert
        result.IsValid.Should().Be(expectedValid);

        if (!expectedValid)
        {
            result.Errors.Should().Contain(e => e.PropertyName == nameof(CreateTeamInput.Name));
        }
    }

    [Fact]
    public void Validator_WithValidRequest_ShouldPass()
    {
        // Act
        var result = _validator.Validate(new CreateTeamInput("Justice League"));

        // Assert
        result.IsValid.Should().BeTrue();
    }
}
