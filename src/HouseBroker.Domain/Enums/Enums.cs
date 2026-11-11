namespace HouseBroker.Domain.Enums;

public enum PropertyType
{
    House = 1,
    Apartment = 2,
    Land = 3,
    Commercial = 4
}

/// <summary>Role names used by ASP.NET Identity.</summary>
public static class Roles
{
    public const string Broker = "Broker";
    public const string Seeker = "Seeker";
}
