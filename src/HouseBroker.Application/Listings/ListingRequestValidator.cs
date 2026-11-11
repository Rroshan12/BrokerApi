using FluentValidation;

namespace HouseBroker.Application.Listings;

/// <summary>Validation rules for creating or updating a listing. Limits match the database columns.</summary>
public sealed class ListingRequestValidator : AbstractValidator<ListingRequest>
{
    public const decimal MaxPrice = 100_000_000_000m;

    public ListingRequestValidator()
    {
        RuleFor(x => x.Title).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Description).NotNull().MaximumLength(4000);
        RuleFor(x => x.PropertyType).IsInEnum();
        RuleFor(x => x.Price).GreaterThan(0).LessThanOrEqualTo(MaxPrice)
            .PrecisionScale(18, 2, ignoreTrailingZeros: true);
        RuleFor(x => x.City).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Area).NotNull().MaximumLength(100);
        RuleFor(x => x.Address).NotNull().MaximumLength(300);
        RuleFor(x => x.Bedrooms).InclusiveBetween(0, 100);
        RuleFor(x => x.Bathrooms).InclusiveBetween(0, 100);
        RuleFor(x => x.AreaSqFt).GreaterThanOrEqualTo(0).LessThan(10_000_000);

        RuleFor(x => x.Features).Must(f => f is null || f.Count <= 30)
            .WithMessage("A listing can have at most 30 features.");
        RuleForEach(x => x.Features).NotEmpty().MaximumLength(50);

        RuleFor(x => x.ImageUrls).Must(i => i is null || i.Count <= 20)
            .WithMessage("A listing can have at most 20 images.");
        RuleForEach(x => x.ImageUrls).NotEmpty().MaximumLength(500).Must(BeHttpUrl)
            .WithMessage("Image URLs must be absolute http or https URLs.");
    }

    private static bool BeHttpUrl(string url) =>
        Uri.TryCreate(url, UriKind.Absolute, out var uri) && (uri.Scheme == Uri.UriSchemeHttps || uri.Scheme == Uri.UriSchemeHttp);
}
