using AutoGarage.Application.Commands.Customers;
using AutoGarage.Application.Services.Status;
using AutoGarage.Communication.Responses.Customers;
using AutoGarage.Domain.Entities;
using AutoGarage.Domain.Exceptions.Customers.Exceptions;
using AutoGarage.Domain.Repositories;
using AutoGarage.Domain.Repositories.Customers;
using AutoGarage.Domain.Shared;
using MapsterMapper;

namespace AutoGarage.Application.UseCases.Customers.UpdateCustomerStatus;

public class UpdateCustomerStatusUseCase : IUpdateCustomerStatusUseCase
{
    private readonly ICustomerReadOnlyRepository _customerReadOnlyRepository;
    private readonly ICustomerWriteOnlyRepository _customerWriteOnlyRepository;
    private readonly IUnitWork _workUnit;
    private readonly IMapper _mapper;

    public UpdateCustomerStatusUseCase(ICustomerReadOnlyRepository customerReadOnlyRepository, ICustomerWriteOnlyRepository customerWriteOnlyRepository, IUnitWork workUnit, IMapper mapper)
    {
        _customerReadOnlyRepository = customerReadOnlyRepository;
        _customerWriteOnlyRepository = customerWriteOnlyRepository;
        _workUnit = workUnit;
        _mapper = mapper;
    }

    public async Task<CustomerResponse> Handle(UpdateCustomerStatusCommand command, CancellationToken cancellationToken)
    {
        if (!Guid.TryParse(command.Id, out var customerId))
        {
            throw new InvalidCustomerIdException();
        }

        if (!EntityStatusParser.TryParse(command.Status, out var status))
        {
            throw new InvalidCustomerStatusException();
        }

        var customer = await _customerReadOnlyRepository.GetById(customerId, cancellationToken)
            ?? throw new CustomerNotFoundException(customerId);

        if (status == EntityStatus.ACTIVE)
        {
            customer.Activate();
        }
        else
        {
            customer.Deactivate();
        }

        await _customerWriteOnlyRepository.Update(customer, cancellationToken);
        await _workUnit.Commit();

        return _mapper.Map<CustomerResponse>(customer);
    }
}
