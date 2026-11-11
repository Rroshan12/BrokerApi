using FluentAssertions;
using HouseBroker.Api.Controllers;
using HouseBroker.Application.Common;
using HouseBroker.Application.Listings;
using HouseBroker.Domain.Enums;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace HouseBroker.Api.Tests.Controllers;

public class ListingsControllerTests
{
    private readonly Mock<IListingService> _service = new();
    private readonly ListingsController _sut;

    public ListingsControllerTests()
    {
        _sut = new ListingsController(_service.Object)
        {
            ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() }
        };
    }

    private static ListingRequest Request() => new(
        "Flat", "Desc", PropertyType.Apartment, 4_000_000m, "Kathmandu", "Area", "Addr", 2, 1, 900m, [], []);

    private static ListingDetailsDto Details(Guid id) => new(
        id, "Flat", "Desc", PropertyType.Apartment, 4_000_000m, "Kathmandu", "Area", "Addr", 2, 1, 900m,
        [], [], null, DateTime.UtcNow, null, 80_000m, 2m);

    [Fact]
    public async Task SearchAsync_ReturnsOkWithPage()
    {
        var page = new PagedResult<ListingSummaryDto>([], 1, 10, 0);
        _service.Setup(s => s.SearchAsync(It.IsAny<ListingSearchQuery>(), It.IsAny<CancellationToken>())).ReturnsAsync(page);

        var result = await _sut.SearchAsync(new ListingSearchQuery(), CancellationToken.None);

        result.Result.Should().BeOfType<OkObjectResult>().Which.Value.Should().BeSameAs(page);
    }

    [Fact]
    public async Task GetByIdAsync_Exists_ReturnsOk()
    {
        var id = Guid.NewGuid();
        _service.Setup(s => s.GetByIdAsync(id, It.IsAny<CancellationToken>())).ReturnsAsync(Details(id));

        var result = await _sut.GetByIdAsync(id, CancellationToken.None);

        result.Result.Should().BeOfType<OkObjectResult>()
            .Which.Value.Should().BeOfType<ListingDetailsDto>().Which.Id.Should().Be(id);
    }

    [Fact]
    public async Task GetByIdAsync_Missing_ReturnsNotFound()
    {
        _service.Setup(s => s.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync((ListingDetailsDto?)null);

        var result = await _sut.GetByIdAsync(Guid.NewGuid(), CancellationToken.None);

        result.Result.Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task CreateAsync_ReturnsCreatedAtGetById()
    {
        var id = Guid.NewGuid();
        _service.Setup(s => s.CreateAsync(It.IsAny<ListingRequest>(), It.IsAny<CancellationToken>())).ReturnsAsync(Details(id));

        var result = await _sut.CreateAsync(Request(), CancellationToken.None);

        var created = result.Result.Should().BeOfType<CreatedAtActionResult>().Subject;
        created.StatusCode.Should().Be(StatusCodes.Status201Created);
        created.ActionName.Should().Be(nameof(ListingsController.GetByIdAsync));
        created.RouteValues!["id"].Should().Be(id);
    }

    [Fact]
    public async Task UpdateAsync_ReturnsNoContent()
    {
        var id = Guid.NewGuid();

        var result = await _sut.UpdateAsync(id, Request(), CancellationToken.None);

        result.Should().BeOfType<NoContentResult>();
        _service.Verify(s => s.UpdateAsync(id, It.IsAny<ListingRequest>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_ReturnsNoContent()
    {
        var id = Guid.NewGuid();

        var result = await _sut.DeleteAsync(id, CancellationToken.None);

        result.Should().BeOfType<NoContentResult>();
        _service.Verify(s => s.DeleteAsync(id, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetMineAsync_ReturnsOk()
    {
        _service.Setup(s => s.GetMineAsync(1, 10, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PagedResult<ListingSummaryDto>([], 1, 10, 0));

        var result = await _sut.GetMineAsync(CancellationToken.None);

        result.Result.Should().BeOfType<OkObjectResult>();
    }
}
