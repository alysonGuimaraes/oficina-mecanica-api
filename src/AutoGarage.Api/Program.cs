using AutoGarage.Api.Authentication;
using AutoGarage.Api.Exceptions;
using AutoGarage.Application;
using AutoGarage.Application.Services.Cryptography;
using AutoGarage.Infrastructure;
using AutoGarage.Infrastructure.Migrations;
using AutoGarage.Infrastructure.Persistence;
using AutoGarage.Infrastructure.Persistence.Seeds;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers()
    .AddJsonOptions(options =>
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter(allowIntegerValues: false)));
builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<ExceptionHandler>();
builder.Services.AddOpenApi();
builder.Services.AddHealthChecks();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddApplication(builder.Configuration);
builder.Services.AddJwtAuthentication(builder.Configuration);

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthentication();

app.UseAuthorization();

app.MapHealthChecks("/health").AllowAnonymous();

app.MapControllers();

MigrateDatabase();

await SeedTestDataAsync();

await EnsureBootstrapAdminAsync();

await app.RunAsync();

void MigrateDatabase()
{
    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")!;

    var serviceScope = app.Services.GetRequiredService<IServiceScopeFactory>().CreateScope();

    DatabaseMigrations.Migrate(connectionString, serviceScope.ServiceProvider);
}

// Test data is opt-in: only the ephemeral environment sets Settings:TestData:Enabled.
async Task SeedTestDataAsync()
{
    if (!builder.Configuration.GetValue<bool>("Settings:TestData:Enabled"))
    {
        return;
    }

    using var scope = app.Services.CreateScope();

    await TestDataSeeder.SeedAsync(
        scope.ServiceProvider.GetRequiredService<AppDbContext>(),
        scope.ServiceProvider.GetRequiredService<PasswordEncripter>(),
        builder.Configuration["Settings:TestData:Password"] ?? string.Empty,
        CancellationToken.None);
}

// First employee for the first deploy. Without the configured credentials nothing is created.
async Task EnsureBootstrapAdminAsync()
{
    using var scope = app.Services.CreateScope();

    await BootstrapAdminSeeder.EnsureAdminAsync(
        scope.ServiceProvider.GetRequiredService<AppDbContext>(),
        scope.ServiceProvider.GetRequiredService<PasswordEncripter>(),
        builder.Configuration["Settings:BootstrapAdmin:Email"],
        builder.Configuration["Settings:BootstrapAdmin:Password"],
        CancellationToken.None);
}
