using AutoGarage.Domain.Entities;
using AutoGarage.Domain.Repositories.Services;
using AutoGarage.Domain.Shared;

namespace AutoGarage.CommonTestUtilities.Repositories.Services;

/// <summary>
/// Repositório em memória. Nome pelo valor normalizado, como no repositório real.
/// </summary>
public class FakeServiceRepository : IServiceReadOnlyRepository, IServiceWriteOnlyRepository
{
    private readonly List<Service> _services = [];

    public IReadOnlyList<Service> Services => _services;

    public FakeServiceRepository WithService(Service service)
    {
        _services.Add(service);
        return this;
    }

    public Task<bool> ExistsByName(string name, CancellationToken cancellationToken)
    {
        var exists = _services.Any(s => s.Name == name);

        return Task.FromResult(exists);
    }

    public Task<IReadOnlyList<Service>> GetByStatus(EntityStatus status, CancellationToken cancellationToken)
    {
        IReadOnlyList<Service> ordered = _services
            .Where(s => s.Status == status)
            .OrderBy(s => s.CreatedAt)
            .ThenBy(s => s.Id)
            .ToList();

        return Task.FromResult(ordered);
    }

    public Task<Service?> GetById(Guid id, CancellationToken cancellationToken)
    {
        return Task.FromResult(_services.FirstOrDefault(s => s.Id == id));
    }

    public Task Add(Service service, CancellationToken cancellationToken)
    {
        _services.Add(service);

        return Task.CompletedTask;
    }
}
