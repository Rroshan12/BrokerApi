using FluentValidation;

namespace HouseBroker.Application.Common.Pagination;

/// <summary>Paging rules. Query validators reuse them with <c>Include(new PageRequestValidator())</c>.</summary>
public sealed class PageRequestValidator : AbstractValidator<PageRequest>
{
    public PageRequestValidator()
    {
        RuleFor(x => x.Page).GreaterThanOrEqualTo(1);
        RuleFor(x => x.PageSize).InclusiveBetween(1, PageRequest.MaxPageSize);
    }
}
