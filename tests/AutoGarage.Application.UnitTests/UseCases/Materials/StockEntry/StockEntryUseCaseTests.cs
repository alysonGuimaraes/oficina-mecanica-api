using AutoGarage.Application.Commands.Materials;
using AutoGarage.Application.UseCases.Materials.StockEntry;
using AutoGarage.CommonTestUtilities.Mapper;
using AutoGarage.CommonTestUtilities.Repositories;
using AutoGarage.CommonTestUtilities.Repositories.Materials;
using AutoGarage.Domain.Common.Exceptions;
using AutoGarage.Domain.Entities;
using AutoGarage.Domain.Enums.MaterialTransactions;
using AutoGarage.Domain.Exceptions.Materials.Exceptions;
using AutoGarage.Domain.Shared;
using Shouldly;

namespace AutoGarage.Application.UnitTests.UseCases.Materials.StockEntry;

public class StockEntryUseCaseTests
{
    [Fact]
    public async Task Handle_ValidEntry_ShouldIncreaseStockAddInTransactionAndCommit()
    {
        var material = Material.Create("PARAFUSO", "Sextavado", 2.50m, 3);
        var sut = CreateSut(material);

        var result = await sut.UseCase.Handle(new StockEntryCommand(material.Id.ToString(), 5), CancellationToken.None);

        result.StockQuantity.ShouldBe(5);
        sut.Materials.Materials[0].StockQuantity.ShouldBe(5);
        sut.Transactions.Transactions.Count.ShouldBe(1);

        var transaction = sut.Transactions.Transactions[0];
        transaction.Type.ShouldBe(TransactionType.IN);
        transaction.Quantity.ShouldBe(5);
        transaction.BalanceAfter.ShouldBe(5);
        transaction.MaterialId.ShouldBe(material.Id);
        sut.UnitWork.CommitCount.ShouldBe(1);
    }

    [Fact]
    public async Task Handle_EntryKeepsNamePriceStockMinimumAndStatus()
    {
        var material = Material.Create("PARAFUSO", "Sextavado", 2.50m, 3);
        var sut = CreateSut(material);

        await sut.UseCase.Handle(new StockEntryCommand(material.Id.ToString(), 5), CancellationToken.None);

        var stored = sut.Materials.Materials[0];
        stored.Name.ShouldBe("PARAFUSO");
        stored.Description.ShouldBe("Sextavado");
        stored.Price.ShouldBe(2.50m);
        stored.StockMinimum.ShouldBe(3);
        stored.Status.ShouldBe(EntityStatus.ACTIVE);
    }

    [Fact]
    public async Task Handle_SuccessiveEntries_ShouldAccumulateStockAndRegisterBalanceAfterEachEntry()
    {
        var material = Material.Create("PARAFUSO", null, 2.50m, 3);
        var sut = CreateSut(material);

        await sut.UseCase.Handle(new StockEntryCommand(material.Id.ToString(), 5), CancellationToken.None);
        await sut.UseCase.Handle(new StockEntryCommand(material.Id.ToString(), 3), CancellationToken.None);

        sut.Materials.Materials[0].StockQuantity.ShouldBe(8);
        sut.Transactions.Transactions.Count.ShouldBe(2);
        sut.Transactions.Transactions[0].BalanceAfter.ShouldBe(5);
        sut.Transactions.Transactions[1].BalanceAfter.ShouldBe(8);
    }

    [Fact]
    public async Task Handle_InactiveMaterial_ShouldAcceptEntryKeepingInactiveStatus()
    {
        var material = Material.Create("PARAFUSO", null, 2.50m, 3);
        material.Deactivate();
        var sut = CreateSut(material);

        var result = await sut.UseCase.Handle(new StockEntryCommand(material.Id.ToString(), 5), CancellationToken.None);

        result.StockQuantity.ShouldBe(5);
        result.Status.ShouldBe(EntityStatus.INACTIVE);
        sut.Transactions.Transactions.Count.ShouldBe(1);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task Handle_QuantityNotPositive_ShouldThrowErrorOnValidationExceptionAndKeepStock(int quantity)
    {
        var material = Material.Create("PARAFUSO", null, 2.50m, 3);
        var sut = CreateSut(material);

        var exception = await Should.ThrowAsync<ErrorOnValidationException>(() =>
            sut.UseCase.Handle(new StockEntryCommand(material.Id.ToString(), quantity), CancellationToken.None));

        exception.ErrorsMessages.ShouldContain("The quantity field must be greater than zero.");
        sut.Materials.Materials[0].StockQuantity.ShouldBe(0);
        sut.Transactions.Transactions.Count.ShouldBe(0);
        sut.UnitWork.CommitCount.ShouldBe(0);
    }

    [Theory]
    [InlineData("abc")]
    [InlineData("123")]
    [InlineData("")]
    public async Task Handle_MalformedId_ShouldThrowInvalidMaterialIdException(string id)
    {
        var sut = CreateSut();

        var exception = await Should.ThrowAsync<InvalidMaterialIdException>(() =>
            sut.UseCase.Handle(new StockEntryCommand(id, 5), CancellationToken.None));

        exception.ErrorType.ShouldBe("invalid-material-id");
        sut.Transactions.Transactions.Count.ShouldBe(0);
    }

    [Fact]
    public async Task Handle_UnknownId_ShouldThrowMaterialNotFoundException()
    {
        var sut = CreateSut();

        var exception = await Should.ThrowAsync<MaterialNotFoundException>(() =>
            sut.UseCase.Handle(new StockEntryCommand(Guid.NewGuid().ToString(), 5), CancellationToken.None));

        exception.ErrorType.ShouldBe("material-not-found");
        sut.Transactions.Transactions.Count.ShouldBe(0);
    }

    private static StockEntryUseCaseSut CreateSut(params Material[] materials)
    {
        var materialRepository = new FakeMaterialRepository();

        foreach (var material in materials)
        {
            materialRepository.WithMaterial(material);
        }

        var transactionRepository = new FakeMaterialTransactionRepository();
        var unitWork = new FakeUnitWork();

        var useCase = new StockEntryUseCase(materialRepository, materialRepository, transactionRepository, unitWork, MapperBuilder.Build());

        return new StockEntryUseCaseSut(useCase, materialRepository, transactionRepository, unitWork);
    }

    private sealed record StockEntryUseCaseSut(
        StockEntryUseCase UseCase,
        FakeMaterialRepository Materials,
        FakeMaterialTransactionRepository Transactions,
        FakeUnitWork UnitWork);
}
