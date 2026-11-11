using FluentAssertions;
using HouseBroker.Application.Common.Pagination;

namespace HouseBroker.Application.Tests.Common;

public class PaginationTests
{
    [Theory]
    [InlineData(1, 10, 0)]
    [InlineData(2, 10, 10)]
    [InlineData(3, 25, 50)]
    public void Skip_PageAndSize_ReturnsItemsBeforePage(int page, int pageSize, int expectedSkip)
    {
        var paging = new PageRequest { Page = page, PageSize = pageSize };

        paging.Skip.Should().Be(expectedSkip);
    }

    [Fact]
    public void PageRequest_Defaults_AreFirstPageOfTen()
    {
        var paging = new PageRequest();

        paging.Page.Should().Be(1);
        paging.PageSize.Should().Be(PageRequest.DefaultPageSize);
    }

    [Theory]
    [InlineData(0, 10, false)]
    [InlineData(1, 0, false)]
    [InlineData(1, 51, false)]
    [InlineData(1, 50, true)]
    [InlineData(5, 1, true)]
    public void PageRequestValidator_Bounds_AreEnforced(int page, int pageSize, bool expectedValid)
    {
        var result = new PageRequestValidator().Validate(new PageRequest { Page = page, PageSize = pageSize });

        result.IsValid.Should().Be(expectedValid);
    }

    [Theory]
    [InlineData(1, 10, 0, 0, false, false)]
    [InlineData(1, 10, 25, 3, false, true)]
    [InlineData(2, 10, 25, 3, true, true)]
    [InlineData(3, 10, 25, 3, true, false)]
    public void PagedResult_PagingInfo_IsComputed(int page, int pageSize, int total, int expectedPages, bool hasPrevious, bool hasNext)
    {
        var result = new PagedResult<int>([], page, pageSize, total);

        result.TotalPages.Should().Be(expectedPages);
        result.HasPreviousPage.Should().Be(hasPrevious);
        result.HasNextPage.Should().Be(hasNext);
    }
}
