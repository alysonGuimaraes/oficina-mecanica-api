using AutoGarage.Application.Services.ServiceOrders;
using AutoGarage.Communication.Responses.ServiceOrders;
using AutoGarage.Domain.Enums.ServiceOrders;
using AutoGarage.Domain.Exceptions.ServiceOrders.Exceptions;
using AutoGarage.Domain.Repositories.ServiceOrders.ServiceOrders;
using MapsterMapper;

namespace AutoGarage.Application.UseCases.ServiceOrders.ListServiceOrders;

public class ListServiceOrdersUseCase : IListServiceOrdersUseCase
{
    private readonly IServiceOrderReadOnlyRepository _serviceOrderReadOnlyRepository;
    private readonly IMapper _mapper;

    public ListServiceOrdersUseCase(IServiceOrderReadOnlyRepository serviceOrderReadOnlyRepository, IMapper mapper)
    {
        _serviceOrderReadOnlyRepository = serviceOrderReadOnlyRepository;
        _mapper = mapper;
    }

    public async Task<IReadOnlyList<ServiceOrderResponse>> Handle(string? status, CancellationToken cancellationToken)
    {
        ServiceOrderStatus? filter = null;

        if (!string.IsNullOrWhiteSpace(status))
        {
            if (!ServiceOrderStatusParser.TryParse(status, out var parsed))
            {
                throw new InvalidServiceOrderStatusException();
            }

            filter = parsed;
        }

        var orders = await _serviceOrderReadOnlyRepository.GetAll(filter, cancellationToken);

        return _mapper.Map<List<ServiceOrderResponse>>(orders);
    }
}
