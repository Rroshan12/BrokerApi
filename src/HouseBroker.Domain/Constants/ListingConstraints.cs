namespace HouseBroker.Domain.Constants;

/// <summary>
/// Size limits for a listing. Used by the Application validators and the EF column
/// configuration, so request validation and the database schema cannot drift apart.
/// </summary>
public static class ListingConstraints
{
    public const int TitleMaxLength = 200;
    public const int DescriptionMaxLength = 4000;
    public const int CityMaxLength = 100;
    public const int AreaMaxLength = 100;
    public const int AddressMaxLength = 300;
    public const int ImageUrlMaxLength = 500;
    public const int FeatureMaxLength = 50;

    public const int MaxFeatures = 30;
    public const int MaxImages = 20;
    public const int MaxRooms = 100;

    public const decimal MaxPrice = 100_000_000_000m;
    public const decimal MaxAreaSqFt = 10_000_000m;

    /// <summary>Precision/scale of money columns (decimal(18,2)).</summary>
    public const int MoneyPrecision = 18;
    public const int MoneyScale = 2;
}
