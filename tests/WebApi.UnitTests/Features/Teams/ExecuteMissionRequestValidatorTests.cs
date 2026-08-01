using SSW.VerticalSliceArchitecture.Common.Domain.Teams;
using SSW.VerticalSliceArchitecture.Features.Teams.ExecuteMission;

namespace SSW.VerticalSliceArchitecture.UnitTests.Features.Teams;

public class ExecuteMissionRequestValidatorTests
{
    private readonly ExecuteMissionRequestValidator _validator = new();

    [Theory]
    [InlineData(0, false)]
    [InlineData(1, true)]
    [InlineData(Mission.DescriptionMaxLength, true)]
    [InlineData(Mission.DescriptionMaxLength + 1, false)]
    public void Validator_WithDescriptionOfLength_ShouldMatchDomainLimit(int length, bool expectedValid)
    {
        // Arrange
        var request = CreateRequest(description: new string('a', length));

        // Act
        var result = _validator.Validate(request);

        // Assert
        result.IsValid.Should().Be(expectedValid);

        if (!expectedValid)
        {
            result.Errors.Should().Contain(e => e.PropertyName == nameof(ExecuteMissionRequest.Description));
        }
    }

    [Fact]
    public void Validator_WithEmptyTeamId_ShouldFail()
    {
        // Arrange
        var request = CreateRequest(teamId: Guid.Empty);

        // Act
        var result = _validator.Validate(request);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(ExecuteMissionRequest.TeamId));
    }

    [Fact]
    public void Validator_WithValidRequest_ShouldPass()
    {
        // Act
        var result = _validator.Validate(CreateRequest());

        // Assert
        result.IsValid.Should().BeTrue();
    }

    private static ExecuteMissionRequest CreateRequest(
        Guid? teamId = null,
        string description = "Save the city") =>
        new(teamId ?? Guid.CreateVersion7(), description);
}
