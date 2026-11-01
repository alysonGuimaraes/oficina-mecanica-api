using AutoGarage.Domain.Entities;
using AutoGarage.Domain.Enums.MaterialTransactions;
using Shouldly;

namespace AutoGarage.Domain.UnitTests.MaterialTransactions;

public class MaterialTransactionServiceOrderTests
{
    [Fact]
    public void Create_WithServiceOrderId_ShouldKeepTheLink()
    {
        var materialId = Guid.NewGuid();
        var orderId = Guid.NewGuid();

        var transaction = MaterialTransaction.Create(materialId, TransactionType.OUT, 2, 3, orderId);

        transaction.ServiceOrderId.ShouldBe(orderId);
    }

    [Fact]
    public void Create_WithoutServiceOrderId_ShouldLeaveItNull()
    {
        var transaction = MaterialTransaction.Create(Guid.NewGuid(), TransactionType.IN, 2, 5);

        transaction.ServiceOrderId.ShouldBeNull();
    }
}
