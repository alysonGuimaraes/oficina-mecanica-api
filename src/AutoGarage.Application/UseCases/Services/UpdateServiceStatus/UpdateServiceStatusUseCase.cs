using AutoGarage.Application.Commands.Services;
using AutoGarage.Application.Services.Status;
using AutoGarage.Communication.Responses.Services;
using AutoGarage.Domain.Exceptions.Services.Exceptions;
using AutoGarage.Domain.Repositories;
using AutoGarage.Domain.Repositories.Services;
using AutoGarage.Domain.Shared;
using MapsterMapper;

namespace AutoGarage.Application.UseCases.Services.UpdateServiceStatus;

public class UpdateServiceStatusUseCase : IUpdateServiceStatusUseCase
{
    private readonly IServiceReadOnlyRepository _serviceReadOnlyRepository;
    private readonly IServiceWriteOnlyRepository _serviceWriteOnlyRepository;
    private readonly IUnitWork _workUnit;
    private readonly IMapper _mapper;

    public UpdateServiceStatusUseCase(IServiceReadOnlyRepository serviceReadOnlyRepository, IServiceWriteOnlyRepository serviceWriteOnlyRepository, IUnitWork workUnit, IMapper mapper)
    {
        _serviceReadOnlyRepository = serviceReadOnlyRepository;
        _serviceWriteOnlyRepository = serviceWriteOnlyRepository;
        _workUnit = workUnit;
        _mapper = mapper;
    }

    public async Task<ServiceResponse> Handle(UpdateServiceStatusCommand command, CancellationToken cancellationToken)
    {
        if (!Guid.TryParse(command.Id, out var serviceId))
        {
            throw new InvalidServiceIdException();
        }

        // Status ausente, vazio ou inválido é rejeitado: não existe padrão para a transição.
        if (!EntityStatusParser.TryParse(command.Status, out var status))
        {
            throw new InvalidServiceStatusException();
        }

        var service = await _serviceReadOnlyRepository.GetById(serviceId, cancellationToken)
            ?? throw new ServiceNotFoundException(serviceId);

        if (status == EntityStatus.ACTIVE)
        {
            service.Activate();
        }
        else
        {
            service.Deactivate();
        }

        await _serviceWriteOnlyRepository.Update(service, cancellationToken);
        await _workUnit.Commit();

        return _mapper.Map<ServiceResponse>(service);
    }
}
