using AutoGarage.Application.Commands.ServiceOrders;
using AutoGarage.Application.Services.ServiceOrders;
using AutoGarage.Application.UseCases.ServiceOrders;
using AutoGarage.Application.Services.Notifications;
using AutoGarage.Communication.Responses.ServiceOrders;
using AutoGarage.Domain.Common.Exceptions;
using AutoGarage.Domain.Enums.ServiceOrders;
using AutoGarage.Domain.Repositories;
using AutoGarage.Domain.Repositories.ServiceOrders.ServiceOrders;
using AutoGarage.Domain.Repositories.ServiceOrders.ServiceOrderStatusHistories;
using MapsterMapper;

namespace AutoGarage.Application.UseCases.ServiceOrders.CancelServiceOrder;

public class CancelServiceOrderUseCase : ICancelServiceOrderUseCase
{
    private readonly ServiceOrderLookup _lookup;
    private readonly ServiceOrderStockReturn _stockReturn;
    private readonly IServiceOrderWriteOnlyRepository _serviceOrderWriteOnlyRepository;
    private readonly IServiceOrderStatusHistoryWriteOnlyRepository _serviceOrderStatusHistoryWriteOnlyRepository;
    private readonly IUnitWork _workUnit;
    private readonly IServiceOrderStatusNotifier _notifier;
    private readonly IMapper _mapper;

    public CancelServiceOrderUseCase(
        ServiceOrderLookup lookup,
        ServiceOrderStockReturn stockReturn,
        IServiceOrderWriteOnlyRepository serviceOrderWriteOnlyRepository,
        IServiceOrderStatusHistoryWriteOnlyRepository serviceOrderStatusHistoryWriteOnlyRepository,
        IUnitWork workUnit,
        IServiceOrderStatusNotifier notifier,
        IMapper mapper)
    {
        _lookup = lookup;
        _stockReturn = stockReturn;
        _serviceOrderWriteOnlyRepository = serviceOrderWriteOnlyRepository;
        _serviceOrderStatusHistoryWriteOnlyRepository = serviceOrderStatusHistoryWriteOnlyRepository;
        _workUnit = workUnit;
        _notifier = notifier;
        _mapper = mapper;
    }

    public async Task<ServiceOrderResponse> Handle(CancelServiceOrderCommand command, CancellationToken cancellationToken)
    {
        Validate(command);

        var order = await _lookup.GetOrder(command.ServiceOrderId, cancellationToken);
        var employeeId = await _lookup.EnsureActiveEmployee(command.EmployeeId, cancellationToken);

        var previousStatus = order.Status;

        // O domínio recusa FINISHED, DELIVERED e CANCELLED antes de qualquer alteração (decisão 6).
        var history = order.Cancel(command.Reason.Trim(), employeeId);

        // Só houve baixa em IN_EXECUTION (plano 07); nos demais estados não há o que devolver.
        if (previousStatus == ServiceOrderStatus.IN_EXECUTION)
        {
            await _stockReturn.ReturnActiveMaterialsAsync(order, cancellationToken);
        }

        await _serviceOrderWriteOnlyRepository.Update(order, cancellationToken);
        await _serviceOrderStatusHistoryWriteOnlyRepository.Add(history, cancellationToken);
        await _workUnit.Commit();
        await _notifier.NotifyAsync(order, history, cancellationToken);

        return _mapper.Map<ServiceOrderResponse>(order);
    }

    private static void Validate(CancelServiceOrderCommand command)
    {
        var result = new CancelServiceOrderValidator().Validate(command);

        if (!result.IsValid)
        {
            throw new ErrorOnValidationException(result.Errors.Select(e => e.ErrorMessage).ToList());
        }
    }
}
