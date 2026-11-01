using AutoGarage.Domain.Exceptions.Auth.Exceptions;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace AutoGarage.Infrastructure.Authentication;

public static class ClaimsPrincipalExtensions
{
    // The acting employee is the token subject. Clients cannot choose it.
    public static string GetEmployeeId(this ClaimsPrincipal user)
    {
        var subject = user.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;

        if (string.IsNullOrWhiteSpace(subject))
        {
            throw new MissingEmployeeIdentityException();
        }

        return subject;
    }
}
