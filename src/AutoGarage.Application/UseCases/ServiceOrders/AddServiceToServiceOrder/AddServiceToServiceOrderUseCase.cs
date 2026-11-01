using AutoGarage.Application.Commands.ServiceOrders;
using AutoGarage.Application.Services.ServiceOrders;
using AutoGarage.Application.UseCases.ServiceOrders;
using AutoGarage.Communication.Responses.ServiceOrders;
using AutoGarage.Domain.Common.Exceptions;
using AutoGarage.Domain.Entities;
using AutoGarage.Domain.Enums.ServiceOrders;
using AutoGarage.Domain.Exceptions.Services.Exceptions;
using AutoGarage.Domain.Exceptions.ServiceOrders.Exceptions;
using AutoGarage.Domain.Repositories;
using AutoGarage.Domain.Repositories.Services;
using AutoGarage.Domain.Repositories.ServiceOrders.ServiceOrderServices;
using AutoGarage.Domain.Repositories.ServiceOrders.ServiceOrderStatusHistories;
using AutoGarage.Domain.Shared;

namespace AutoGarage.Application.UseCases.ServiceOrders.AddServiceToServiceOrder;

public class AddServiceToServiceOrderUseCase : IAddServiceToServiceOrderUseCase
{
    private readonly ServiceOrderLookup _lookup;
    private readonly IServiceReadOnlyRepository _serviceReadOnlyRepository;
    private readonly IServiceOrderServiceReadOnlyRepository _serviceOrderServiceReadOnlyRepository;
    private readonly IServiceOrderServiceWriteOnlyRepository _serviceOrderServiceWriteOnlyRepository;
    private readonly IServiceOrderStatusHistoryWriteOnlyRepository _serviceOrderStatusHistoryWriteOnlyRepository;
    private readonly IUnitWork _workUnit;

    public AddServiceToServiceOrderUseCase(
        ServiceOrderLookup lookup,
        IServiceReadOnlyRepository serviceReadOnlyRepository,
        IServiceOrderServiceReadOnlyRepository serviceOrderServiceReadOnlyRepository,
        IServiceOrderServiceWriteOnlyRepository serviceOrderServiceWriteOnlyRepository,
        IServiceOrderStatusHistoryWriteOnlyRepository serviceOrderStatusHistoryWriteOnlyRepository,
        IUnitWork workUnit)
    {
        _lookup = lookup;
        _serviceReadOnlyRepository = serviceReadOnlyRepository;
        _serviceOrderServiceReadOnlyRepository = serviceOrderServiceReadOnlyRepository;
        _serviceOrderServiceWriteOnlyRepository = serviceOrderServiceWriteOnlyRepository;
        _serviceOrderStatusHistoryWriteOnlyRepository = serviceOrderStatusHistoryWriteOnlyRepository;
        _workUnit = workUnit;
    }

    public async Task<ServiceOrderServiceItemResponse> Handle(AddServiceToServiceOrderCommand command, CancellationToken cancellationToken)
    {
        Validate(command);

        var order = await _lookup.GetOrder(command.ServiceOrderId, cancellationToken);
        var employeeId = await _lookup.EnsureActiveEmployee(command.EmployeeId, cancellationToken);

        order.EnsureItemsEditable();

        if (!Guid.TryParse(command.ServiceId, out var serviceId))
        {
            throw new InvalidServiceIdException();
        }

        var service = await _serviceReadOnlyRepository.GetById(serviceId, cancellationToken)
            ?? throw new ServiceNotFoundException(serviceId);

        if (service.Status != EntityStatus.ACTIVE)
        {
            throw new ServiceOrderServiceInactiveException(serviceId);
        }

        var item = await _serviceOrderServiceReadOnlyRepository.GetByServiceOrderAndServiceId(order.Id, serviceId, cancellationToken);

        if (item is null)
        {
            item = ServiceOrderService.Create(order.Id, service.Id, service.Price);
            await _serviceOrderServiceWriteOnlyRepository.Add(item, cancellationToken);
        }
        else if (item.Status == EntityStatus.ACTIVE)
        {
            throw new ServiceAlreadyAddedToServiceOrderException(serviceId);
        }
        else
        {
            item.Reactivate(service.Price);
            await _serviceOrderServiceWriteOnlyRepository.Update(item, cancellationToken);
        }

        var history = ServiceOrderStatusHistory.ForItemChange(order.Id, ServiceOrderEventType.ITEM_SERVICE, item.Id, ServiceOrderItemDirection.IN, employeeId);

        await _serviceOrderStatusHistoryWriteOnlyRepository.Add(history, cancellationToken);
        await _workUnit.Commit();

        return new ServiceOrderServiceItemResponse(item.Id, item.ServiceId, item.Price);
    }

    private static void Validate(AddServiceToServiceOrderCommand command)
    {
        var result = new AddServiceToServiceOrderValidator().Validate(command);

        if (!result.IsValid)
        {
            throw new ErrorOnValidationException(result.Errors.Select(e => e.ErrorMessage).ToList());
        }
    }
}
