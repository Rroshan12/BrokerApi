namespace HouseBroker.Api.Common;

/// <summary>Supported API versions.</summary>
public static class ApiVersions
{
    public const double V1 = 1.0;

    /// <summary>Route value for the <c>{version}</c> segment when generating v1 links.</summary>
    public const string V1RouteValue = "1";
}
