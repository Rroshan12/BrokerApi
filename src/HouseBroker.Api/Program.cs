// PRESENTATION LAYER / composition root: HTTP endpoints, auth, versioning, output caching, Swagger.
// Wires Application + Infrastructure together; contains no business logic.
using System.Text;
using System.Text.Json.Serialization;
using Asp.Versioning;
using HouseBroker.Api;
using HouseBroker.Api.Middleware;
using HouseBroker.Api.Services;
using HouseBroker.Application;
using HouseBroker.Application.Common;
using HouseBroker.Application.Common.Interfaces;
using HouseBroker.Infrastructure;
using HouseBroker.Infrastructure.Identity;
using HouseBroker.Infrastructure.Persistence.Seed;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();
builder.Services.AddScoped<ICacheInvalidator, OutputCacheInvalidator>();

// JWT auth. Jwt:Key comes from user-secrets (dev) or environment variables (prod).
var jwt = builder.Configuration.GetSection(JwtOptions.SectionName).Get<JwtOptions>() ?? new JwtOptions();
if (string.IsNullOrWhiteSpace(jwt.Key) || Encoding.UTF8.GetByteCount(jwt.Key) < 32)
    throw new InvalidOperationException("Jwt:Key is missing or shorter than 32 bytes. Run: dotnet user-secrets set \"Jwt:Key\" \"<32+ char secret>\"");

builder.Services
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
builder.Services.AddAuthorization();

// Controllers: enums as strings, null fields (e.g. commission for non-owners) left out of JSON.
builder.Services
    .AddControllers(o => o.SuppressAsyncSuffixInActionNames = false)
    .AddJsonOptions(o =>
    {
        o.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
        o.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
    });

// Errors: every error is RFC 7807 ProblemDetails, built through ProblemDetailsFactory.
builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

// API versioning: version in the URL (api/v1/...), reported in the api-supported-versions header.
builder.Services
    .AddApiVersioning(o =>
    {
        o.DefaultApiVersion = new ApiVersion(1, 0);
        o.ReportApiVersions = true;
        o.ApiVersionReader = new UrlSegmentApiVersionReader();
    })
    .AddMvc()
    .AddApiExplorer(o =>
    {
        o.GroupNameFormat = "'v'VVV";
        o.SubstituteApiVersionInUrl = true;
    });

// Output caching for public listing reads. The base policy skips requests with an
// Authorization header, so owner responses that include commission are never stored.
builder.Services.AddOutputCache(o =>
    o.AddPolicy(OutputCachePolicies.Listings, p => p.Expire(TimeSpan.FromSeconds(60)).Tag(CacheTags.Listings)));

// Third-party browser clients authenticate with bearer tokens (no cookies), so any origin is allowed.
builder.Services.AddCors(o => o.AddDefaultPolicy(p => p.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod()));

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
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

var app = builder.Build();

app.UseExceptionHandler();
app.UseStatusCodePages();

if (app.Environment.IsDevelopment())
{
    // Dev convenience: apply migrations and seed demo accounts/listings on startup.
    await app.Services.InitialiseDatabaseAsync(seedDemoData: true);
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors();
app.UseAuthentication();
app.UseAuthorization();
app.UseOutputCache();
app.MapControllers();

await app.RunAsync();

// Exposed so the API test project can reference the entry point.
public partial class Program;
