using AutoGarage.Communication.Responses.Services;
using AutoGarage.Domain.Exceptions.Services.Exceptions;
using AutoGarage.Domain.Repositories.Services;
using MapsterMapper;

namespace AutoGarage.Application.UseCases.Services.GetServiceById;

public class GetServiceByIdUseCase : IGetServiceByIdUseCase
{
    private readonly IServiceReadOnlyRepository _serviceReadOnlyRepository;
    private readonly IMapper _mapper;

    public GetServiceByIdUseCase(IServiceReadOnlyRepository serviceReadOnlyRepository, IMapper mapper)
    {
        _serviceReadOnlyRepository = serviceReadOnlyRepository;
        _mapper = mapper;
    }

    public async Task<ServiceResponse> Handle(string id, CancellationToken cancellationToken)
    {
        if (!Guid.TryParse(id, out var serviceId))
        {
            throw new InvalidServiceIdException();
        }

        var service = await _serviceReadOnlyRepository.GetById(serviceId, cancellationToken)
            ?? throw new ServiceNotFoundException(serviceId);

        return _mapper.Map<ServiceResponse>(service);
    }
}
