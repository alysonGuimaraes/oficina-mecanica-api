using AutoGarage.Application.Commands.ServiceOrders;
using AutoGarage.Communication.Responses.ServiceOrders;
using AutoGarage.Domain.Common.Exceptions;
using AutoGarage.Domain.Entities;
using AutoGarage.Domain.Enums.ServiceOrders;
using AutoGarage.Domain.Exceptions.Customers.Exceptions;
using AutoGarage.Domain.Exceptions.Employees.Exceptions;
using AutoGarage.Domain.Exceptions.ServiceOrders.Exceptions;
using AutoGarage.Domain.Exceptions.Vehicles.Exceptions;
using AutoGarage.Domain.Repositories;
using AutoGarage.Domain.Repositories.Customers;
using AutoGarage.Domain.Repositories.Employees;
using AutoGarage.Domain.Repositories.ServiceOrders.ServiceOrders;
using AutoGarage.Domain.Repositories.ServiceOrders.ServiceOrderStatusHistories;
using AutoGarage.Domain.Repositories.Vehicles;
using AutoGarage.Domain.Shared;
using MapsterMapper;

namespace AutoGarage.Application.UseCases.ServiceOrders.CreateServiceOrder;

public class CreateServiceOrderUseCase : ICreateServiceOrderUseCase
{
    private readonly ICustomerReadOnlyRepository _customerReadOnlyRepository;
    private readonly IVehicleReadOnlyRepository _vehicleReadOnlyRepository;
    private readonly IEmployeeReadOnlyRepository _employeeReadOnlyRepository;
    private readonly IServiceOrderWriteOnlyRepository _serviceOrderWriteOnlyRepository;
    private readonly IServiceOrderStatusHistoryWriteOnlyRepository _serviceOrderStatusHistoryWriteOnlyRepository;
    private readonly IUnitWork _workUnit;
    private readonly IMapper _mapper;

    public CreateServiceOrderUseCase(
        ICustomerReadOnlyRepository customerReadOnlyRepository,
        IVehicleReadOnlyRepository vehicleReadOnlyRepository,
        IEmployeeReadOnlyRepository employeeReadOnlyRepository,
        IServiceOrderWriteOnlyRepository serviceOrderWriteOnlyRepository,
        IServiceOrderStatusHistoryWriteOnlyRepository serviceOrderStatusHistoryWriteOnlyRepository,
        IUnitWork workUnit,
        IMapper mapper)
    {
        _customerReadOnlyRepository = customerReadOnlyRepository;
        _vehicleReadOnlyRepository = vehicleReadOnlyRepository;
        _employeeReadOnlyRepository = employeeReadOnlyRepository;
        _serviceOrderWriteOnlyRepository = serviceOrderWriteOnlyRepository;
        _serviceOrderStatusHistoryWriteOnlyRepository = serviceOrderStatusHistoryWriteOnlyRepository;
        _workUnit = workUnit;
        _mapper = mapper;
    }

    public async Task<ServiceOrderResponse> Handle(CreateServiceOrderCommand command, CancellationToken cancellationToken)
    {
        Validate(command);

        if (!Guid.TryParse(command.CustomerId, out var customerId))
        {
            throw new InvalidCustomerIdException();
        }

        if (!Guid.TryParse(command.VehicleId, out var vehicleId))
        {
            throw new InvalidVehicleIdException();
        }

        if (!Guid.TryParse(command.EmployeeId, out var employeeId))
        {
            throw new InvalidEmployeeIdException();
        }

        var customer = await _customerReadOnlyRepository.GetById(customerId, cancellationToken)
            ?? throw new CustomerNotFoundException(customerId);

        if (customer.Status != EntityStatus.ACTIVE)
        {
            throw new ServiceOrderCustomerInactiveException(customerId);
        }

        var vehicle = await _vehicleReadOnlyRepository.GetById(vehicleId, cancellationToken)
            ?? throw new VehicleNotFoundException(vehicleId);

        if (vehicle.Status != EntityStatus.ACTIVE)
        {
            throw new ServiceOrderVehicleInactiveException(vehicleId);
        }

        if (vehicle.CustomerId != customerId)
        {
            throw new ServiceOrderVehicleCustomerMismatchException(vehicleId, customerId);
        }

        var employee = await _employeeReadOnlyRepository.GetById(employeeId, cancellationToken)
            ?? throw new EmployeeNotFoundException(employeeId);

        if (employee.Status != EntityStatus.ACTIVE)
        {
            throw new ServiceOrderEmployeeInactiveException(employeeId);
        }

        var order = ServiceOrder.Create(customerId, vehicleId);
        var history = ServiceOrderStatusHistory.ForStatusChange(order.Id, null, ServiceOrderStatus.RECEIVED, employeeId);

        await _serviceOrderWriteOnlyRepository.Add(order, cancellationToken);
        await _serviceOrderStatusHistoryWriteOnlyRepository.Add(history, cancellationToken);
        await _workUnit.Commit();

        return _mapper.Map<ServiceOrderResponse>(order);
    }

    private static void Validate(CreateServiceOrderCommand command)
    {
        var validator = new CreateServiceOrderValidator();

        var result = validator.Validate(command);

        if (!result.IsValid)
        {
            throw new ErrorOnValidationException(result.Errors.Select(e => e.ErrorMessage).ToList());
        }
    }
}
