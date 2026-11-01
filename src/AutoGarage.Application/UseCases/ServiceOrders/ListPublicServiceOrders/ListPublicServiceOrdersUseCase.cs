using AutoGarage.Communication.Responses.ServiceOrders;
using AutoGarage.Domain.Common.Exceptions;
using AutoGarage.Domain.Enums.ServiceOrders;
using AutoGarage.Domain.Repositories.Customers;
using AutoGarage.Domain.Repositories.ServiceOrders.ServiceOrders;
using AutoGarage.Domain.ValueObjects;
using MapsterMapper;

namespace AutoGarage.Application.UseCases.ServiceOrders.ListPublicServiceOrders;

public class ListPublicServiceOrdersUseCase : IListPublicServiceOrdersUseCase
{
    private readonly ICustomerReadOnlyRepository _customerReadOnlyRepository;
    private readonly IServiceOrderReadOnlyRepository _serviceOrderReadOnlyRepository;
    private readonly IMapper _mapper;

    public ListPublicServiceOrdersUseCase(
        ICustomerReadOnlyRepository customerReadOnlyRepository,
        IServiceOrderReadOnlyRepository serviceOrderReadOnlyRepository,
        IMapper mapper)
    {
        _customerReadOnlyRepository = customerReadOnlyRepository;
        _serviceOrderReadOnlyRepository = serviceOrderReadOnlyRepository;
        _mapper = mapper;
    }

    public async Task<IReadOnlyList<ServiceOrderResponse>> Handle(string? document, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(document))
        {
            throw new ErrorOnValidationException(["The document field is required."]);
        }

        var documentValue = new Document(document);

        var customer = await _customerReadOnlyRepository.GetByDocument(documentValue.Value, cancellationToken);

        if (customer is null)
        {
            return [];
        }

        var orders = await _serviceOrderReadOnlyRepository.GetAllByCustomerExcludingStatus(
            customer.Id,
            ServiceOrderStatus.DELIVERED,
            cancellationToken);

        return _mapper.Map<List<ServiceOrderResponse>>(orders);
    }
}
