using SSW.VerticalSliceArchitecture.Common.Domain.Teams;
using SSW.VerticalSliceArchitecture.Features.Teams.CreateTeam;

namespace SSW.VerticalSliceArchitecture.UnitTests.Features.Teams;

public class CreateTeamRequestValidatorTests
{
    private readonly CreateTeamRequestValidator _validator = new();

    [Theory]
    [InlineData(0, false)]
    [InlineData(1, true)]
    [InlineData(Team.NameMaxLength, true)]
    [InlineData(Team.NameMaxLength + 1, false)]
    public void Validator_WithNameOfLength_ShouldMatchDomainLimit(int length, bool expectedValid)
    {
        // Arrange
        var request = new CreateTeamRequest(new string('a', length));

        // Act
        var result = _validator.Validate(request);

        // Assert
        result.IsValid.Should().Be(expectedValid);

        if (!expectedValid)
        {
            result.Errors.Should().Contain(e => e.PropertyName == nameof(CreateTeamRequest.Name));
        }
    }

    [Fact]
    public void Validator_WithValidRequest_ShouldPass()
    {
        // Act
        var result = _validator.Validate(new CreateTeamRequest("Justice League"));

        // Assert
        result.IsValid.Should().BeTrue();
    }
}
