using FluentValidation;
using HouseBroker.Application.Auth;
using HouseBroker.Application.Commission;
using HouseBroker.Application.Listings;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace HouseBroker.Application;

/// <summary>
/// APPLICATION LAYER: use cases (services), DTOs, validation and the interfaces
/// that the Infrastructure layer implements. Depends only on Domain.
/// </summary>
public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddValidatorsFromAssembly(typeof(DependencyInjection).Assembly);
        services.TryAddSingleton(TimeProvider.System);
        services.AddScoped<ICommissionCalculator, CommissionCalculator>();
        services.AddScoped<IListingService, ListingService>();
        services.AddScoped<IAuthService, AuthService>();
        return services;
    }
}
