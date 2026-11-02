using AutoGarage.Application.Services.Notifications;
using AutoGarage.CommonTestUtilities.Notifications;
using AutoGarage.CommonTestUtilities.Repositories.Customers;
using AutoGarage.Domain.Entities;
using AutoGarage.Domain.Enums.ServiceOrders;
using AutoGarage.Domain.ValueObjects;
using Microsoft.Extensions.Logging.Abstractions;
using Shouldly;

namespace AutoGarage.Application.UnitTests.Services.Notifications;

public class ServiceOrderStatusNotifierTests
{
    [Fact]
    public async Task NotifyAsync_ShouldSendToCustomerWithOrderIdAndStatusChange()
    {
        var customer = Customer.Create("Ana", new Document("529.982.247-25"), "ana@autogarage.local", "41", "999990001");
        var order = ServiceOrder.Create(customer.Id, Guid.NewGuid());
        var history = ServiceOrderStatusHistory.ForStatusChange(order.Id, ServiceOrderStatus.AWAITING_APPROVAL, ServiceOrderStatus.IN_EXECUTION, Guid.NewGuid());
        var sender = new FakeEmailSender();
        var sut = CreateSut(sender, customer);

        await sut.NotifyAsync(order, history, CancellationToken.None);

        sender.Sent.Count.ShouldBe(1);
        var message = sender.Sent[0];
        message.ToEmail.ShouldBe("ana@autogarage.local");
        message.ToName.ShouldBe("Ana");
        message.Subject.ShouldBe($"Ordem de serviço {order.Id}: status atualizado");
        message.PlainTextBody.ShouldContain(order.Id.ToString());
        message.PlainTextBody.ShouldContain("de Aguardando Aprovação para Em Execução");
    }

    [Fact]
    public async Task NotifyAsync_SenderThrows_ShouldLogAndNotThrow()
    {
        var customer = Customer.Create("Ana", new Document("529.982.247-25"), "ana@autogarage.local", "41", "999990001");
        var order = ServiceOrder.Create(customer.Id, Guid.NewGuid());
        var history = ServiceOrderStatusHistory.ForStatusChange(order.Id, ServiceOrderStatus.IN_DIAGNOSIS, ServiceOrderStatus.AWAITING_APPROVAL, Guid.NewGuid());
        var sender = new FakeEmailSender { ThrowOnSend = new InvalidOperationException("provider unavailable") };
        var sut = CreateSut(sender, customer);

        await Should.NotThrowAsync(() => sut.NotifyAsync(order, history, CancellationToken.None));

        sender.Sent.ShouldBeEmpty();
    }

    [Fact]
    public async Task NotifyAsync_CustomerNotFound_ShouldNotSend()
    {
        var order = ServiceOrder.Create(Guid.NewGuid(), Guid.NewGuid());
        var history = ServiceOrderStatusHistory.ForStatusChange(order.Id, ServiceOrderStatus.IN_DIAGNOSIS, ServiceOrderStatus.AWAITING_APPROVAL, Guid.NewGuid());
        var sender = new FakeEmailSender();
        var sut = CreateSut(sender);

        await sut.NotifyAsync(order, history, CancellationToken.None);

        sender.Sent.ShouldBeEmpty();
    }

    [Fact]
    public async Task NotifyAsync_Creation_ShouldSendReceivedMessage()
    {
        var customer = Customer.Create("Ana", new Document("529.982.247-25"), "ana@autogarage.local", "41", "999990001");
        var order = ServiceOrder.Create(customer.Id, Guid.NewGuid());
        var history = ServiceOrderStatusHistory.ForStatusChange(order.Id, null, ServiceOrderStatus.RECEIVED, Guid.NewGuid());
        var sender = new FakeEmailSender();
        var sut = CreateSut(sender, customer);

        await sut.NotifyAsync(order, history, CancellationToken.None);

        sender.Sent.Count.ShouldBe(1);
        sender.Sent[0].Subject.ShouldBe($"Ordem de serviço {order.Id}: recebida");
        sender.Sent[0].PlainTextBody.ShouldContain($"A sua ordem de serviço {order.Id} foi recebida.");
    }

    private static ServiceOrderStatusNotifier CreateSut(FakeEmailSender sender, params Customer[] customers)
    {
        var customerRepository = new FakeCustomerRepository();
        foreach (var customer in customers)
        {
            customerRepository.WithCustomer(customer);
        }

        return new ServiceOrderStatusNotifier(customerRepository, sender, NullLogger<ServiceOrderStatusNotifier>.Instance);
    }
}
