using FluentValidation;
using HouseBroker.Application.Common.Pagination;
using HouseBroker.Domain.Constants;

namespace HouseBroker.Application.Listings;

/// <summary>Validation rules for listing search filters, plus the shared paging rules.</summary>
public sealed class ListingSearchQueryValidator : AbstractValidator<ListingSearchQuery>
{
    public const int MaxFeatureFilters = 10;

    public ListingSearchQueryValidator()
    {
        Include(new PageRequestValidator());

        RuleFor(x => x.Location).MaximumLength(ListingConstraints.AddressMaxLength);
        RuleFor(x => x.City).MaximumLength(ListingConstraints.CityMaxLength);
        RuleFor(x => x.PropertyType).IsInEnum();
        RuleFor(x => x.SortBy).IsInEnum();
        RuleFor(x => x.MinPrice).GreaterThanOrEqualTo(0);
        RuleFor(x => x.MaxPrice).GreaterThanOrEqualTo(0);
        RuleFor(x => x.MaxPrice).GreaterThanOrEqualTo(x => x.MinPrice)
            .When(x => x.MinPrice.HasValue && x.MaxPrice.HasValue)
            .WithMessage("MaxPrice must be greater than or equal to MinPrice.");
        RuleFor(x => x.MinBedrooms).InclusiveBetween(0, ListingConstraints.MaxRooms);
        RuleFor(x => x.MinBathrooms).InclusiveBetween(0, ListingConstraints.MaxRooms);
        RuleFor(x => x.Features).Must(f => f is null || f.Count <= MaxFeatureFilters)
            .WithMessage($"Filter by at most {MaxFeatureFilters} features.");
        RuleForEach(x => x.Features).NotEmpty().MaximumLength(ListingConstraints.FeatureMaxLength);
    }
}
