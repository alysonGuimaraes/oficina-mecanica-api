using AutoGarage.Application.UseCases.ServiceOrders.Shared;
using AutoGarage.Application.Commands.ServiceOrders;
using AutoGarage.Application.Services.ServiceOrders;
using AutoGarage.Application.UseCases.ServiceOrders;
using AutoGarage.Application.Services.Notifications;
using AutoGarage.Communication.Responses.ServiceOrders;
using AutoGarage.Domain.Common.Exceptions;
using AutoGarage.Domain.Enums.ServiceOrders;
using AutoGarage.Domain.Exceptions.ServiceOrders.Exceptions;
using AutoGarage.Domain.Repositories;
using AutoGarage.Domain.Repositories.ServiceOrders.ServiceOrders;
using AutoGarage.Domain.Repositories.ServiceOrders.ServiceOrderStatusHistories;
using MapsterMapper;

namespace AutoGarage.Application.UseCases.ServiceOrders.StartServiceOrderExecution;

public class StartServiceOrderExecutionUseCase : IStartServiceOrderExecutionUseCase
{
    private readonly ServiceOrderLookup _lookup;
    private readonly ServiceOrderStockDebit _stockDebit;
    private readonly IServiceOrderWriteOnlyRepository _serviceOrderWriteOnlyRepository;
    private readonly IServiceOrderStatusHistoryWriteOnlyRepository _serviceOrderStatusHistoryWriteOnlyRepository;
    private readonly IUnitWork _workUnit;
    private readonly IServiceOrderStatusNotifier _notifier;
    private readonly IMapper _mapper;

    public StartServiceOrderExecutionUseCase(
        ServiceOrderLookup lookup,
        ServiceOrderStockDebit stockDebit,
        IServiceOrderWriteOnlyRepository serviceOrderWriteOnlyRepository,
        IServiceOrderStatusHistoryWriteOnlyRepository serviceOrderStatusHistoryWriteOnlyRepository,
        IUnitWork workUnit,
        IServiceOrderStatusNotifier notifier,
        IMapper mapper)
    {
        _lookup = lookup;
        _stockDebit = stockDebit;
        _serviceOrderWriteOnlyRepository = serviceOrderWriteOnlyRepository;
        _serviceOrderStatusHistoryWriteOnlyRepository = serviceOrderStatusHistoryWriteOnlyRepository;
        _workUnit = workUnit;
        _notifier = notifier;
        _mapper = mapper;
    }

    public async Task<ServiceOrderResponse> Handle(ServiceOrderEmployeeCommand command, CancellationToken cancellationToken)
    {
        Validate(command);

        var order = await _lookup.GetOrder(command.ServiceOrderId, cancellationToken);
        var employeeId = await _lookup.EnsureActiveEmployee(command.EmployeeId, cancellationToken);

        // A matriz também permite AWAITING_APPROVAL → IN_EXECUTION (usada pela aprovação).
        // Aqui só vale AWAITING_MATERIALS.
        if (order.Status != ServiceOrderStatus.AWAITING_MATERIALS)
        {
            throw new InvalidServiceOrderTransitionException(order.Status, ServiceOrderStatus.IN_EXECUTION);
        }

        await _stockDebit.DebitActiveMaterialsAsync(order, cancellationToken);

        var history = order.ChangeStatus(ServiceOrderStatus.IN_EXECUTION, employeeId);

        await _serviceOrderWriteOnlyRepository.Update(order, cancellationToken);
        await _serviceOrderStatusHistoryWriteOnlyRepository.Add(history, cancellationToken);
        await _workUnit.Commit();
        await _notifier.NotifyAsync(order, history, cancellationToken);

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
