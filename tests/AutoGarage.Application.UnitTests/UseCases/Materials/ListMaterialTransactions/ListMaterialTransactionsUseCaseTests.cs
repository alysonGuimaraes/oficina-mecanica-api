using AutoGarage.CommonTestUtilities.Mapper;
using AutoGarage.CommonTestUtilities.Repositories.Materials;
using AutoGarage.Application.UseCases.Materials.ListMaterialTransactions;
using AutoGarage.Domain.Entities;
using AutoGarage.Domain.Enums.Materials;
using AutoGarage.Domain.Exceptions.Materials.Exceptions;
using Shouldly;

namespace AutoGarage.Application.UnitTests.UseCases.Materials.ListMaterialTransactions;

public class ListMaterialTransactionsUseCaseTests
{
    [Fact]
    public async Task Handle_NoType_ShouldReturnAllTransactionsOfMaterial()
    {
        var (material, sut) = await CreateScenarioAsync();

        var result = await sut.UseCase.Handle(material.Id.ToString(), null, CancellationToken.None);

        result.Count.ShouldBe(2);
        result.Select(r => r.Type).ShouldBe([TransactionType.IN, TransactionType.OUT], ignoreOrder: true);
    }

    [Fact]
    public async Task Handle_TypeIn_ShouldReturnOnlyInTransactions()
    {
        var (material, sut) = await CreateScenarioAsync();

        var result = await sut.UseCase.Handle(material.Id.ToString(), "IN", CancellationToken.None);

        result.Count.ShouldBe(1);
        result[0].Type.ShouldBe(TransactionType.IN);
        result[0].Quantity.ShouldBe(10);
    }

    [Fact]
    public async Task Handle_TypeOut_ShouldReturnOnlyOutTransactions()
    {
        var (material, sut) = await CreateScenarioAsync();

        var result = await sut.UseCase.Handle(material.Id.ToString(), "OUT", CancellationToken.None);

        result.Count.ShouldBe(1);
        result[0].Type.ShouldBe(TransactionType.OUT);
        result[0].Quantity.ShouldBe(4);
    }

