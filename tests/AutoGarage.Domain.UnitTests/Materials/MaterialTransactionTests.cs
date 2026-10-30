using AutoGarage.Domain.Entities;
using AutoGarage.Domain.Enums.MaterialTransactions;
using Shouldly;

namespace AutoGarage.Domain.UnitTests.Materials;

public class MaterialTransactionTests
{
    [Fact]
    public void Create_ValidValues_ShouldKeepValuesAndSetIdAndCreatedAt()
    {
        var materialId = Guid.NewGuid();

        var transaction = MaterialTransaction.Create(materialId, TransactionType.IN, 5, 5);

        transaction.MaterialId.ShouldBe(materialId);
        transaction.Type.ShouldBe(TransactionType.IN);
        transaction.Quantity.ShouldBe(5);
        transaction.BalanceAfter.ShouldBe(5);
        transaction.Id.ShouldNotBe(Guid.Empty);
        transaction.CreatedAt.ShouldNotBe(default);
    }

    [Fact]
    public void Create_ShouldSetCreatedAtInUtc()
    {
        var before = DateTime.UtcNow;

        var transaction = MaterialTransaction.Create(Guid.NewGuid(), TransactionType.IN, 5, 5);

        transaction.CreatedAt.ShouldBeGreaterThanOrEqualTo(before);
        transaction.CreatedAt.ShouldBeLessThanOrEqualTo(DateTime.UtcNow);
        transaction.CreatedAt.Kind.ShouldBe(DateTimeKind.Utc);
    }
}
