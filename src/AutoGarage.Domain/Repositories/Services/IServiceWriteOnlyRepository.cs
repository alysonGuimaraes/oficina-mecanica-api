using AutoGarage.Domain.Entities;

namespace AutoGarage.Domain.Repositories.Services;

public interface IServiceWriteOnlyRepository
{
    Task Add(Service service, CancellationToken cancellationToken);
}
