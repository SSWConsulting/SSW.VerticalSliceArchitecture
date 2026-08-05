using System.Linq.Expressions;
using SSW.VerticalSliceArchitecture.Common.Pagination;

namespace SSW.VerticalSliceArchitecture.UnitTests.Common.Pagination;

public class SortColumnMapTests
{
    private static SortColumnMap<Widget> Map() => new(
        defaultColumn: "name",
        columns: new Dictionary<string, Expression<Func<Widget, object?>>>
        {
            ["name"] = w => w.Name,
            ["size"] = w => w.Size
        });

    [Theory]
    [InlineData(null, true)]
    [InlineData("", true)]
    [InlineData("name", true)]
    [InlineData("NAME", true)]
    [InlineData("size", true)]
    [InlineData("password", false)]
    public void IsAllowed_ShouldOnlyAcceptMappedColumns(string? sortBy, bool expected)
    {
        Map().IsAllowed(sortBy).Should().Be(expected);
    }

    [Fact]
    public void AllowedColumns_ShouldListEveryMappedColumn()
    {
        Map().AllowedColumns.Should().BeEquivalentTo("name", "size");
    }

    [Fact]
    public void Ctor_ShouldThrow_WhenDefaultColumnIsNotMapped()
    {
        var act = () => new SortColumnMap<Widget>(
            defaultColumn: "missing",
            columns: new Dictionary<string, Expression<Func<Widget, object?>>>
            {
                ["name"] = w => w.Name
            });

        act.Should().Throw<ArgumentException>();
    }

    private sealed record Widget(string Name, int Size);
}
