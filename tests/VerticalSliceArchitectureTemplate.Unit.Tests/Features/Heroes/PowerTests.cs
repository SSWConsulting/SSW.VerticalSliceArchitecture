using VerticalSliceArchitectureTemplate.Common.Domain.Heroes;

namespace VerticalSliceArchitectureTemplate.Unit.Tests.Features.Heroes;

public class PowerTests
{
    [Fact]
    public void Power_ShouldBeCreatable()
    {
        // Arrange
        var name = "PowerName";
        var powerLevel = 5;

        // Act
        var power = new Power(name, powerLevel);

        // Assert
        power.Should().NotBeNull();
        power.Name.Should().Be(name);
        power.PowerLevel.Should().Be(powerLevel);
    }

    [Fact]
    public void Power_ShouldBeComparable()
    {
        // Arrange
        var name = "PowerName";
        var powerLevel = 5;
        var power1 = new Power(name, powerLevel);
        var power2 = new Power(name, powerLevel);

        // Act
        var areEqual = power1 == power2;

        // Assert
        areEqual.Should().BeTrue();
    }

    [Theory]
    [InlineData(-1, true)]
    [InlineData(0, true)]
    [InlineData(1, false)]
    [InlineData(9, false)]
    [InlineData(10, false)]
    [InlineData(11, true)]
    public void Power_WithInvalidPowerLevel_ShouldThrow(int powerLevel, bool shouldThrow)
    {
        // Arrange
        var name = "PowerName";

        // Act
        var act = () => new Power(name, powerLevel);

        // Assert
        if (shouldThrow)
        {
            act.Should().ThrowExactly<ArgumentOutOfRangeException>();
        }
        else
        {
            act.Should().NotThrow();
        }
    }
}