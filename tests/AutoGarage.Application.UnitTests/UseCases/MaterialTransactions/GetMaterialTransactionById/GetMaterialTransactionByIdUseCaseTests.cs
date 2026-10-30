using AutoGarage.Application.UseCases.MaterialTransactions.GetMaterialTransactionById;
using AutoGarage.CommonTestUtilities.Mapper;
using AutoGarage.CommonTestUtilities.Repositories.Materials;
using AutoGarage.Domain.Entities;
using AutoGarage.Domain.Enums.MaterialTransactions;
using AutoGarage.Domain.Exceptions.MaterialTransactions.Exceptions;
using Shouldly;

namespace AutoGarage.Application.UnitTests.UseCases.MaterialTransactions.GetMaterialTransactionById;

public class GetMaterialTransactionByIdUseCaseTests
{
    [Fact]
    public async Task Handle_ExistingId_ShouldReturnTransaction()
    {
        var materialId = Guid.NewGuid();
        var sut = CreateSut();
        var saved = MaterialTransaction.Create(materialId, TransactionType.IN, 5, 5);
        await sut.Transactions.Add(saved, CancellationToken.None);

        var result = await sut.UseCase.Handle(saved.Id.ToString(), CancellationToken.None);

        result.Id.ShouldBe(saved.Id);
        result.MaterialId.ShouldBe(materialId);
        result.Type.ShouldBe(TransactionType.IN);
        result.Quantity.ShouldBe(5);
        result.BalanceAfter.ShouldBe(5);
        result.CreatedAt.ShouldBe(saved.CreatedAt);
    }

    [Fact]
    public async Task Handle_UnknownId_ShouldThrowMaterialTransactionNotFoundException()
    {
        var sut = CreateSut();

        var exception = await Should.ThrowAsync<MaterialTransactionNotFoundException>(() =>
            sut.UseCase.Handle(Guid.NewGuid().ToString(), CancellationToken.None));

        exception.ErrorType.ShouldBe("material-transaction-not-found");
    }

    [Theory]
    [InlineData("abc")]
    [InlineData("123")]
    [InlineData("")]
    public async Task Handle_MalformedId_ShouldThrowInvalidMaterialTransactionIdException(string id)
    {
        var sut = CreateSut();

        var exception = await Should.ThrowAsync<InvalidMaterialTransactionIdException>(() =>
            sut.UseCase.Handle(id, CancellationToken.None));

        exception.ErrorType.ShouldBe("invalid-material-transaction-id");
    }

    private static GetMaterialTransactionByIdUseCaseSut CreateSut()
    {
        var transactionRepository = new FakeMaterialTransactionRepository();

        var useCase = new GetMaterialTransactionByIdUseCase(transactionRepository, MapperBuilder.Build());

        return new GetMaterialTransactionByIdUseCaseSut(useCase, transactionRepository);
    }

    private sealed record GetMaterialTransactionByIdUseCaseSut(
        GetMaterialTransactionByIdUseCase UseCase,
        FakeMaterialTransactionRepository Transactions);
}
