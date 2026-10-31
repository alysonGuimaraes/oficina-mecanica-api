using AutoGarage.Communication.Responses.ServiceOrders;
using AutoGarage.Domain.Entities;
using AutoGarage.Domain.Exceptions.ServiceOrders.Exceptions;
using AutoGarage.Domain.Repositories.ServiceOrders.ServiceOrderMaterials;
using AutoGarage.Domain.Repositories.ServiceOrders.ServiceOrders;
using AutoGarage.Domain.Repositories.ServiceOrders.ServiceOrderServices;
using AutoGarage.Domain.Repositories.ServiceOrders.ServiceOrderStatusHistories;
using AutoGarage.Domain.Shared;

namespace AutoGarage.Application.UseCases.ServiceOrders.GetServiceOrderById;

public class GetServiceOrderByIdUseCase : IGetServiceOrderByIdUseCase
{
    private readonly IServiceOrderReadOnlyRepository _serviceOrderReadOnlyRepository;
    private readonly IServiceOrderServiceReadOnlyRepository _serviceOrderServiceReadOnlyRepository;
    private readonly IServiceOrderMaterialReadOnlyRepository _serviceOrderMaterialReadOnlyRepository;
    private readonly IServiceOrderStatusHistoryReadOnlyRepository _serviceOrderStatusHistoryReadOnlyRepository;

    public GetServiceOrderByIdUseCase(
        IServiceOrderReadOnlyRepository serviceOrderReadOnlyRepository,
        IServiceOrderServiceReadOnlyRepository serviceOrderServiceReadOnlyRepository,
        IServiceOrderMaterialReadOnlyRepository serviceOrderMaterialReadOnlyRepository,
        IServiceOrderStatusHistoryReadOnlyRepository serviceOrderStatusHistoryReadOnlyRepository)
    {
        _serviceOrderReadOnlyRepository = serviceOrderReadOnlyRepository;
        _serviceOrderServiceReadOnlyRepository = serviceOrderServiceReadOnlyRepository;
        _serviceOrderMaterialReadOnlyRepository = serviceOrderMaterialReadOnlyRepository;
        _serviceOrderStatusHistoryReadOnlyRepository = serviceOrderStatusHistoryReadOnlyRepository;
    }

    public async Task<ServiceOrderDetailResponse> Handle(string id, CancellationToken cancellationToken)
    {
        if (!Guid.TryParse(id, out var serviceOrderId))
        {
            throw new InvalidServiceOrderIdException();
        }

        var order = await _serviceOrderReadOnlyRepository.GetById(serviceOrderId, cancellationToken)
            ?? throw new ServiceOrderNotFoundException(serviceOrderId);

        var services = await _serviceOrderServiceReadOnlyRepository.GetAllByServiceOrder(serviceOrderId, cancellationToken);
        var materials = await _serviceOrderMaterialReadOnlyRepository.GetAllByServiceOrder(serviceOrderId, cancellationToken);
        var history = await _serviceOrderStatusHistoryReadOnlyRepository.GetAllByServiceOrder(serviceOrderId, cancellationToken);

        return new ServiceOrderDetailResponse(
            order.Id,
            order.CustomerId,
            order.VehicleId,
            order.Status,
            order.BudgetTotal,
            order.BudgetSentAt,
            order.Observation,
            order.CancellationReason,
            order.CreatedAt,
            order.UpdatedAt,
            services
                .Where(s => s.Status == EntityStatus.ACTIVE)
                .Select(s => new ServiceOrderServiceItemResponse(s.Id, s.ServiceId, s.Price))
                .ToList(),
            materials
                .Where(m => m.Status == EntityStatus.ACTIVE)
                .Select(m => new ServiceOrderMaterialItemResponse(m.Id, m.MaterialId, m.Quantity, m.UnitPrice))
                .ToList(),
            history
                .OrderBy(h => h.CreatedAt)
                .ThenBy(h => h.Id)
                .Select(h => new ServiceOrderStatusHistoryResponse(h.Id, h.EventType, h.FromStatus, h.ToStatus, h.ItemId, h.ItemDirection, h.EmployeeId, h.CreatedAt))
                .ToList());
    }
}
