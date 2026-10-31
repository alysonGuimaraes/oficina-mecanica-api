using AutoGarage.Application.Commands.ServiceOrders;
using AutoGarage.Application.Services.ServiceOrders;
using AutoGarage.Application.UseCases.ServiceOrders;
using AutoGarage.Communication.Responses.ServiceOrders;
using AutoGarage.Domain.Common.Exceptions;
using AutoGarage.Domain.Entities;
using AutoGarage.Domain.Enums.ServiceOrders;
using AutoGarage.Domain.Exceptions.ServiceOrders.Exceptions;
using AutoGarage.Domain.Repositories;
using AutoGarage.Domain.Repositories.ServiceOrders.ServiceOrderServices;
using AutoGarage.Domain.Repositories.ServiceOrders.ServiceOrderStatusHistories;

namespace AutoGarage.Application.UseCases.ServiceOrders.RemoveServiceFromServiceOrder;

public class RemoveServiceFromServiceOrderUseCase : IRemoveServiceFromServiceOrderUseCase
{
    private readonly ServiceOrderLookup _lookup;
    private readonly IServiceOrderServiceReadOnlyRepository _serviceOrderServiceReadOnlyRepository;
    private readonly IServiceOrderServiceWriteOnlyRepository _serviceOrderServiceWriteOnlyRepository;
    private readonly IServiceOrderStatusHistoryWriteOnlyRepository _serviceOrderStatusHistoryWriteOnlyRepository;
    private readonly IUnitWork _workUnit;

    public RemoveServiceFromServiceOrderUseCase(
        ServiceOrderLookup lookup,
        IServiceOrderServiceReadOnlyRepository serviceOrderServiceReadOnlyRepository,
        IServiceOrderServiceWriteOnlyRepository serviceOrderServiceWriteOnlyRepository,
        IServiceOrderStatusHistoryWriteOnlyRepository serviceOrderStatusHistoryWriteOnlyRepository,
        IUnitWork workUnit)
    {
        _lookup = lookup;
        _serviceOrderServiceReadOnlyRepository = serviceOrderServiceReadOnlyRepository;
        _serviceOrderServiceWriteOnlyRepository = serviceOrderServiceWriteOnlyRepository;
        _serviceOrderStatusHistoryWriteOnlyRepository = serviceOrderStatusHistoryWriteOnlyRepository;
        _workUnit = workUnit;
    }

    public async Task<ServiceOrderServiceItemResponse> Handle(RemoveServiceFromServiceOrderCommand command, CancellationToken cancellationToken)
    {
        Validate(command);

        var order = await _lookup.GetOrder(command.ServiceOrderId, cancellationToken);
        var employeeId = await _lookup.EnsureActiveEmployee(command.EmployeeId, cancellationToken);

        order.EnsureItemsEditable();

        if (!Guid.TryParse(command.ItemId, out var itemId))
        {
            throw new InvalidServiceOrderItemIdException();
        }

        var item = await _serviceOrderServiceReadOnlyRepository.GetById(itemId, cancellationToken);

        if (item is null || item.ServiceOrderId != order.Id)
        {
            throw new ServiceOrderItemNotFoundException(itemId);
        }

        item.Deactivate();

        await _serviceOrderServiceWriteOnlyRepository.Update(item, cancellationToken);

        var history = ServiceOrderStatusHistory.ForItemChange(order.Id, ServiceOrderEventType.ITEM_SERVICE, item.Id, ServiceOrderItemDirection.OUT, employeeId);

        await _serviceOrderStatusHistoryWriteOnlyRepository.Add(history, cancellationToken);
        await _workUnit.Commit();

        return new ServiceOrderServiceItemResponse(item.Id, item.ServiceId, item.Price);
    }

    private static void Validate(RemoveServiceFromServiceOrderCommand command)
    {
        var result = new RemoveServiceFromServiceOrderValidator().Validate(command);

        if (!result.IsValid)
        {
            throw new ErrorOnValidationException(result.Errors.Select(e => e.ErrorMessage).ToList());
        }
    }
}
