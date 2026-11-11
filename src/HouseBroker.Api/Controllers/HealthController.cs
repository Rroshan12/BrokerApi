using Asp.Versioning;
using HouseBroker.Api.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HouseBroker.Api.Controllers;

/// <summary>Simple liveness check for third-party consumers.</summary>
[ApiController]
[ApiVersion(ApiVersions.V1)]
[Route($"{ApiRoutes.Prefix}/health")]
[AllowAnonymous]
public sealed class HealthController : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public IActionResult Get() => Ok(new { status = "ok", utc = DateTime.UtcNow });
}
