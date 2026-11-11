using FluentValidation;

namespace HouseBroker.Application.Listings;

/// <summary>Validation rules for listing search filters.</summary>
public sealed class ListingSearchQueryValidator : AbstractValidator<ListingSearchQuery>
{
    public const int MaxPageSize = 50;

    public ListingSearchQueryValidator()
    {
        RuleFor(x => x.Location).MaximumLength(100);
        RuleFor(x => x.City).MaximumLength(100);
        RuleFor(x => x.PropertyType).IsInEnum();
        RuleFor(x => x.SortBy).IsInEnum();
        RuleFor(x => x.MinPrice).GreaterThanOrEqualTo(0);
        RuleFor(x => x.MaxPrice).GreaterThanOrEqualTo(0);
        RuleFor(x => x.MaxPrice).GreaterThanOrEqualTo(x => x.MinPrice)
            .When(x => x.MinPrice.HasValue && x.MaxPrice.HasValue)
            .WithMessage("MaxPrice must be greater than or equal to MinPrice.");
        RuleFor(x => x.MinBedrooms).InclusiveBetween(0, 100);
        RuleFor(x => x.MinBathrooms).InclusiveBetween(0, 100);
        RuleFor(x => x.Features).Must(f => f is null || f.Count <= 10)
            .WithMessage("Filter by at most 10 features.");
        RuleForEach(x => x.Features).NotEmpty().MaximumLength(50);
        RuleFor(x => x.Page).GreaterThanOrEqualTo(1);
        RuleFor(x => x.PageSize).InclusiveBetween(1, MaxPageSize);
    }
}
