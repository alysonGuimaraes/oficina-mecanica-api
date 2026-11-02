using AutoGarage.Domain.Entities;
using AutoGarage.Domain.Repositories.Customers;
using Microsoft.Extensions.Logging;

namespace AutoGarage.Application.Services.Notifications;

public class ServiceOrderStatusNotifier : IServiceOrderStatusNotifier
{
    private readonly ICustomerReadOnlyRepository _customerReadOnlyRepository;
    private readonly IEmailSender _emailSender;
    private readonly ILogger<ServiceOrderStatusNotifier> _logger;

    public ServiceOrderStatusNotifier(
        ICustomerReadOnlyRepository customerReadOnlyRepository,
        IEmailSender emailSender,
        ILogger<ServiceOrderStatusNotifier> logger)
    {
        _customerReadOnlyRepository = customerReadOnlyRepository;
        _emailSender = emailSender;
        _logger = logger;
    }

    public async Task NotifyAsync(ServiceOrder order, ServiceOrderStatusHistory history, CancellationToken cancellationToken)
    {
        try
        {
            var customer = await _customerReadOnlyRepository.GetById(order.CustomerId, cancellationToken);

            if (customer is null)
            {
                _logger.LogWarning("No status e-mail sent for service order {OrderId}: customer not found.", order.Id);
                return;
            }

            var message = history.FromStatus is null
                ? BuildCreationMessage(order, customer.Email, customer.Name)
                : BuildTransitionMessage(order, history, customer.Email, customer.Name);

            await _emailSender.SendAsync(message, cancellationToken);
        }
        catch (Exception exception)
        {
            // Logged only: the service order change is already committed and must not be affected.
            _logger.LogError(exception, "Failed to send status e-mail for service order {OrderId}.", order.Id);
        }
    }

    private static EmailMessage BuildCreationMessage(ServiceOrder order, string toEmail, string toName) =>
        new(
            toEmail,
            toName,
            $"Ordem de serviço {order.Id}: recebida",
            $"Olá, {toName}.\n\nA sua ordem de serviço {order.Id} foi recebida.\n");

    private static EmailMessage BuildTransitionMessage(ServiceOrder order, ServiceOrderStatusHistory history, string toEmail, string toName)
    {
        var from = ServiceOrderStatusLabels.For(history.FromStatus!.Value);
        var to = ServiceOrderStatusLabels.For(history.ToStatus!.Value);

        return new EmailMessage(
            toEmail,
            toName,
            $"Ordem de serviço {order.Id}: status atualizado",
            $"Olá, {toName}.\n\nA situação da sua ordem de serviço {order.Id} mudou de {from} para {to}.\n");
    }
}
