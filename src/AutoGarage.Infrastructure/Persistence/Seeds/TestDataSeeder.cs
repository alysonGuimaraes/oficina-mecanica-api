using AutoGarage.Application.Services.Cryptography;
using AutoGarage.Domain.Entities;
using AutoGarage.Domain.Enums.Employees;
using AutoGarage.Domain.Enums.ServiceOrders;
using AutoGarage.Domain.Enums.Vehicles;
using AutoGarage.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;

namespace AutoGarage.Infrastructure.Persistence.Seeds;

// Test data for the ephemeral environment (docker compose). Runs only when Settings:TestData:Enabled is true.
// Passwords go through the same PasswordEncripter as the application, so logins work with the configured pepper.
public static class TestDataSeeder
{
    public const string TestPassword = "Teste@123";

    private const string MechanicEmail = "mecanico@autogarage.local";

    public static async Task SeedAsync(AppDbContext context, PasswordEncripter passwordEncripter, CancellationToken cancellationToken)
    {
        // Idempotent: the mechanic is the marker of a previous seed run.
        if (await context.Employees.AnyAsync(e => e.Email == MechanicEmail, cancellationToken))
        {
            return;
        }

        var passwordHash = passwordEncripter.Encrypt(TestPassword);

        var mechanic = Employee.Create("Mecânico Teste", MechanicEmail, passwordHash, EmployeeRole.MECHANIC);
        var attendant = Employee.Create("Atendente Teste", "atendente@autogarage.local", passwordHash, EmployeeRole.ATTENDANT);
        var inactiveAttendant = Employee.Create("Atendente Inativo", "atendente.inativo@autogarage.local", passwordHash, EmployeeRole.ATTENDANT);
        inactiveAttendant.Deactivate();

        var activeService = Service.Create("Troca de óleo", "Troca de óleo e filtro", 150m);
        var inactiveService = Service.Create("Alinhamento", "Alinhamento de direção", 80m);
        inactiveService.Deactivate();

        var fullStockMaterial = Material.Create("Filtro de óleo", "Filtro de óleo do motor", 35.90m, 3);
        fullStockMaterial.IncreaseStock(10);
        var zeroStockMaterial = Material.Create("Pastilha de freio", "Jogo de pastilhas dianteiras", 120m, 2);

        // Three customers, three vehicles (one per customer) and three received orders (one per pair).
        var customers = new[]
        {
            Customer.Create("Cliente Um", new Document("529.982.247-25"), "cliente1@autogarage.local", "41", "999990001"),
            Customer.Create("Cliente Dois", new Document("390.533.447-05"), "cliente2@autogarage.local", "41", "999990002"),
            Customer.Create("Cliente Três", new Document("111.444.777-35"), "cliente3@autogarage.local", "41", "999990003")
        };

        var vehicles = new[]
        {
            Vehicle.Create("Ford", "Ka", new Plate("ABC1D23"), 2016, VehicleType.CAR, customers[0].Id),
            Vehicle.Create("Fiat", "Uno", new Plate("DEF2G34"), 2018, VehicleType.CAR, customers[1].Id),
            Vehicle.Create("VW", "Gol", new Plate("GHI3J45"), 2020, VehicleType.CAR, customers[2].Id)
        };

        var orders = new[]
        {
            ServiceOrder.Create(customers[0].Id, vehicles[0].Id),
            ServiceOrder.Create(customers[1].Id, vehicles[1].Id),
            ServiceOrder.Create(customers[2].Id, vehicles[2].Id)
        };

        var histories = orders.Select(order =>
            ServiceOrderStatusHistory.ForStatusChange(order.Id, null, ServiceOrderStatus.RECEIVED, mechanic.Id));

        context.Employees.AddRange(mechanic, attendant, inactiveAttendant);
        context.Services.AddRange(activeService, inactiveService);
        context.Materials.AddRange(fullStockMaterial, zeroStockMaterial);
        context.Customers.AddRange(customers);
        context.Vehicles.AddRange(vehicles);
        context.ServiceOrders.AddRange(orders);
        context.ServiceOrderStatusHistories.AddRange(histories);

        await new WorkUnit(context).Commit();
    }
}
