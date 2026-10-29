using AutoGarage.Application.Services.Authentication;
using AutoGarage.Domain.Entities;

namespace AutoGarage.CommonTestUtilities.Services;

public class FakeAccessTokenGenerator : IAccessTokenGenerator
{
    public (string Token, DateTime ExpiresAt) Generate(Employee employee)
    {
        return ($"token-{employee.Id}", DateTime.UtcNow.AddMinutes(60));
    }
}
