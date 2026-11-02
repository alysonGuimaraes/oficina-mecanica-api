using AutoGarage.Application.Commands.ServiceOrders;
using AutoGarage.Application.Services.ServiceOrders;
using AutoGarage.Application.UseCases.ServiceOrders;
using AutoGarage.Application.Services.Notifications;
using AutoGarage.Communication.Responses.ServiceOrders;
using AutoGarage.Domain.Common.Exceptions;
using AutoGarage.Domain.Repositories;
using AutoGarage.Domain.Repositories.ServiceOrders.ServiceOrders;
using AutoGarage.Domain.Repositories.ServiceOrders.ServiceOrderStatusHistories;
using MapsterMapper;

namespace AutoGarage.Application.UseCases.ServiceOrders.CompleteServiceOrder;

public class CompleteServiceOrderUseCase : ICompleteServiceOrderUseCase
{
    private readonly ServiceOrderLookup _lookup;
    private readonly IServiceOrderWriteOnlyRepository _serviceOrderWriteOnlyRepository;
    private readonly IServiceOrderStatusHistoryWriteOnlyRepository _serviceOrderStatusHistoryWriteOnlyRepository;
    private readonly IUnitWork _workUnit;
    private readonly IServiceOrderStatusNotifier _notifier;
    private readonly IMapper _mapper;

    public CompleteServiceOrderUseCase(
        ServiceOrderLookup lookup,
        IServiceOrderWriteOnlyRepository serviceOrderWriteOnlyRepository,
        IServiceOrderStatusHistoryWriteOnlyRepository serviceOrderStatusHistoryWriteOnlyRepository,
        IUnitWork workUnit,
        IServiceOrderStatusNotifier notifier,
        IMapper mapper)
    {
        _lookup = lookup;
        _serviceOrderWriteOnlyRepository = serviceOrderWriteOnlyRepository;
        _serviceOrderStatusHistoryWriteOnlyRepository = serviceOrderStatusHistoryWriteOnlyRepository;
        _workUnit = workUnit;
        _notifier = notifier;
        _mapper = mapper;
    }

    public async Task<ServiceOrderResponse> Handle(CompleteServiceOrderCommand command, CancellationToken cancellationToken)
    {
        Validate(command);

        var order = await _lookup.GetOrder(command.ServiceOrderId, cancellationToken);
        var employeeId = await _lookup.EnsureActiveEmployee(command.EmployeeId, cancellationToken);

        // O domínio recusa origem diferente de IN_DIAGNOSIS/IN_EXECUTION antes de alterar qualquer campo.
        var history = order.Complete(command.Observation.Trim(), employeeId);

        await _serviceOrderWriteOnlyRepository.Update(order, cancellationToken);
        await _serviceOrderStatusHistoryWriteOnlyRepository.Add(history, cancellationToken);
        await _workUnit.Commit();
        await _notifier.NotifyAsync(order, history, cancellationToken);

        return _mapper.Map<ServiceOrderResponse>(order);
    }

    private static void Validate(CompleteServiceOrderCommand command)
    {
        var result = new CompleteServiceOrderValidator().Validate(command);

        if (!result.IsValid)
        {
            throw new ErrorOnValidationException(result.Errors.Select(e => e.ErrorMessage).ToList());
        }
    }
}
