using AutoGarage.Application.Commands.ServiceOrders;
using AutoGarage.Application.Services.ServiceOrders;
using AutoGarage.Application.Services.Notifications;
using AutoGarage.Communication.Responses.ServiceOrders;
using AutoGarage.Domain.Common.Exceptions;
using AutoGarage.Domain.Enums.ServiceOrders;
using AutoGarage.Domain.Exceptions.ServiceOrders.Exceptions;
using AutoGarage.Domain.Repositories;
using AutoGarage.Domain.Repositories.ServiceOrders.ServiceOrders;
using AutoGarage.Domain.Repositories.ServiceOrders.ServiceOrderStatusHistories;

namespace AutoGarage.Application.UseCases.ServiceOrders.PublicServiceOrderApproval;

public class PublicServiceOrderApprovalUseCase : IPublicServiceOrderApprovalUseCase
{
    private const string ApprovalExpiredReason = "Approval deadline expired.";
    private const string ApprovedMessage = "Budget approved.";
    private const string RefusedMessage = "Budget refused.";

    private readonly ServiceOrderLookup _lookup;
    private readonly ServiceOrderStockDebit _stockDebit;
    private readonly IServiceOrderWriteOnlyRepository _serviceOrderWriteOnlyRepository;
    private readonly IServiceOrderStatusHistoryWriteOnlyRepository _serviceOrderStatusHistoryWriteOnlyRepository;
    private readonly IUnitWork _workUnit;
    private readonly IServiceOrderStatusNotifier _notifier;
    private readonly ServiceOrderApprovalSettings _settings;
    private readonly TimeProvider _timeProvider;

    public PublicServiceOrderApprovalUseCase(
        ServiceOrderLookup lookup,
        ServiceOrderStockDebit stockDebit,
        IServiceOrderWriteOnlyRepository serviceOrderWriteOnlyRepository,
        IServiceOrderStatusHistoryWriteOnlyRepository serviceOrderStatusHistoryWriteOnlyRepository,
        IUnitWork workUnit,
        IServiceOrderStatusNotifier notifier,
        ServiceOrderApprovalSettings settings,
        TimeProvider timeProvider)
    {
        _lookup = lookup;
        _stockDebit = stockDebit;
        _serviceOrderWriteOnlyRepository = serviceOrderWriteOnlyRepository;
        _serviceOrderStatusHistoryWriteOnlyRepository = serviceOrderStatusHistoryWriteOnlyRepository;
        _workUnit = workUnit;
        _notifier = notifier;
        _settings = settings;
        _timeProvider = timeProvider;
    }

    public async Task<ServiceOrderApprovalResponse> Handle(PublicServiceOrderApprovalCommand command, CancellationToken cancellationToken)
    {
        Validate(command);

        var order = await _lookup.GetOrder(command.ServiceOrderId, cancellationToken);

        if (order.Status != ServiceOrderStatus.AWAITING_APPROVAL)
        {
            throw new ServiceOrderNotAwaitingApprovalException();
        }

        var nowUtc = _timeProvider.GetUtcNow().UtcDateTime;

        // Prazo vale para aprovação e recusa (R-B). O cancelamento é gravado antes da exceção de 400.
        if (order.IsBudgetApprovalExpired(nowUtc, _settings.DeadlineDays))
        {
            var cancelHistory = order.Cancel(ApprovalExpiredReason, null, isCustomerAction: true);

            await _serviceOrderWriteOnlyRepository.Update(order, cancellationToken);
            await _serviceOrderStatusHistoryWriteOnlyRepository.Add(cancelHistory, cancellationToken);
            await _workUnit.Commit();
            await _notifier.NotifyAsync(order, cancelHistory, cancellationToken);

            throw new ServiceOrderApprovalExpiredException();
        }

        if (command.Approved is not true)
        {
            var refuseHistory = order.Refuse(null, isCustomerAction: true);

            await _serviceOrderWriteOnlyRepository.Update(order, cancellationToken);
            await _serviceOrderStatusHistoryWriteOnlyRepository.Add(refuseHistory, cancellationToken);
            await _workUnit.Commit();
            await _notifier.NotifyAsync(order, refuseHistory, cancellationToken);

            return new ServiceOrderApprovalResponse(RefusedMessage);
        }

        // Com estoque suficiente: baixa atômica e IN_EXECUTION. Sem estoque: AWAITING_MATERIALS, sem baixa.
        var hasStock = await _stockDebit.HasSufficientStockAsync(order, cancellationToken);

        if (hasStock)
        {
            await _stockDebit.DebitActiveMaterialsAsync(order, cancellationToken);
        }

        var target = hasStock ? ServiceOrderStatus.IN_EXECUTION : ServiceOrderStatus.AWAITING_MATERIALS;
        var history = order.ChangeStatus(target, null, isCustomerAction: true);

        await _serviceOrderWriteOnlyRepository.Update(order, cancellationToken);
        await _serviceOrderStatusHistoryWriteOnlyRepository.Add(history, cancellationToken);
        await _workUnit.Commit();
        await _notifier.NotifyAsync(order, history, cancellationToken);

        return new ServiceOrderApprovalResponse(ApprovedMessage);
    }

    private static void Validate(PublicServiceOrderApprovalCommand command)
    {
        var result = new PublicServiceOrderApprovalValidator().Validate(command);

        if (!result.IsValid)
        {
            throw new ErrorOnValidationException(result.Errors.Select(e => e.ErrorMessage).ToList());
        }
    }
}
