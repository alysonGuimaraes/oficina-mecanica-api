using AutoGarage.Domain.Entities;
using AutoGarage.Domain.Exceptions.Employees.Exceptions;
using AutoGarage.Domain.Exceptions.ServiceOrders.Exceptions;
using AutoGarage.Domain.Repositories.Employees;
using AutoGarage.Domain.Repositories.ServiceOrders.ServiceOrders;
using AutoGarage.Domain.Shared;

namespace AutoGarage.Application.Services.ServiceOrders;

public class ServiceOrderLookup
{
    private readonly IServiceOrderReadOnlyRepository _serviceOrderReadOnlyRepository;
    private readonly IEmployeeReadOnlyRepository _employeeReadOnlyRepository;

    public ServiceOrderLookup(
        IServiceOrderReadOnlyRepository serviceOrderReadOnlyRepository,
        IEmployeeReadOnlyRepository employeeReadOnlyRepository)
    {
        _serviceOrderReadOnlyRepository = serviceOrderReadOnlyRepository;
        _employeeReadOnlyRepository = employeeReadOnlyRepository;
    }

    public async Task<ServiceOrder> GetOrder(string id, CancellationToken cancellationToken)
    {
        if (!Guid.TryParse(id, out var serviceOrderId))
        {
            throw new InvalidServiceOrderIdException();
        }

        return await _serviceOrderReadOnlyRepository.GetById(serviceOrderId, cancellationToken)
            ?? throw new ServiceOrderNotFoundException(serviceOrderId);
    }

    public async Task<Guid> EnsureActiveEmployee(string employeeId, CancellationToken cancellationToken)
    {
        if (!Guid.TryParse(employeeId, out var parsedId))
        {
            throw new InvalidEmployeeIdException();
        }

        var employee = await _employeeReadOnlyRepository.GetById(parsedId, cancellationToken)
            ?? throw new EmployeeNotFoundException(parsedId);

        if (employee.Status != EntityStatus.ACTIVE)
        {
            throw new ServiceOrderEmployeeInactiveException(parsedId);
        }

        return parsedId;
    }
}
