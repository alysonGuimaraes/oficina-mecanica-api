using AutoGarage.Application.Services.Cryptography;
using AutoGarage.Domain.Entities;
using AutoGarage.Domain.Enums.Employees;
using Microsoft.EntityFrameworkCore;

namespace AutoGarage.Infrastructure.Persistence.Seeds;

// Creates the first employee (admin) so the system can be used after the first deploy.
// The credentials come from configuration (Settings:BootstrapAdmin), never from the code.
// It only creates the employee when that email does not exist yet: later deploys never change its password.
public static class BootstrapAdminSeeder
{
    public const string AdminName = "Admin";

    private const int MinimumPasswordLength = 6;

    public static async Task EnsureAdminAsync(
        AppDbContext context,
        PasswordEncripter passwordEncripter,
        string? email,
        string? password,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
        {
            return;
        }

        if (password.Length < MinimumPasswordLength)
        {
            throw new ArgumentException($"The bootstrap admin password must be at least {MinimumPasswordLength} characters long.", nameof(password));
        }

        if (await context.Employees.AnyAsync(e => e.Email == email, cancellationToken))
        {
            return;
        }

        // Role ATTENDANT: the domain has no admin role, and the current roles grant the same access.
        var admin = Employee.Create(AdminName, email, passwordEncripter.Encrypt(password), EmployeeRole.ATTENDANT);

        context.Employees.Add(admin);

        await new WorkUnit(context).Commit();
    }
}
