using SSW.VerticalSliceArchitecture.Common.Pagination;

namespace SSW.VerticalSliceArchitecture.UnitTests.Common.Pagination;

public class PagingParamsTests
{
    [Theory]
    [InlineData(1, 1)]
    [InlineData(5, 5)]
    [InlineData(0, PagingParams.FirstPage)]
    [InlineData(-3, PagingParams.FirstPage)]
    public void From_ShouldClampPageToFirstPage_WhenBelowRange(int requested, int expected)
    {
        // Act
        var paging = PagingParams.From(requested, PagingParams.DefaultPageSize);

        // Assert
        paging.Page.Should().Be(expected);
    }

    [Theory]
    [InlineData(10, 10)]
    [InlineData(PagingParams.MaxPageSize, PagingParams.MaxPageSize)]
    [InlineData(PagingParams.MaxPageSize + 1, PagingParams.MaxPageSize)]
    [InlineData(5000, PagingParams.MaxPageSize)]
    [InlineData(0, PagingParams.MinPageSize)]
    [InlineData(-1, PagingParams.MinPageSize)]
    public void From_ShouldClampPageSizeIntoRange(int requested, int expected)
    {
        // Act
        var paging = PagingParams.From(PagingParams.FirstPage, requested);

        // Assert
        paging.PageSize.Should().Be(expected);
    }

    [Theory]
    [InlineData(1, 10, 0)]
    [InlineData(2, 10, 10)]
    [InlineData(3, 25, 50)]
    public void Skip_ShouldBeRowsBeforeThePage(int page, int pageSize, int expectedSkip)
    {
        // Act
        var paging = PagingParams.From(page, pageSize);

        // Assert
        paging.Skip.Should().Be(expectedSkip);
    }

    [Fact]
    public void Skip_ShouldNeverBeNegative_WhenPageIsOutOfRange()
    {
        // Act
        var paging = PagingParams.From(-10, PagingParams.DefaultPageSize);

        // Assert
        paging.Skip.Should().Be(0);
    }
}
