using AutoGarage.Application.Services.Status;
using AutoGarage.Communication.Responses.Customers;
using AutoGarage.Domain.Exceptions.Customers.Exceptions;
using AutoGarage.Domain.Repositories.Customers;
using AutoGarage.Domain.Shared;
using MapsterMapper;

namespace AutoGarage.Application.UseCases.Customers.ListCustomers;

public class ListCustomersUseCase : IListCustomersUseCase
{
    private readonly ICustomerReadOnlyRepository _customerReadOnlyRepository;
    private readonly IMapper _mapper;

    public ListCustomersUseCase(ICustomerReadOnlyRepository customerReadOnlyRepository, IMapper mapper)
    {
        _customerReadOnlyRepository = customerReadOnlyRepository;
        _mapper = mapper;
    }

    public async Task<IReadOnlyList<CustomerResponse>> Handle(string? status, CancellationToken cancellationToken)
    {
        var filter = EntityStatus.ACTIVE;

        if (!string.IsNullOrWhiteSpace(status) && !EntityStatusParser.TryParse(status, out filter))
        {
            throw new InvalidCustomerStatusException();
        }

        var customers = await _customerReadOnlyRepository.GetByStatus(filter, cancellationToken);

        return _mapper.Map<List<CustomerResponse>>(customers);
    }
}
