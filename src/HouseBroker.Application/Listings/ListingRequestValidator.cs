using FluentValidation;
using HouseBroker.Domain.Constants;

namespace HouseBroker.Application.Listings;

/// <summary>Validation rules for creating or updating a listing. Limits come from <see cref="ListingConstraints"/>.</summary>
public sealed class ListingRequestValidator : AbstractValidator<ListingRequest>
{
    public ListingRequestValidator()
    {
        RuleFor(x => x.Title).NotEmpty().MaximumLength(ListingConstraints.TitleMaxLength);
        RuleFor(x => x.Description).NotNull().MaximumLength(ListingConstraints.DescriptionMaxLength);
        RuleFor(x => x.PropertyType).IsInEnum();
        RuleFor(x => x.Price).GreaterThan(0).LessThanOrEqualTo(ListingConstraints.MaxPrice)
            .PrecisionScale(ListingConstraints.MoneyPrecision, ListingConstraints.MoneyScale, ignoreTrailingZeros: true);
        RuleFor(x => x.City).NotEmpty().MaximumLength(ListingConstraints.CityMaxLength);
        RuleFor(x => x.Area).NotNull().MaximumLength(ListingConstraints.AreaMaxLength);
        RuleFor(x => x.Address).NotNull().MaximumLength(ListingConstraints.AddressMaxLength);
        RuleFor(x => x.Bedrooms).InclusiveBetween(0, ListingConstraints.MaxRooms);
        RuleFor(x => x.Bathrooms).InclusiveBetween(0, ListingConstraints.MaxRooms);
        RuleFor(x => x.AreaSqFt).GreaterThanOrEqualTo(0).LessThan(ListingConstraints.MaxAreaSqFt);

        RuleFor(x => x.Features).Must(f => f is null || f.Count <= ListingConstraints.MaxFeatures)
            .WithMessage($"A listing can have at most {ListingConstraints.MaxFeatures} features.");
        RuleForEach(x => x.Features).NotEmpty().MaximumLength(ListingConstraints.FeatureMaxLength);

        RuleFor(x => x.ImageUrls).Must(i => i is null || i.Count <= ListingConstraints.MaxImages)
            .WithMessage($"A listing can have at most {ListingConstraints.MaxImages} images.");
        RuleForEach(x => x.ImageUrls).NotEmpty().MaximumLength(ListingConstraints.ImageUrlMaxLength).Must(BeHttpUrl)
            .WithMessage("Image URLs must be absolute http or https URLs.");
    }

    private static bool BeHttpUrl(string url) =>
        Uri.TryCreate(url, UriKind.Absolute, out var uri) && (uri.Scheme == Uri.UriSchemeHttps || uri.Scheme == Uri.UriSchemeHttp);
}
