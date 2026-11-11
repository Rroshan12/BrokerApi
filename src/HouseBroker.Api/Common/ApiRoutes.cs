namespace HouseBroker.Api.Common;

/// <summary>Route prefix shared by all controllers. The version is a URL segment: <c>api/v1/...</c>.</summary>
public static class ApiRoutes
{
    public const string Prefix = "api/v{version:apiVersion}";
}
