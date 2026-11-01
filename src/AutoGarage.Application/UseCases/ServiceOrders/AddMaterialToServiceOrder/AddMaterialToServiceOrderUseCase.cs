using AutoGarage.Application.Commands.ServiceOrders;
using AutoGarage.Application.Services.ServiceOrders;
using AutoGarage.Application.UseCases.ServiceOrders;
using AutoGarage.Communication.Responses.ServiceOrders;
using AutoGarage.Domain.Common.Exceptions;
using AutoGarage.Domain.Entities;
using AutoGarage.Domain.Enums.ServiceOrders;
using AutoGarage.Domain.Exceptions.Materials.Exceptions;
using AutoGarage.Domain.Exceptions.ServiceOrders.Exceptions;
using AutoGarage.Domain.Repositories;
using AutoGarage.Domain.Repositories.Materials;
using AutoGarage.Domain.Repositories.ServiceOrders.ServiceOrderMaterials;
using AutoGarage.Domain.Repositories.ServiceOrders.ServiceOrderStatusHistories;
using AutoGarage.Domain.Shared;

namespace AutoGarage.Application.UseCases.ServiceOrders.AddMaterialToServiceOrder;

public class AddMaterialToServiceOrderUseCase : IAddMaterialToServiceOrderUseCase
{
    private readonly ServiceOrderLookup _lookup;
    private readonly IMaterialReadOnlyRepository _materialReadOnlyRepository;
    private readonly IServiceOrderMaterialReadOnlyRepository _serviceOrderMaterialReadOnlyRepository;
    private readonly IServiceOrderMaterialWriteOnlyRepository _serviceOrderMaterialWriteOnlyRepository;
    private readonly IServiceOrderStatusHistoryWriteOnlyRepository _serviceOrderStatusHistoryWriteOnlyRepository;
    private readonly IUnitWork _workUnit;

    public AddMaterialToServiceOrderUseCase(
        ServiceOrderLookup lookup,
        IMaterialReadOnlyRepository materialReadOnlyRepository,
        IServiceOrderMaterialReadOnlyRepository serviceOrderMaterialReadOnlyRepository,
        IServiceOrderMaterialWriteOnlyRepository serviceOrderMaterialWriteOnlyRepository,
        IServiceOrderStatusHistoryWriteOnlyRepository serviceOrderStatusHistoryWriteOnlyRepository,
        IUnitWork workUnit)
    {
        _lookup = lookup;
        _materialReadOnlyRepository = materialReadOnlyRepository;
        _serviceOrderMaterialReadOnlyRepository = serviceOrderMaterialReadOnlyRepository;
        _serviceOrderMaterialWriteOnlyRepository = serviceOrderMaterialWriteOnlyRepository;
        _serviceOrderStatusHistoryWriteOnlyRepository = serviceOrderStatusHistoryWriteOnlyRepository;
        _workUnit = workUnit;
    }

    public async Task<ServiceOrderMaterialItemResponse> Handle(AddMaterialToServiceOrderCommand command, CancellationToken cancellationToken)
    {
        Validate(command);

        var order = await _lookup.GetOrder(command.ServiceOrderId, cancellationToken);
        var employeeId = await _lookup.EnsureActiveEmployee(command.EmployeeId, cancellationToken);

        order.EnsureItemsEditable();

        if (!Guid.TryParse(command.MaterialId, out var materialId))
        {
            throw new InvalidMaterialIdException();
        }

        var material = await _materialReadOnlyRepository.GetById(materialId, cancellationToken)
            ?? throw new MaterialNotFoundException(materialId);

        if (material.Status != EntityStatus.ACTIVE)
        {
            throw new ServiceOrderMaterialInactiveException(materialId);
        }

        var item = await _serviceOrderMaterialReadOnlyRepository.GetByServiceOrderAndMaterialId(order.Id, materialId, cancellationToken);

        if (item is null)
        {
            item = ServiceOrderMaterial.Create(order.Id, material.Id, command.Quantity, material.Price);
            await _serviceOrderMaterialWriteOnlyRepository.Add(item, cancellationToken);
        }
        else if (item.Status == EntityStatus.ACTIVE)
        {
            item.AddQuantity(command.Quantity);
            await _serviceOrderMaterialWriteOnlyRepository.Update(item, cancellationToken);
        }
        else
        {
            item.Reactivate(command.Quantity, material.Price);
            await _serviceOrderMaterialWriteOnlyRepository.Update(item, cancellationToken);
        }

        var history = ServiceOrderStatusHistory.ForItemChange(order.Id, ServiceOrderEventType.ITEM_MATERIAL, item.Id, ServiceOrderItemDirection.IN, employeeId);

        await _serviceOrderStatusHistoryWriteOnlyRepository.Add(history, cancellationToken);
        await _workUnit.Commit();

        return new ServiceOrderMaterialItemResponse(item.Id, item.MaterialId, item.Quantity, item.UnitPrice);
    }

    private static void Validate(AddMaterialToServiceOrderCommand command)
    {
        var result = new AddMaterialToServiceOrderValidator().Validate(command);

        if (!result.IsValid)
        {
            throw new ErrorOnValidationException(result.Errors.Select(e => e.ErrorMessage).ToList());
        }
    }
}