    [Theory]
    [InlineData("in", TransactionType.IN)]
    [InlineData(" Out ", TransactionType.OUT)]
    public async Task Handle_TypeWithDifferentCaseOrSpaces_ShouldFilterNormally(string type, TransactionType expected)
    {
        var (material, sut) = await CreateScenarioAsync();

        var result = await sut.UseCase.Handle(material.Id.ToString(), type, CancellationToken.None);

        result.Count.ShouldBe(1);
        result[0].Type.ShouldBe(expected);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public async Task Handle_BlankType_ShouldReturnAllTransactionsOfMaterial(string type)
    {
        var (material, sut) = await CreateScenarioAsync();

        var result = await sut.UseCase.Handle(material.Id.ToString(), type, CancellationToken.None);

        result.Count.ShouldBe(2);
    }

    [Theory]
    [InlineData("XYZ")]
    [InlineData("ENTRADA")]
    public async Task Handle_InvalidType_ShouldThrowInvalidMaterialTransactionTypeException(string type)
    {
        var (material, sut) = await CreateScenarioAsync();

        var exception = await Should.ThrowAsync<InvalidMaterialTransactionTypeException>(() =>
            sut.UseCase.Handle(material.Id.ToString(), type, CancellationToken.None));

        exception.ErrorType.ShouldBe("invalid-material-transaction-type");
    }

    [Theory]
    [InlineData("abc")]
    [InlineData("123")]
    [InlineData("")]
    public async Task Handle_MalformedId_ShouldThrowInvalidMaterialIdException(string id)
    {
        var sut = CreateSut();

        var exception = await Should.ThrowAsync<InvalidMaterialIdException>(() =>
            sut.UseCase.Handle(id, null, CancellationToken.None));

        exception.ErrorType.ShouldBe("invalid-material-id");
    }

    [Fact]
    public async Task Handle_UnknownMaterial_ShouldThrowMaterialNotFoundException()
    {
        var sut = CreateSut();

        var exception = await Should.ThrowAsync<MaterialNotFoundException>(() =>
            sut.UseCase.Handle(Guid.NewGuid().ToString(), null, CancellationToken.None));

        exception.ErrorType.ShouldBe("material-not-found");
    }

    [Fact]
    public async Task Handle_InvalidTypeOnUnknownMaterial_ShouldThrowInvalidTypeBeforeLookup()
    {
        var sut = CreateSut();

        await Should.ThrowAsync<InvalidMaterialTransactionTypeException>(() =>
            sut.UseCase.Handle(Guid.NewGuid().ToString(), "XYZ", CancellationToken.None));
    }

    [Fact]
    public async Task Handle_MaterialWithoutTransactions_ShouldReturnEmptyList()
    {
        var material = Material.Create("PARAFUSO", null, 2.50m, 3);
        var sut = CreateSut(material);

        var result = await sut.UseCase.Handle(material.Id.ToString(), null, CancellationToken.None);

        result.Count.ShouldBe(0);
    }

    [Fact]
    public async Task Handle_TransactionsOfOtherMaterial_ShouldNotBeIncluded()
    {
        var material = Material.Create("PARAFUSO", null, 2.50m, 3);
        var other = Material.Create("PORCA", null, 1m, 1);
        var sut = CreateSut(material, other);
        await sut.Transactions.Add(MaterialTransaction.Create(material.Id, TransactionType.IN, 10, 10), CancellationToken.None);
        await sut.Transactions.Add(MaterialTransaction.Create(other.Id, TransactionType.IN, 3, 3), CancellationToken.None);

        var result = await sut.UseCase.Handle(material.Id.ToString(), null, CancellationToken.None);

        result.Count.ShouldBe(1);
        result[0].MaterialId.ShouldBe(material.Id);
    }

    [Fact]
    public async Task Handle_ShouldOrderByCreatedAtThenId()
    {
        var material = Material.Create("PARAFUSO", null, 2.50m, 3);
        var sut = CreateSut(material);
        await sut.Transactions.Add(MaterialTransaction.Create(material.Id, TransactionType.IN, 10, 10), CancellationToken.None);
        await sut.Transactions.Add(MaterialTransaction.Create(material.Id, TransactionType.OUT, 4, 6), CancellationToken.None);
        await sut.Transactions.Add(MaterialTransaction.Create(material.Id, TransactionType.IN, 3, 9), CancellationToken.None);

        var expected = sut.Transactions.Transactions
            .OrderBy(t => t.CreatedAt)
            .ThenBy(t => t.Id)
            .Select(t => t.Id)
            .ToList();

        var result = await sut.UseCase.Handle(material.Id.ToString(), null, CancellationToken.None);

        result.Select(r => r.Id).ShouldBe(expected);
    }

    private static async Task<(Material Material, ListMaterialTransactionsUseCaseSut Sut)> CreateScenarioAsync()
    {
        var material = Material.Create("PARAFUSO", null, 2.50m, 3);
        var sut = CreateSut(material);
        await sut.Transactions.Add(MaterialTransaction.Create(material.Id, TransactionType.IN, 10, 10), CancellationToken.None);
        await sut.Transactions.Add(MaterialTransaction.Create(material.Id, TransactionType.OUT, 4, 6), CancellationToken.None);

        return (material, sut);
    }

    private static ListMaterialTransactionsUseCaseSut CreateSut(params Material[] materials)
    {
        var materialRepository = new FakeMaterialRepository();

        foreach (var material in materials)
        {
            materialRepository.WithMaterial(material);
        }

        var transactionRepository = new FakeMaterialTransactionRepository();

        var useCase = new ListMaterialTransactionsUseCase(materialRepository, transactionRepository, MapperBuilder.Build());

        return new ListMaterialTransactionsUseCaseSut(useCase, materialRepository, transactionRepository);
    }

    private sealed record ListMaterialTransactionsUseCaseSut(
        ListMaterialTransactionsUseCase UseCase,
        FakeMaterialRepository Materials,
        FakeMaterialTransactionRepository Transactions);
}
