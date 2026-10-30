using AutoGarage.Domain.Entities;
using AutoGarage.Domain.Repositories.Vehicles;
using AutoGarage.Domain.Shared;
using AutoGarage.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;

namespace AutoGarage.Infrastructure.Persistence.Repositories;

public class VehicleRepository : IVehicleReadOnlyRepository, IVehicleWriteOnlyRepository
{
    private readonly AppDbContext _context;

    public VehicleRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<bool> ExistsByPlate(string plate, CancellationToken cancellationToken)
    {
        var plateValue = new Plate(plate);

        return await _context.Vehicles
            .AnyAsync(v => v.Plate == plateValue, cancellationToken);
    }

    public async Task<IReadOnlyList<Vehicle>> GetByStatus(EntityStatus status, CancellationToken cancellationToken)
    {
        return await _context.Vehicles
            .AsNoTracking()
            .Where(v => v.Status == status)
            .OrderBy(v => v.CreatedAt)
            .ThenBy(v => v.Id)
            .ToListAsync(cancellationToken);
    }

    public async Task<Vehicle?> GetById(Guid id, CancellationToken cancellationToken)
    {
        return await _context.Vehicles
            .AsNoTracking()
            .FirstOrDefaultAsync(v => v.Id == id, cancellationToken);
    }

    public async Task<bool> ExistsByPlateExcludingId(string plate, Guid id, CancellationToken cancellationToken)
    {
        var plateValue = new Plate(plate);

        return await _context.Vehicles
            .AnyAsync(v => v.Id != id && v.Plate == plateValue, cancellationToken);
    }

    public async Task Add(Vehicle vehicle, CancellationToken cancellationToken)
    {
        await _context.Vehicles.AddAsync(vehicle, cancellationToken);
    }

    public Task Update(Vehicle vehicle, CancellationToken cancellationToken)
    {
        _context.Vehicles.Update(vehicle);

        return Task.CompletedTask;
    }
}
