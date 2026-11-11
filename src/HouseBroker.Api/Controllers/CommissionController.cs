using Asp.Versioning;
using HouseBroker.Api.Common;
using HouseBroker.Application.Commission;
using HouseBroker.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HouseBroker.Api.Controllers;

/// <summary>Broker-only view of the commission engine and its DB-configured rates.</summary>
[ApiController]
[ApiVersion(ApiVersions.V1)]
[Route($"{ApiRoutes.Prefix}/commission")]
[Produces("application/json")]
[Authorize(Roles = Roles.Broker)]
public sealed class CommissionController(ICommissionCalculator calculator) : ControllerBase
{
    /// <summary>The active commission bands, lowest price first.</summary>
    [HttpGet("rules")]
    [ProducesResponseType<IReadOnlyList<CommissionRuleDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<IReadOnlyList<CommissionRuleDto>>> GetRulesAsync(CancellationToken ct) =>
        Ok(await calculator.GetActiveRulesAsync(ct));

    /// <summary>Estimate the commission for a price before creating a listing.</summary>
    [HttpGet("estimate")]
    [ProducesResponseType<CommissionResult>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<CommissionResult>> EstimateAsync([FromQuery] decimal price, CancellationToken ct) =>
        Ok(await calculator.CalculateAsync(price, ct));
}
