using Asp.Versioning;
using HouseBroker.Application.Common;
using HouseBroker.Application.Listings;
using HouseBroker.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;

namespace HouseBroker.Api.Controllers;

/// <summary>
/// Property listings. Anyone can search and view; only brokers create, and only the owning
/// broker updates or deletes. Commission appears only in the owning broker's responses.
/// </summary>
[ApiController]
[ApiVersion(1.0)]
[Route("api/v{version:apiVersion}/listings")]
[Produces("application/json")]
public sealed class ListingsController(IListingService listings) : ControllerBase
{
    /// <summary>Search listings by location, price range, type, rooms and features.</summary>
    [HttpGet]
    [AllowAnonymous]
    [OutputCache(PolicyName = OutputCachePolicies.Listings)]
    [ProducesResponseType<PagedResult<ListingSummaryDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<PagedResult<ListingSummaryDto>>> SearchAsync(
        [FromQuery] ListingSearchQuery query, CancellationToken ct) =>
        Ok(await listings.SearchAsync(query, ct));

    /// <summary>Full details of one listing: images, description and broker contact.</summary>
    /// <remarks>Commission fields are returned only when the caller is the broker who owns the listing.</remarks>
    [HttpGet("{id:guid}")]
    [AllowAnonymous]
    [OutputCache(PolicyName = OutputCachePolicies.Listings)]
    [ProducesResponseType<ListingDetailsDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ListingDetailsDto>> GetByIdAsync(Guid id, CancellationToken ct)
    {
        var listing = await listings.GetByIdAsync(id, ct);
        return listing is null ? NotFound() : Ok(listing);
    }

    /// <summary>The calling broker's own listings, including commission.</summary>
    [HttpGet("mine")]
    [Authorize(Roles = Roles.Broker)]
    [ProducesResponseType<PagedResult<ListingSummaryDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<PagedResult<ListingSummaryDto>>> GetMineAsync(
        CancellationToken ct, [FromQuery] int page = 1, [FromQuery] int pageSize = 10) =>
        Ok(await listings.GetMineAsync(page, pageSize, ct));

    /// <summary>Create a listing. Its commission is calculated from the DB-configured rates.</summary>
    [HttpPost]
    [Authorize(Roles = Roles.Broker)]
    [ProducesResponseType<ListingDetailsDto>(StatusCodes.Status201Created)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<ListingDetailsDto>> CreateAsync(ListingRequest request, CancellationToken ct)
    {
        var created = await listings.CreateAsync(request, ct);
        return CreatedAtAction(nameof(GetByIdAsync), new { id = created.Id, version = "1" }, created);
    }

    /// <summary>Replace a listing you own. Commission is recalculated.</summary>
    [HttpPut("{id:guid}")]
    [Authorize(Roles = Roles.Broker)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateAsync(Guid id, ListingRequest request, CancellationToken ct)
    {
        await listings.UpdateAsync(id, request, ct);
        return NoContent();
    }

    /// <summary>Delete a listing you own.</summary>
    [HttpDelete("{id:guid}")]
    [Authorize(Roles = Roles.Broker)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteAsync(Guid id, CancellationToken ct)
    {
        await listings.DeleteAsync(id, ct);
        return NoContent();
    }
}
