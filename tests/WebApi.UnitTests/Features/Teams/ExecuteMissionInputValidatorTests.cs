using SSW.VerticalSliceArchitecture.Domain.Teams;
using SSW.VerticalSliceArchitecture.Features.Teams.ExecuteMission;

namespace SSW.VerticalSliceArchitecture.UnitTests.Features.Teams;

public class ExecuteMissionInputValidatorTests
{
    private readonly ExecuteMissionInputValidator _validator = new();

    [Theory]
    [InlineData(0, false)]
    [InlineData(1, true)]
    [InlineData(Mission.DescriptionMaxLength, true)]
    [InlineData(Mission.DescriptionMaxLength + 1, false)]
    public void Validator_WithDescriptionOfLength_ShouldMatchDomainLimit(int length, bool expectedValid)
    {
        // Arrange
        var input = CreateInput(description: new string('a', length));

        // Act
        var result = _validator.Validate(input);

        // Assert
        result.IsValid.Should().Be(expectedValid);

        if (!expectedValid)
        {
            result.Errors.Should().Contain(e => e.PropertyName == nameof(ExecuteMissionInput.Description));
        }
    }

    [Fact]
    public void Validator_WithEmptyTeamId_ShouldFail()
    {
        // Arrange
        var input = CreateInput(teamId: TeamId.From(Guid.Empty));

        // Act
        var result = _validator.Validate(input);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(ExecuteMissionInput.TeamId));
    }

    [Fact]
    public void Validator_WithValidRequest_ShouldPass()
    {
        // Act
        var result = _validator.Validate(CreateInput());

        // Assert
        result.IsValid.Should().BeTrue();
    }

    private static ExecuteMissionInput CreateInput(
        TeamId? teamId = null,
        string description = "Save the city") =>
        new(teamId ?? TeamId.From(Guid.CreateVersion7()), description);
}
