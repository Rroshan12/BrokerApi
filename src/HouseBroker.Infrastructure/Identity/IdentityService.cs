using FluentValidation;
using FluentValidation.Results;
using HouseBroker.Application.Auth;
using HouseBroker.Application.Common.Exceptions;
using HouseBroker.Application.Listings;
using HouseBroker.Domain.Enums;
using HouseBroker.Infrastructure.Persistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace HouseBroker.Infrastructure.Identity;

/// <summary>
/// ASP.NET Identity adapter: creates users, checks passwords (with lockout) and
/// reads broker contact details for listing pages.
/// </summary>
public sealed class IdentityService(UserManager<ApplicationUser> userManager, AppDbContext db)
    : IIdentityService, IBrokerDirectory
{
    public async Task<AuthenticatedUser> CreateUserAsync(NewUser newUser, string password, string role, CancellationToken ct)
    {
        if (await userManager.FindByEmailAsync(newUser.Email) is not null)
            throw new ConflictException("An account with this email already exists.");

        var user = new ApplicationUser
        {
            UserName = newUser.Email,
            Email = newUser.Email,
            FullName = newUser.FullName,
            PhoneNumber = newUser.PhoneNumber,
            AgencyName = newUser.AgencyName
        };

        // User + role must be saved together, or a failed role leaves a user with no role.
        await using var transaction = await db.Database.BeginTransactionAsync(ct);
        ThrowIfFailed(await userManager.CreateAsync(user, password));
        ThrowIfFailed(await userManager.AddToRoleAsync(user, role));
        await transaction.CommitAsync(ct);

        return new AuthenticatedUser(user.Id, newUser.Email, user.FullName, role);
    }

    public async Task<AuthenticatedUser?> ValidateCredentialsAsync(string email, string password, CancellationToken ct)
    {
        var user = await userManager.FindByEmailAsync(email);
        if (user is null || await userManager.IsLockedOutAsync(user))
            return null;

        if (!await userManager.CheckPasswordAsync(user, password))
        {
            await userManager.AccessFailedAsync(user);
            return null;
        }

        await userManager.ResetAccessFailedCountAsync(user);
        var roles = await userManager.GetRolesAsync(user);
        var role = roles.Contains(Roles.Broker) ? Roles.Broker : Roles.Seeker;
        return new AuthenticatedUser(user.Id, user.Email ?? email, user.FullName, role);
    }

    public Task<BrokerContactDto?> GetContactAsync(string brokerId, CancellationToken ct) =>
        db.Users
            .AsNoTracking()
            .Where(u => u.Id == brokerId)
            .Select(u => new BrokerContactDto(u.Id, u.FullName, u.AgencyName, u.Email ?? string.Empty, u.PhoneNumber))
            .FirstOrDefaultAsync(ct);

    /// <summary>Turns Identity errors (weak password, bad email) into a 400 validation response.</summary>
    private static void ThrowIfFailed(IdentityResult result)
    {
        if (result.Succeeded)
            return;

        throw new ValidationException(result.Errors.Select(e => new ValidationFailure(
            e.Code.StartsWith("Password", StringComparison.Ordinal) ? "Password" : "Email",
            e.Description)));
    }
}
