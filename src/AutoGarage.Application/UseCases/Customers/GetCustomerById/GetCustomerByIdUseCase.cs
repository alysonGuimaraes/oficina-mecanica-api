using AutoGarage.Communication.Responses.Customers;
using AutoGarage.Domain.Exceptions.Customers.Exceptions;
using AutoGarage.Domain.Repositories.Customers;
using MapsterMapper;

namespace AutoGarage.Application.UseCases.Customers.GetCustomerById;

public class GetCustomerByIdUseCase : IGetCustomerByIdUseCase
{
    private readonly ICustomerReadOnlyRepository _customerReadOnlyRepository;
    private readonly IMapper _mapper;

    public GetCustomerByIdUseCase(ICustomerReadOnlyRepository customerReadOnlyRepository, IMapper mapper)
    {
        _customerReadOnlyRepository = customerReadOnlyRepository;
        _mapper = mapper;
    }

    public async Task<CustomerResponse> Handle(string id, CancellationToken cancellationToken)
    {
        if (!Guid.TryParse(id, out var customerId))
        {
            throw new InvalidCustomerIdException();
        }

        var customer = await _customerReadOnlyRepository.GetById(customerId, cancellationToken)
            ?? throw new CustomerNotFoundException(customerId);

        return _mapper.Map<CustomerResponse>(customer);
    }
}
