// PRESENTATION LAYER / composition root: wires Domain, Application, Infrastructure and the
// HTTP pipeline together. Contains no business logic; registrations live in each layer's
// DependencyInjection.cs (AddApplication, AddInfrastructure, AddPresentation).
using HouseBroker.Api;
using HouseBroker.Application;
using HouseBroker.Infrastructure;
using HouseBroker.Infrastructure.Persistence.Seed;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddApplication()
    .AddInfrastructure(builder.Configuration)
    .AddPresentation(builder.Configuration);

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
