using FluentAssertions;
using FluentValidation;
using HouseBroker.Application.Commission;
using HouseBroker.Application.Common;
using HouseBroker.Application.Common.Pagination;
using HouseBroker.Application.Common.Interfaces;
using HouseBroker.Application.Listings;
using HouseBroker.Domain.Entities;
using HouseBroker.Domain.Enums;
using HouseBroker.Application.Tests.TestDoubles;
using Moq;

namespace HouseBroker.Application.Tests.Listings;

public class ListingServiceTests
{
    private const string OwnerId = "broker-1";
    private const string OtherBrokerId = "broker-2";

    private readonly Mock<IListingRepository> _repo = new();
    private readonly Mock<ICommissionCalculator> _commission = new();
    private readonly Mock<IBrokerDirectory> _brokers = new();
    private readonly Mock<ICurrentUserService> _currentUser = new();
    private readonly Mock<ICacheInvalidator> _cache = new();
    private static readonly DateTime Now = new(2026, 6, 1, 12, 0, 0, DateTimeKind.Utc);
    private readonly ListingService _sut;

    public ListingServiceTests()
    {
        _commission
            .Setup(c => c.CalculateAsync(It.IsAny<decimal>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((decimal price, CancellationToken _) => new CommissionResult(price, 2m, price * 0.02m));
        _brokers
            .Setup(b => b.GetContactAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((string id, CancellationToken _) => new BrokerContactDto(id, "Asha", "Demo Realty", "a@x.com", "+977 980"));

        _sut = new ListingService(
            _repo.Object, _commission.Object, _brokers.Object, _currentUser.Object, _cache.Object,
            new FixedTimeProvider(Now),
            new ListingRequestValidator(), new ListingSearchQueryValidator(), new PageRequestValidator());
    }

    private void ActAs(string? userId, string? role = Roles.Broker)
    {
        _currentUser.Setup(u => u.UserId).Returns(userId);
        _currentUser.Setup(u => u.IsInRole(It.IsAny<string>())).Returns((string r) => r == role);
    }

    private static ListingRequest ValidRequest(decimal price = 4_000_000m) => new(
        "2BHK flat", "Nice flat", PropertyType.Apartment, price, "Kathmandu", "Baneshwor", "Ring Road",
        2, 1, 900m, ["Parking", "parking", " Lift "], ["https://img.example.com/1.jpg"]);

    private static PropertyListing ExistingListing(string brokerId = OwnerId) => new()
    {
        Id = Guid.NewGuid(),
        Title = "Old",
        Price = 3_000_000m,
        City = "Kathmandu",
        BrokerId = brokerId,
        CommissionAmount = 60_000m,
        CommissionRatePercent = 2m
    };

    private void SetupGet(PropertyListing listing) =>
        _repo.Setup(r => r.GetByIdAsync(listing.Id, It.IsAny<bool>(), It.IsAny<CancellationToken>())).ReturnsAsync(listing);

    // ---------- Create ----------

    [Fact]
    public async Task CreateAsync_ValidRequest_StoresCommissionAndOwner()
    {
        ActAs(OwnerId);
        PropertyListing? saved = null;
        _repo.Setup(r => r.Add(It.IsAny<PropertyListing>())).Callback<PropertyListing>(l => saved = l);

        var result = await _sut.CreateAsync(ValidRequest(4_000_000m), CancellationToken.None);

        saved.Should().NotBeNull();
        saved!.BrokerId.Should().Be(OwnerId);
        saved.CommissionAmount.Should().Be(80_000m);
        saved.CommissionRatePercent.Should().Be(2m);
        saved.Features.Should().Equal("Parking", "Lift");
        result.CommissionAmount.Should().Be(80_000m);
        result.Broker!.Id.Should().Be(OwnerId);
        _repo.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        _cache.Verify(c => c.EvictByTagAsync(CacheTags.Listings, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CreateAsync_InvalidRequest_ThrowsValidationExceptionAndSavesNothing()
    {
        ActAs(OwnerId);
        var request = ValidRequest() with { Title = "", Price = -5m };

        var act = () => _sut.CreateAsync(request, CancellationToken.None);

        (await act.Should().ThrowAsync<ValidationException>())
            .Which.Errors.Select(e => e.PropertyName).Should().Contain(["Title", "Price"]);
        _repo.Verify(r => r.Add(It.IsAny<PropertyListing>()), Times.Never);
    }

    [Fact]
    public async Task CreateAsync_CallerIsSeeker_ThrowsUnauthorized()
    {
        ActAs("seeker-1", Roles.Seeker);

        var act = () => _sut.CreateAsync(ValidRequest(), CancellationToken.None);

        await act.Should().ThrowAsync<UnauthorizedAccessException>();
    }

    // ---------- Update ----------

    [Fact]
    public async Task UpdateAsync_ByOwner_RecalculatesCommission()
    {
        ActAs(OwnerId);
        var listing = ExistingListing();
        SetupGet(listing);

        await _sut.UpdateAsync(listing.Id, ValidRequest(9_000_000m), CancellationToken.None);

        listing.Price.Should().Be(9_000_000m);
        listing.CommissionAmount.Should().Be(180_000m);
        listing.UpdatedAtUtc.Should().Be(Now);
        _repo.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        _cache.Verify(c => c.EvictByTagAsync(CacheTags.Listings, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_ByNonOwner_ThrowsUnauthorizedAndSavesNothing()
    {
        ActAs(OtherBrokerId);
        var listing = ExistingListing(OwnerId);
        SetupGet(listing);

        var act = () => _sut.UpdateAsync(listing.Id, ValidRequest(), CancellationToken.None);

        await act.Should().ThrowAsync<UnauthorizedAccessException>();
        listing.Title.Should().Be("Old");
        _repo.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task UpdateAsync_ListingMissing_ThrowsKeyNotFound()
    {
        ActAs(OwnerId);

        var act = () => _sut.UpdateAsync(Guid.NewGuid(), ValidRequest(), CancellationToken.None);

        await act.Should().ThrowAsync<KeyNotFoundException>();
    }

    // ---------- Delete ----------

    [Fact]
    public async Task DeleteAsync_ByOwner_RemovesAndEvictsCache()
    {
        ActAs(OwnerId);
        var listing = ExistingListing();
        SetupGet(listing);

        await _sut.DeleteAsync(listing.Id, CancellationToken.None);

        _repo.Verify(r => r.Remove(listing), Times.Once);
        _repo.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        _cache.Verify(c => c.EvictByTagAsync(CacheTags.Listings, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_ByNonOwner_ThrowsUnauthorized()
    {
        ActAs(OtherBrokerId);
        var listing = ExistingListing(OwnerId);
        SetupGet(listing);

        var act = () => _sut.DeleteAsync(listing.Id, CancellationToken.None);

        await act.Should().ThrowAsync<UnauthorizedAccessException>();
        _repo.Verify(r => r.Remove(It.IsAny<PropertyListing>()), Times.Never);
    }

    // ---------- Details: commission visibility ----------

    [Fact]
    public async Task GetByIdAsync_CallerIsOwner_IncludesCommission()
    {
        ActAs(OwnerId);
        var listing = ExistingListing(OwnerId);
        SetupGet(listing);

        var result = await _sut.GetByIdAsync(listing.Id, CancellationToken.None);

        result!.CommissionAmount.Should().Be(60_000m);
        result.CommissionRatePercent.Should().Be(2m);
    }

    [Fact]
    public async Task GetByIdAsync_CallerIsOtherBroker_HidesCommission()
    {
        ActAs(OtherBrokerId);
        var listing = ExistingListing(OwnerId);
        SetupGet(listing);

        var result = await _sut.GetByIdAsync(listing.Id, CancellationToken.None);

        result!.CommissionAmount.Should().BeNull();
        result.CommissionRatePercent.Should().BeNull();
    }

    [Fact]
    public async Task GetByIdAsync_AnonymousCaller_HidesCommissionButShowsBrokerContact()
    {
        ActAs(null, role: null);
        var listing = ExistingListing(OwnerId);
        SetupGet(listing);

        var result = await _sut.GetByIdAsync(listing.Id, CancellationToken.None);

        result!.CommissionAmount.Should().BeNull();
        result.Broker!.PhoneNumber.Should().Be("+977 980");
    }

    [Fact]
    public async Task GetByIdAsync_NotFound_ReturnsNull()
    {
        ActAs(null, role: null);

        var result = await _sut.GetByIdAsync(Guid.NewGuid(), CancellationToken.None);

        result.Should().BeNull();
    }

    // ---------- Search / mine ----------

    [Fact]
    public async Task SearchAsync_MinPriceAboveMaxPrice_ThrowsValidationException()
    {
        var query = new ListingSearchQuery { MinPrice = 5_000_000m, MaxPrice = 1_000_000m };

        var act = () => _sut.SearchAsync(query, CancellationToken.None);

        await act.Should().ThrowAsync<ValidationException>();
        _repo.Verify(r => r.SearchAsync(It.IsAny<ListingSearchQuery>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task SearchAsync_ValidQuery_ReturnsRepositoryPage()
    {
        var query = new ListingSearchQuery { City = "Kathmandu", PropertyType = PropertyType.House, Page = 2, PageSize = 5 };
        var page = new PagedResult<ListingSummaryDto>([], 2, 5, 0);
        _repo.Setup(r => r.SearchAsync(query, It.IsAny<CancellationToken>())).ReturnsAsync(page);

        var result = await _sut.SearchAsync(query, CancellationToken.None);

        result.Should().BeSameAs(page);
    }

    [Fact]
    public async Task GetMineAsync_ValidPaging_PassesItToRepository()
    {
        ActAs(OwnerId);
        var paging = new PageRequest { Page = 2, PageSize = 20 };
        _repo.Setup(r => r.GetByBrokerAsync(OwnerId, paging, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PagedResult<ListingSummaryDto>([], 2, 20, 0));

        var result = await _sut.GetMineAsync(paging, CancellationToken.None);

        result.Page.Should().Be(2);
        result.PageSize.Should().Be(20);
    }

    [Theory]
    [InlineData(0, 10)]
    [InlineData(1, 0)]
    [InlineData(1, 51)]
    public async Task GetMineAsync_InvalidPaging_ThrowsValidationException(int page, int pageSize)
    {
        ActAs(OwnerId);

        var act = () => _sut.GetMineAsync(new PageRequest { Page = page, PageSize = pageSize }, CancellationToken.None);

        await act.Should().ThrowAsync<ValidationException>();
        _repo.Verify(r => r.GetByBrokerAsync(It.IsAny<string>(), It.IsAny<PageRequest>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task SearchAsync_PageSizeAboveMax_ThrowsValidationException()
    {
        var query = new ListingSearchQuery { PageSize = PageRequest.MaxPageSize + 1 };

        var act = () => _sut.SearchAsync(query, CancellationToken.None);

        (await act.Should().ThrowAsync<ValidationException>())
            .Which.Errors.Should().Contain(e => e.PropertyName == nameof(PageRequest.PageSize));
    }

    [Fact]
    public async Task CreateAsync_SetsCreatedAtFromClock()
    {
        ActAs(OwnerId);
        PropertyListing? saved = null;
        _repo.Setup(r => r.Add(It.IsAny<PropertyListing>())).Callback<PropertyListing>(l => saved = l);

        await _sut.CreateAsync(ValidRequest(), CancellationToken.None);

        saved!.CreatedAtUtc.Should().Be(Now);
    }
}
