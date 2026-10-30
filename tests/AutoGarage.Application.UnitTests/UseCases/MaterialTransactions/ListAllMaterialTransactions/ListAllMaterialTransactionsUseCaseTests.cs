using AutoGarage.Application.UseCases.MaterialTransactions.ListAllMaterialTransactions;
using AutoGarage.CommonTestUtilities.Mapper;
using AutoGarage.CommonTestUtilities.Repositories.Materials;
using AutoGarage.Domain.Entities;
using AutoGarage.Domain.Enums.MaterialTransactions;
using AutoGarage.Domain.Exceptions.MaterialTransactions.Exceptions;
using Shouldly;

namespace AutoGarage.Application.UnitTests.UseCases.MaterialTransactions.ListAllMaterialTransactions;

public class ListAllMaterialTransactionsUseCaseTests
{
    [Fact]
    public async Task Handle_NoType_ShouldReturnTransactionsOfAllMaterials()
    {
        var sut = await CreateScenarioAsync();

        var result = await sut.UseCase.Handle(null, CancellationToken.None);

        result.Count.ShouldBe(3);
        result.Select(r => r.MaterialId).Distinct().Count().ShouldBe(2);
    }

    [Fact]
    public async Task Handle_TypeIn_ShouldReturnOnlyInTransactionsOfAllMaterials()
    {
        var sut = await CreateScenarioAsync();

        var result = await sut.UseCase.Handle("IN", CancellationToken.None);

        result.Count.ShouldBe(2);
        result.All(r => r.Type == TransactionType.IN).ShouldBeTrue();
    }

    [Fact]
    public async Task Handle_TypeOut_ShouldReturnOnlyOutTransactionsOfAllMaterials()
    {
        var sut = await CreateScenarioAsync();

        var result = await sut.UseCase.Handle("OUT", CancellationToken.None);

        result.Count.ShouldBe(1);
        result[0].Type.ShouldBe(TransactionType.OUT);
        result[0].Quantity.ShouldBe(4);
    }

    [Theory]
    [InlineData("in", TransactionType.IN)]
    [InlineData(" Out ", TransactionType.OUT)]
    public async Task Handle_TypeWithDifferentCaseOrSpaces_ShouldFilterNormally(string type, TransactionType expected)
    {
        var sut = await CreateScenarioAsync();

        var result = await sut.UseCase.Handle(type, CancellationToken.None);

        result.All(r => r.Type == expected).ShouldBeTrue();
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public async Task Handle_BlankType_ShouldReturnTransactionsOfBothTypes(string type)
    {
        var sut = await CreateScenarioAsync();

        var result = await sut.UseCase.Handle(type, CancellationToken.None);

        result.Count.ShouldBe(3);
    }

    [Theory]
    [InlineData("XYZ")]
    [InlineData("ENTRADA")]
    public async Task Handle_InvalidType_ShouldThrowInvalidMaterialTransactionTypeException(string type)
    {
        var sut = await CreateScenarioAsync();

        var exception = await Should.ThrowAsync<InvalidMaterialTransactionTypeException>(() =>
            sut.UseCase.Handle(type, CancellationToken.None));

        exception.ErrorType.ShouldBe("invalid-material-transaction-type");
    }

    [Fact]
    public async Task Handle_NoTransactions_ShouldReturnEmptyList()
    {
        var sut = CreateSut();

        var result = await sut.UseCase.Handle(null, CancellationToken.None);

        result.Count.ShouldBe(0);
    }

    [Fact]
    public async Task Handle_ShouldOrderByCreatedAtThenId()
    {
        var sut = CreateSut();
        var materialA = Guid.NewGuid();
        var materialB = Guid.NewGuid();
        await sut.Transactions.Add(MaterialTransaction.Create(materialA, TransactionType.IN, 10, 10), CancellationToken.None);
        await sut.Transactions.Add(MaterialTransaction.Create(materialB, TransactionType.OUT, 4, 6), CancellationToken.None);
        await sut.Transactions.Add(MaterialTransaction.Create(materialA, TransactionType.IN, 3, 9), CancellationToken.None);

        var expected = sut.Transactions.Transactions
            .OrderBy(t => t.CreatedAt)
            .ThenBy(t => t.Id)
            .Select(t => t.Id)
            .ToList();

        var result = await sut.UseCase.Handle(null, CancellationToken.None);

        result.Select(r => r.Id).ShouldBe(expected);
    }

    private static async Task<ListAllMaterialTransactionsUseCaseSut> CreateScenarioAsync()
    {
        var sut = CreateSut();
        var materialA = Guid.NewGuid();
        var materialB = Guid.NewGuid();
        await sut.Transactions.Add(MaterialTransaction.Create(materialA, TransactionType.IN, 10, 10), CancellationToken.None);
        await sut.Transactions.Add(MaterialTransaction.Create(materialA, TransactionType.OUT, 4, 6), CancellationToken.None);
        await sut.Transactions.Add(MaterialTransaction.Create(materialB, TransactionType.IN, 3, 3), CancellationToken.None);

        return sut;
    }

    private static ListAllMaterialTransactionsUseCaseSut CreateSut()
    {
        var transactionRepository = new FakeMaterialTransactionRepository();

        var useCase = new ListAllMaterialTransactionsUseCase(transactionRepository, MapperBuilder.Build());

        return new ListAllMaterialTransactionsUseCaseSut(useCase, transactionRepository);
    }

    private sealed record ListAllMaterialTransactionsUseCaseSut(
        ListAllMaterialTransactionsUseCase UseCase,
        FakeMaterialTransactionRepository Transactions);
}
