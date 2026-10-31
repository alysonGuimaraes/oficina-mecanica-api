using AutoGarage.Application.Commands.ServiceOrders;
using AutoGarage.Application.Services.ServiceOrders;
using AutoGarage.Application.UseCases.ServiceOrders;
using AutoGarage.Communication.Responses.ServiceOrders;
using AutoGarage.Domain.Common.Exceptions;
using AutoGarage.Domain.Enums.ServiceOrders;
using AutoGarage.Domain.Exceptions.ServiceOrders.Exceptions;
using AutoGarage.Domain.Repositories;
using AutoGarage.Domain.Repositories.Services;
using AutoGarage.Domain.Repositories.ServiceOrders.ServiceOrderMaterials;
using AutoGarage.Domain.Repositories.ServiceOrders.ServiceOrders;
using AutoGarage.Domain.Repositories.ServiceOrders.ServiceOrderServices;
using AutoGarage.Domain.Repositories.ServiceOrders.ServiceOrderStatusHistories;
using AutoGarage.Domain.Shared;
using AutoGarage.Domain.Utilities;
using MapsterMapper;

namespace AutoGarage.Application.UseCases.ServiceOrders.SendServiceOrderBudget;

public class SendServiceOrderBudgetUseCase : ISendServiceOrderBudgetUseCase
{
    private readonly ServiceOrderLookup _lookup;
    private readonly IServiceReadOnlyRepository _serviceReadOnlyRepository;
    private readonly IServiceOrderServiceReadOnlyRepository _serviceOrderServiceReadOnlyRepository;
    private readonly IServiceOrderMaterialReadOnlyRepository _serviceOrderMaterialReadOnlyRepository;
    private readonly IServiceOrderWriteOnlyRepository _serviceOrderWriteOnlyRepository;
    private readonly IServiceOrderStatusHistoryWriteOnlyRepository _serviceOrderStatusHistoryWriteOnlyRepository;
    private readonly IUnitWork _workUnit;
    private readonly IMapper _mapper;

    public SendServiceOrderBudgetUseCase(
        ServiceOrderLookup lookup,
        IServiceReadOnlyRepository serviceReadOnlyRepository,
        IServiceOrderServiceReadOnlyRepository serviceOrderServiceReadOnlyRepository,
        IServiceOrderMaterialReadOnlyRepository serviceOrderMaterialReadOnlyRepository,
        IServiceOrderWriteOnlyRepository serviceOrderWriteOnlyRepository,
        IServiceOrderStatusHistoryWriteOnlyRepository serviceOrderStatusHistoryWriteOnlyRepository,
        IUnitWork workUnit,
        IMapper mapper)
    {
        _lookup = lookup;
        _serviceReadOnlyRepository = serviceReadOnlyRepository;
        _serviceOrderServiceReadOnlyRepository = serviceOrderServiceReadOnlyRepository;
        _serviceOrderMaterialReadOnlyRepository = serviceOrderMaterialReadOnlyRepository;
        _serviceOrderWriteOnlyRepository = serviceOrderWriteOnlyRepository;
        _serviceOrderStatusHistoryWriteOnlyRepository = serviceOrderStatusHistoryWriteOnlyRepository;
        _workUnit = workUnit;
        _mapper = mapper;
    }

    public async Task<ServiceOrderResponse> Handle(ServiceOrderEmployeeCommand command, CancellationToken cancellationToken)
    {
        Validate(command);

        var order = await _lookup.GetOrder(command.ServiceOrderId, cancellationToken);
        var employeeId = await _lookup.EnsureActiveEmployee(command.EmployeeId, cancellationToken);

        if (order.Status != ServiceOrderStatus.IN_DIAGNOSIS)
        {
            throw new InvalidServiceOrderTransitionException(order.Status, ServiceOrderStatus.AWAITING_APPROVAL);
        }

        var services = await _serviceOrderServiceReadOnlyRepository.GetAllByServiceOrder(order.Id, cancellationToken);
        var materials = await _serviceOrderMaterialReadOnlyRepository.GetAllByServiceOrder(order.Id, cancellationToken);
        var inactiveCatalogServiceIds = (await _serviceReadOnlyRepository.GetByStatus(EntityStatus.INACTIVE, cancellationToken))
            .Select(s => s.Id)
            .ToHashSet();

        var total = ServiceOrderBudgetCalculator.Calculate(services, materials, inactiveCatalogServiceIds);

        var history = order.SendBudget(total, employeeId);

        await _serviceOrderWriteOnlyRepository.Update(order, cancellationToken);
        await _serviceOrderStatusHistoryWriteOnlyRepository.Add(history, cancellationToken);
        await _workUnit.Commit();

        return _mapper.Map<ServiceOrderResponse>(order);
    }

    private static void Validate(ServiceOrderEmployeeCommand command)
    {
        var result = new ServiceOrderEmployeeValidator().Validate(command);

        if (!result.IsValid)
        {
            throw new ErrorOnValidationException(result.Errors.Select(e => e.ErrorMessage).ToList());
        }
    }
}
