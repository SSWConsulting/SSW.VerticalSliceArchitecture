using SSW.VerticalSliceArchitecture.Common.Domain.Teams;

namespace SSW.VerticalSliceArchitecture.UnitTests.Features.Teams;

public class MissionTests
{
    [Theory]
    [InlineData("c8ad9974-ca93-44a5-9215-2f4d9e866c7a", "cc3431a8-4a31-4f76-af64-e8198279d7a4", false)]
    [InlineData("c8ad9974-ca93-44a5-9215-2f4d9e866c7a", "c8ad9974-ca93-44a5-9215-2f4d9e866c7a", true)]
    public void MissionId_ShouldBeComparable(string stringGuid1, string stringGuid2, bool isEqual)
    {
        // Arrange
        Guid guid1 = Guid.Parse(stringGuid1);
        Guid guid2 = Guid.Parse(stringGuid2);
        MissionId id1 = MissionId.From(guid1);
        MissionId id2 = MissionId.From(guid2);

        // Act
        var areEqual = id1 == id2;

        // Assert
        areEqual.Should().Be(isEqual);
        id1.Value.Should().Be(guid1);
        id2.Value.Should().Be(guid2);
    }
}