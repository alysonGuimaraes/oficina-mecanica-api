using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Text;

namespace AutoGarage.Infrastructure.Authentication;

// Single source of the token validation rules, used by the JWT Bearer middleware.
public static class JwtTokenValidation
{
    public static TokenValidationParameters Create(JwtSettings settings)
    {
        return new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = settings.Issuer,
            ValidateAudience = true,
            ValidAudience = settings.Audience,
            ValidateLifetime = true,
            // Zero tolerance: a token is refused the moment its exp passes.
            ClockSkew = TimeSpan.Zero,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(settings.Key)),
            NameClaimType = JwtRegisteredClaimNames.Sub
        };
    }
}
