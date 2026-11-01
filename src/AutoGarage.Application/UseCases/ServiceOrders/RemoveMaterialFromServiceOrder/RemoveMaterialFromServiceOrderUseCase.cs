using AutoGarage.Application.Commands.ServiceOrders;
using AutoGarage.Application.Services.ServiceOrders;
using AutoGarage.Application.UseCases.ServiceOrders;
using AutoGarage.Communication.Responses.ServiceOrders;
using AutoGarage.Domain.Common.Exceptions;
using AutoGarage.Domain.Entities;
using AutoGarage.Domain.Enums.ServiceOrders;
using AutoGarage.Domain.Exceptions.ServiceOrders.Exceptions;
using AutoGarage.Domain.Repositories;
using AutoGarage.Domain.Repositories.ServiceOrders.ServiceOrderMaterials;
using AutoGarage.Domain.Repositories.ServiceOrders.ServiceOrderStatusHistories;

namespace AutoGarage.Application.UseCases.ServiceOrders.RemoveMaterialFromServiceOrder;

public class RemoveMaterialFromServiceOrderUseCase : IRemoveMaterialFromServiceOrderUseCase
{
    private readonly ServiceOrderLookup _lookup;
    private readonly IServiceOrderMaterialReadOnlyRepository _serviceOrderMaterialReadOnlyRepository;
    private readonly IServiceOrderMaterialWriteOnlyRepository _serviceOrderMaterialWriteOnlyRepository;
    private readonly IServiceOrderStatusHistoryWriteOnlyRepository _serviceOrderStatusHistoryWriteOnlyRepository;
    private readonly IUnitWork _workUnit;

    public RemoveMaterialFromServiceOrderUseCase(
        ServiceOrderLookup lookup,
        IServiceOrderMaterialReadOnlyRepository serviceOrderMaterialReadOnlyRepository,
        IServiceOrderMaterialWriteOnlyRepository serviceOrderMaterialWriteOnlyRepository,
        IServiceOrderStatusHistoryWriteOnlyRepository serviceOrderStatusHistoryWriteOnlyRepository,
        IUnitWork workUnit)
    {
        _lookup = lookup;
        _serviceOrderMaterialReadOnlyRepository = serviceOrderMaterialReadOnlyRepository;
        _serviceOrderMaterialWriteOnlyRepository = serviceOrderMaterialWriteOnlyRepository;
        _serviceOrderStatusHistoryWriteOnlyRepository = serviceOrderStatusHistoryWriteOnlyRepository;
        _workUnit = workUnit;
    }

    public async Task<ServiceOrderMaterialItemResponse> Handle(RemoveMaterialFromServiceOrderCommand command, CancellationToken cancellationToken)
    {
        Validate(command);

        var order = await _lookup.GetOrder(command.ServiceOrderId, cancellationToken);
        var employeeId = await _lookup.EnsureActiveEmployee(command.EmployeeId, cancellationToken);

        order.EnsureItemsEditable();

        if (!Guid.TryParse(command.ItemId, out var itemId))
        {
            throw new InvalidServiceOrderItemIdException();
        }

        var item = await _serviceOrderMaterialReadOnlyRepository.GetById(itemId, cancellationToken);

        if (item is null || item.ServiceOrderId != order.Id)
        {
            throw new ServiceOrderItemNotFoundException(itemId);
        }

        item.Deactivate();

        await _serviceOrderMaterialWriteOnlyRepository.Update(item, cancellationToken);

        var history = ServiceOrderStatusHistory.ForItemChange(order.Id, ServiceOrderEventType.ITEM_MATERIAL, item.Id, ServiceOrderItemDirection.OUT, employeeId);

        await _serviceOrderStatusHistoryWriteOnlyRepository.Add(history, cancellationToken);
        await _workUnit.Commit();

        return new ServiceOrderMaterialItemResponse(item.Id, item.MaterialId, item.Quantity, item.UnitPrice);
    }

    private static void Validate(RemoveMaterialFromServiceOrderCommand command)
    {
        var result = new RemoveMaterialFromServiceOrderValidator().Validate(command);

        if (!result.IsValid)
        {
            throw new ErrorOnValidationException(result.Errors.Select(e => e.ErrorMessage).ToList());
        }
    }
}
