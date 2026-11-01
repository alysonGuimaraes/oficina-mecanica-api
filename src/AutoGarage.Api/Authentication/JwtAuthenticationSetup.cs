using AutoGarage.Infrastructure.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;

namespace AutoGarage.Api.Authentication;

public static class JwtAuthenticationSetup
{
    // Every endpoint requires an authenticated user unless it is explicitly anonymous (login, health, public lookup).
    public static IServiceCollection AddJwtAuthentication(this IServiceCollection services, IConfiguration configuration)
    {
        var settings = configuration.GetSection("Settings:Jwt").Get<JwtSettings>() ?? new JwtSettings();

        services
            .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                // Keep the token claim names as issued ("sub", not the .NET NameIdentifier mapping).
                options.MapInboundClaims = false;
                options.TokenValidationParameters = JwtTokenValidation.Create(settings);
            });

        services.AddAuthorization(options =>
        {
            options.FallbackPolicy = new AuthorizationPolicyBuilder()
                .RequireAuthenticatedUser()
                .Build();
        });

        return services;
    }
}
