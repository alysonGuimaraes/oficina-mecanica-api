using AutoGarage.Domain.Entities;
using AutoGarage.Domain.Enums.ServiceOrders;
using Shouldly;

namespace AutoGarage.Domain.UnitTests.ServiceOrders;

public class ServiceOrderApprovalTests
{
    private const int DeadlineDays = 10;

    [Fact]
    public void IsBudgetApprovalExpired_BeforeDeadline_ReturnsFalse()
    {
        var order = CreateAwaitingApproval();
        var now = order.BudgetSentAt!.Value.AddDays(9);

        order.IsBudgetApprovalExpired(now, DeadlineDays).ShouldBeFalse();
    }

    [Fact]
    public void IsBudgetApprovalExpired_ExactlyAtDeadline_ReturnsFalse()
    {
        var order = CreateAwaitingApproval();
        var now = order.BudgetSentAt!.Value.AddDays(10);

        order.IsBudgetApprovalExpired(now, DeadlineDays).ShouldBeFalse();
    }

    [Fact]
    public void IsBudgetApprovalExpired_AfterDeadline_ReturnsTrue()
    {
        var order = CreateAwaitingApproval();
        var now = order.BudgetSentAt!.Value.AddDays(10).AddSeconds(1);

        order.IsBudgetApprovalExpired(now, DeadlineDays).ShouldBeTrue();
    }

    [Fact]
    public void IsBudgetApprovalExpired_WithoutBudgetSent_ReturnsFalse()
    {
        var order = ServiceOrder.Create(Guid.NewGuid(), Guid.NewGuid());

        order.IsBudgetApprovalExpired(DateTime.UtcNow.AddYears(1), DeadlineDays).ShouldBeFalse();
    }

    [Fact]
    public void ChangeStatus_CustomerAction_RecordsNullEmployeeAndCustomerFlag()
    {
        var order = CreateAwaitingApproval();

        var history = order.ChangeStatus(ServiceOrderStatus.IN_EXECUTION, null, isCustomerAction: true);

        order.Status.ShouldBe(ServiceOrderStatus.IN_EXECUTION);
        history.EmployeeId.ShouldBeNull();
        history.IsCustomerAction.ShouldBeTrue();
        history.ToStatus.ShouldBe(ServiceOrderStatus.IN_EXECUTION);
    }

    [Fact]
    public void ChangeStatus_WithoutEmployeeOrCustomerAction_ShouldThrowArgumentException()
    {
        var order = CreateAwaitingApproval();

        Should.Throw<ArgumentException>(() => order.ChangeStatus(ServiceOrderStatus.IN_EXECUTION, null, isCustomerAction: false));

        order.Status.ShouldBe(ServiceOrderStatus.AWAITING_APPROVAL);
    }

    [Fact]
    public void ChangeStatus_WithEmployeeAndCustomerAction_ShouldThrowArgumentException()
    {
        var order = CreateAwaitingApproval();

        Should.Throw<ArgumentException>(() => order.ChangeStatus(ServiceOrderStatus.IN_EXECUTION, Guid.NewGuid(), isCustomerAction: true));

        order.Status.ShouldBe(ServiceOrderStatus.AWAITING_APPROVAL);
    }

    private static ServiceOrder CreateAwaitingApproval()
    {
        var employeeId = Guid.NewGuid();
        var order = ServiceOrder.Create(Guid.NewGuid(), Guid.NewGuid());
        order.ChangeStatus(ServiceOrderStatus.IN_DIAGNOSIS, employeeId);
        order.SendBudget(50m, employeeId);

        return order;
    }
}
