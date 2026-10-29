using AutoGarage.Domain.Entities;

namespace AutoGarage.Application.Services.Authentication;

public interface IAccessTokenGenerator
{
    (string Token, DateTime ExpiresAt) Generate(Employee employee);
}
