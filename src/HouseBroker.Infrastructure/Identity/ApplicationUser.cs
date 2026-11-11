using Microsoft.AspNetCore.Identity;

namespace HouseBroker.Infrastructure.Identity;

/// <summary>Identity user. Brokers' contact details are shown on listing detail pages.</summary>
public class ApplicationUser : IdentityUser
{
    public string FullName { get; set; } = string.Empty;
    public string? AgencyName { get; set; }
}
