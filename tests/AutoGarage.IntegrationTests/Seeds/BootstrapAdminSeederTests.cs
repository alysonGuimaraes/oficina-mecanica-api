using AutoGarage.Application.Services.Cryptography;
using AutoGarage.Domain.Entities;
using AutoGarage.Domain.Enums.Employees;
using AutoGarage.Domain.Shared;
using AutoGarage.Infrastructure.Persistence;
using AutoGarage.Infrastructure.Persistence.Seeds;
using Microsoft.EntityFrameworkCore;
using Shouldly;

namespace AutoGarage.IntegrationTests.Seeds;

public class BootstrapAdminSeederTests
{
    private const string AdminEmail = "admin@autogarage.com";
    private const string AdminPassword = "Admin@2026";

    private static readonly PasswordEncripter Encripter = new("production-pepper");

    private readonly string _databaseName = Guid.NewGuid().ToString();

    [Fact]
    public async Task EnsureAdminAsync_WhenNoAdminExists_ShouldCreateActiveEmployeeWithHashedPassword()
    {
        await EnsureAdminAsync(AdminEmail, AdminPassword);

        await using var context = CreateContext();
        var admin = await context.Employees.SingleAsync();

        admin.Name.ShouldBe("Admin");
        admin.Email.ShouldBe(AdminEmail);
        admin.Role.ShouldBe(EmployeeRole.ATTENDANT);
        admin.Status.ShouldBe(EntityStatus.ACTIVE);
        admin.PasswordHash.ShouldNotBe(AdminPassword);
        Encripter.Verify(AdminPassword, admin.PasswordHash).ShouldBeTrue();
    }

    [Fact]
    public async Task EnsureAdminAsync_WhenAdminExists_ShouldNotCreateAgainOrChangePassword()
    {
        await EnsureAdminAsync(AdminEmail, AdminPassword);
        await EnsureAdminAsync(AdminEmail, "OutraSenha@2026");

        await using var context = CreateContext();
        var admins = await context.Employees.ToListAsync();

        admins.Count.ShouldBe(1);
        Encripter.Verify(AdminPassword, admins[0].PasswordHash).ShouldBeTrue();
        Encripter.Verify("OutraSenha@2026", admins[0].PasswordHash).ShouldBeFalse();
    }

    [Theory]
    [InlineData(null, AdminPassword)]
    [InlineData("", AdminPassword)]
    [InlineData(AdminEmail, null)]
    [InlineData(AdminEmail, "   ")]
    public async Task EnsureAdminAsync_WithoutEmailOrPassword_ShouldDoNothing(string? email, string? password)
    {
        await EnsureAdminAsync(email, password);

        await using var context = CreateContext();
        (await context.Employees.CountAsync()).ShouldBe(0);
    }

    [Fact]
    public async Task EnsureAdminAsync_PasswordShorterThanSixCharacters_ShouldThrowArgumentException()
    {
        await Should.ThrowAsync<ArgumentException>(() => EnsureAdminAsync(AdminEmail, "12345"));

        await using var context = CreateContext();
        (await context.Employees.CountAsync()).ShouldBe(0);
    }

    private async Task EnsureAdminAsync(string? email, string? password)
    {
        await using var context = CreateContext();
        await BootstrapAdminSeeder.EnsureAdminAsync(context, Encripter, email, password, CancellationToken.None);
    }

    private AppDbContext CreateContext() =>
        new(new DbContextOptionsBuilder<AppDbContext>().UseInMemoryDatabase(_databaseName).Options);
}
