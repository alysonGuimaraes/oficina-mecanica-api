using AutoGarage.Application.Services.Status;
using AutoGarage.Communication.Responses.Services;
using AutoGarage.Domain.Exceptions.Services.Exceptions;
using AutoGarage.Domain.Repositories.Services;
using AutoGarage.Domain.Shared;
using MapsterMapper;

namespace AutoGarage.Application.UseCases.Services.ListServices;

public class ListServicesUseCase : IListServicesUseCase
{
    private readonly IServiceReadOnlyRepository _serviceReadOnlyRepository;
    private readonly IMapper _mapper;

    public ListServicesUseCase(IServiceReadOnlyRepository serviceReadOnlyRepository, IMapper mapper)
    {
        _serviceReadOnlyRepository = serviceReadOnlyRepository;
        _mapper = mapper;
    }

    public async Task<IReadOnlyList<ServiceResponse>> Handle(string? status, CancellationToken cancellationToken)
    {
        var filter = EntityStatus.ACTIVE;

        if (!string.IsNullOrWhiteSpace(status) && !EntityStatusParser.TryParse(status, out filter))
        {
            throw new InvalidServiceStatusException();
        }

        var services = await _serviceReadOnlyRepository.GetByStatus(filter, cancellationToken);

        return _mapper.Map<List<ServiceResponse>>(services);
    }
}
