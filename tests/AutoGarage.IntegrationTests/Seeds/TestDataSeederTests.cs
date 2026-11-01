using AutoGarage.Application.Services.Cryptography;
using AutoGarage.Domain.Entities;
using AutoGarage.Domain.Enums.Employees;
using AutoGarage.Domain.Enums.ServiceOrders;
using AutoGarage.Domain.Shared;
using AutoGarage.Infrastructure.Persistence;
using AutoGarage.Infrastructure.Persistence.Seeds;
using Microsoft.EntityFrameworkCore;
using Shouldly;

namespace AutoGarage.IntegrationTests.Seeds;

public class TestDataSeederTests
{
    private static readonly PasswordEncripter Encripter = new("test-pepper");

    private readonly string _databaseName = Guid.NewGuid().ToString();

    [Fact]
    public async Task SeedAsync_ShouldCreateOneActiveAndOneInactiveService()
    {
        await SeedAsync();

        await using var context = CreateContext();
        var services = await context.Services.ToListAsync();

        services.Count(s => s.Status == EntityStatus.ACTIVE).ShouldBe(1);
        services.Count(s => s.Status == EntityStatus.INACTIVE).ShouldBe(1);
    }

    [Fact]
    public async Task SeedAsync_ShouldCreateAFullStockMaterialAndAZeroStockMaterial_BothWithStockMinimumAboveZero()
    {
        await SeedAsync();

        await using var context = CreateContext();
        var materials = await context.Materials.ToListAsync();

        materials.Count.ShouldBe(2);
        materials.ShouldAllBe(m => m.StockMinimum > 0);
        materials.ShouldContain(m => m.StockQuantity > 0 && m.StockQuantity >= m.StockMinimum);
        materials.ShouldContain(m => m.StockQuantity == 0);
    }

    [Fact]
    public async Task SeedAsync_ShouldCreateThreeReceivedOrdersWithDistinctCustomersAndVehicles()
    {
        await SeedAsync();

        await using var context = CreateContext();
        var orders = await context.ServiceOrders.ToListAsync();
        var customers = await context.Customers.ToListAsync();
        var vehicles = await context.Vehicles.ToListAsync();

        orders.Count.ShouldBe(3);
        orders.ShouldAllBe(o => o.Status == ServiceOrderStatus.RECEIVED);
        orders.Select(o => o.CustomerId).Distinct().Count().ShouldBe(3);
        orders.Select(o => o.VehicleId).Distinct().Count().ShouldBe(3);

        customers.Count.ShouldBe(3);
        vehicles.Count.ShouldBe(3);
        orders.ShouldAllBe(o => customers.Any(c => c.Id == o.CustomerId));
        orders.ShouldAllBe(o => vehicles.Any(v => v.Id == o.VehicleId && v.CustomerId == o.CustomerId));
    }

    [Fact]
    public async Task SeedAsync_ShouldCreateMechanicAttendantAndInactiveAttendant_WithHashedPasswords()
    {
        await SeedAsync();

        await using var context = CreateContext();
        var employees = await context.Employees.ToListAsync();

        employees.Count.ShouldBe(3);
        employees.Count(e => e.Role == EmployeeRole.MECHANIC && e.Status == EntityStatus.ACTIVE).ShouldBe(1);
        employees.Count(e => e.Role == EmployeeRole.ATTENDANT && e.Status == EntityStatus.ACTIVE).ShouldBe(1);
        employees.Count(e => e.Role == EmployeeRole.ATTENDANT && e.Status == EntityStatus.INACTIVE).ShouldBe(1);

        foreach (var employee in employees)
        {
            employee.PasswordHash.ShouldNotBe(TestDataSeeder.TestPassword);
            Encripter.Verify(TestDataSeeder.TestPassword, employee.PasswordHash).ShouldBeTrue();
        }
    }

    [Fact]
    public async Task SeedAsync_CalledTwice_ShouldNotDuplicateRecords()
    {
        await SeedAsync();
        await SeedAsync();

        await using var context = CreateContext();

        (await context.Employees.CountAsync()).ShouldBe(3);
        (await context.Services.CountAsync()).ShouldBe(2);
        (await context.Materials.CountAsync()).ShouldBe(2);
        (await context.Customers.CountAsync()).ShouldBe(3);
        (await context.Vehicles.CountAsync()).ShouldBe(3);
        (await context.ServiceOrders.CountAsync()).ShouldBe(3);
    }

    private async Task SeedAsync()
    {
        await using var context = CreateContext();
        await TestDataSeeder.SeedAsync(context, Encripter, CancellationToken.None);
    }

    private AppDbContext CreateContext() =>
        new(new DbContextOptionsBuilder<AppDbContext>().UseInMemoryDatabase(_databaseName).Options);
}
