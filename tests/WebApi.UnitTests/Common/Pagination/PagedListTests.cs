using SSW.VerticalSliceArchitecture.Common.Pagination;

namespace SSW.VerticalSliceArchitecture.UnitTests.Common.Pagination;

public class PagedListTests
{
    [Theory]
    [InlineData(0, 10, 0)]
    [InlineData(10, 10, 1)]
    [InlineData(11, 10, 2)]
    [InlineData(25, 10, 3)]
    public void TotalPages_ShouldRoundUp(int totalCount, int pageSize, int expected)
    {
        // Act
        var page = new PagedList<string>([], 1, pageSize, totalCount);

        // Assert
        page.TotalPages.Should().Be(expected);
    }

    [Fact]
    public void TotalPages_ShouldBeZero_WhenPageSizeIsZero()
    {
        // A page size of zero can't come from an endpoint (PagingParams clamps it), but the envelope is
        // public. The division is in double, so it yields Infinity rather than throwing, and
        // (int)Math.Ceiling(Infinity) is int.MinValue — the guard is what stops a negative totalPages
        // reaching a client.
        var page = new PagedList<string>([], 1, 0, 10);

        page.TotalPages.Should().Be(0);
    }

    [Theory]
    [InlineData(1, 25, false, true)]
    [InlineData(2, 25, true, true)]
    [InlineData(3, 25, true, false)]
    [InlineData(1, 0, false, false)]
    public void HasPreviousAndNextPage_ShouldReflectPosition(
        int page,
        int totalCount,
        bool expectedHasPrevious,
        bool expectedHasNext)
    {
        // Act
        var pagedList = new PagedList<string>([], page, 10, totalCount);

        // Assert
        pagedList.HasPreviousPage.Should().Be(expectedHasPrevious);
        pagedList.HasNextPage.Should().Be(expectedHasNext);
    }
}
