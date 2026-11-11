using System.Text;
using System.Text.Json.Serialization;
using Asp.Versioning;
using HouseBroker.Api.Common;
using HouseBroker.Api.Middleware;
using HouseBroker.Api.Services;
using HouseBroker.Application.Common;
using HouseBroker.Application.Common.Interfaces;
using HouseBroker.Infrastructure.Identity;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;

namespace HouseBroker.Api;

/// <summary>
/// PRESENTATION LAYER registration: controllers, JWT auth, API versioning, ProblemDetails,
/// output caching, CORS and Swagger, plus the HTTP adapters for Application interfaces.
/// </summary>
public static class DependencyInjection
{
    public static IServiceCollection AddPresentation(this IServiceCollection services, IConfiguration config)
    {
        services.AddHttpContextAccessor();
        services.AddScoped<ICurrentUserService, CurrentUserService>();
        services.AddScoped<ICacheInvalidator, OutputCacheInvalidator>();

        services
            .AddControllers(o => o.SuppressAsyncSuffixInActionNames = false)
            .AddJsonOptions(o =>
            {
                // Enums as strings; null fields (e.g. commission for non-owners) left out of the JSON.
                o.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
                o.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
            });

        // Every error is RFC 7807 ProblemDetails, built through ProblemDetailsFactory.
        services.AddProblemDetails();
        services.AddExceptionHandler<GlobalExceptionHandler>();

        // Third-party browser clients authenticate with bearer tokens (no cookies), so any origin is allowed.
        services.AddCors(o => o.AddDefaultPolicy(p => p.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod()));

        return services
            .AddJwtAuthentication(config)
            .AddUrlApiVersioning()
            .AddListingOutputCache()
            .AddSwaggerDocumentation();
    }

    private static IServiceCollection AddJwtAuthentication(this IServiceCollection services, IConfiguration config)
    {
        // Jwt:Key comes from user-secrets (dev) or environment variables (prod).
        var jwt = config.GetSection(JwtOptions.SectionName).Get<JwtOptions>() ?? new JwtOptions();
        if (string.IsNullOrWhiteSpace(jwt.Key) || Encoding.UTF8.GetByteCount(jwt.Key) < 32)
            throw new InvalidOperationException(
                "Jwt:Key is missing or shorter than 32 bytes. Run: dotnet user-secrets set \"Jwt:Key\" \"<32+ char secret>\"");

        services
            .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(o =>
            {
                o.MapInboundClaims = false;
                o.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = jwt.Issuer,
                    ValidAudience = jwt.Audience,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt.Key)),
                    NameClaimType = JwtRegisteredClaimNames.Sub,
                    RoleClaimType = JwtOptions.RoleClaimType,
                    ClockSkew = TimeSpan.FromMinutes(1)
                };
            });

        services.AddAuthorization();
        return services;
    }

    /// <summary>Version in the URL (api/v1/...), reported in the api-supported-versions header.</summary>
    private static IServiceCollection AddUrlApiVersioning(this IServiceCollection services)
    {
        services
            .AddApiVersioning(o =>
            {
                o.DefaultApiVersion = new ApiVersion(ApiVersions.V1);
                o.ReportApiVersions = true;
                o.ApiVersionReader = new UrlSegmentApiVersionReader();
            })
            .AddMvc()
            .AddApiExplorer(o =>
            {
                o.GroupNameFormat = "'v'VVV";
                o.SubstituteApiVersionInUrl = true;
            });
        return services;
    }

    /// <summary>
    /// Output caching for public listing reads. The base policy skips requests with an
    /// Authorization header, so owner responses that include commission are never stored.
    /// </summary>
    private static IServiceCollection AddListingOutputCache(this IServiceCollection services) =>
        services.AddOutputCache(o =>
            o.AddPolicy(OutputCachePolicies.Listings, p => p.Expire(OutputCachePolicies.ListingsTtl).Tag(CacheTags.Listings)));

    private static IServiceCollection AddSwaggerDocumentation(this IServiceCollection services)
    {
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "House Broker API",
                Version = "v1",
                Description = "Listings, search and broker commission. Log in via /api/v1/auth/login, then click Authorize and paste the token."
            });
            c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Type = SecuritySchemeType.Http,
                Scheme = "bearer",
                BearerFormat = "JWT",
                Description = "Paste the JWT returned by /api/v1/auth/login"
            });
            c.AddSecurityRequirement(doc => new OpenApiSecurityRequirement
            {
                [new OpenApiSecuritySchemeReference("Bearer", doc)] = []
            });
            foreach (var xml in new[] { "HouseBroker.Api.xml", "HouseBroker.Application.xml" })
            {
                var path = Path.Combine(AppContext.BaseDirectory, xml);
                if (File.Exists(path))
                    c.IncludeXmlComments(path);
            }
        });
        return services;
    }
}
