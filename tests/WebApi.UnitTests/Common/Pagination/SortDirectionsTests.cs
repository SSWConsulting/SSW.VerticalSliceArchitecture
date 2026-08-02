using SSW.VerticalSliceArchitecture.Common.Pagination;

namespace SSW.VerticalSliceArchitecture.UnitTests.Common.Pagination;

public class SortDirectionsTests
{
    [Theory]
    [InlineData("asc", SortDirection.Ascending)]
    [InlineData("ASC", SortDirection.Ascending)]
    [InlineData("ascending", SortDirection.Ascending)]
    [InlineData("desc", SortDirection.Descending)]
    [InlineData("Descending", SortDirection.Descending)]
    public void From_ShouldParseKnownDirections_CaseInsensitively(string value, SortDirection expected)
    {
        SortDirections.From(value).Should().Be(expected);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void From_ShouldDefaultToAscending_WhenUnspecified(string? value)
    {
        SortDirections.From(value).Should().Be(SortDirection.Ascending);
    }

    [Fact]
    public void From_ShouldThrow_WhenDirectionIsUnknown()
    {
        var act = () => SortDirections.From("sideways");

        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Theory]
    [InlineData(null, true)]
    [InlineData("", true)]
    [InlineData("desc", true)]
    [InlineData("DESC", true)]
    [InlineData("sideways", false)]
    public void IsAllowed_ShouldMatchWhatFromAccepts(string? value, bool expected)
    {
        SortDirections.IsAllowed(value).Should().Be(expected);
    }
}
