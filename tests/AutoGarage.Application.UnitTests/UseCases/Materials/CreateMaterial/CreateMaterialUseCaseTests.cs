using AutoGarage.Application.Commands.Materials;
using AutoGarage.Application.Services.Normalization;
using AutoGarage.Application.UseCases.Materials.CreateMaterial;
using AutoGarage.CommonTestUtilities.Mapper;
using AutoGarage.CommonTestUtilities.Repositories;
using AutoGarage.CommonTestUtilities.Repositories.Materials;
using AutoGarage.Domain.Common.Exceptions;
using AutoGarage.Domain.Entities;
using AutoGarage.Domain.Exceptions.Materials.Exceptions;
using AutoGarage.Domain.Shared;
using Shouldly;

namespace AutoGarage.Application.UnitTests.UseCases.Materials.CreateMaterial;

public class CreateMaterialUseCaseTests
{
    [Fact]
    public async Task Handle_ValidCommand_ShouldCreateActiveMaterialWithZeroStockAndCommit()
    {
        var sut = CreateSut();

        var result = await sut.UseCase.Handle(
            new CreateMaterialCommand("  parafuso  sextavado  ", "  aco inox  ", 2.50m, 3),
            CancellationToken.None);

        result.Name.ShouldBe("PARAFUSO SEXTAVADO");
        result.Description.ShouldBe("aco inox");
        result.Price.ShouldBe(2.50m);
        result.StockQuantity.ShouldBe(0);
        result.StockMinimum.ShouldBe(3);
        result.Status.ShouldBe(EntityStatus.ACTIVE);
        sut.Materials.Materials.Count.ShouldBe(1);
        sut.UnitWork.CommitCount.ShouldBe(1);
    }

    [Fact]
    public async Task Handle_BlankDescription_ShouldStoreNull()
    {
        var sut = CreateSut();

        var result = await sut.UseCase.Handle(
            new CreateMaterialCommand("PARAFUSO", "   ", 2.50m, 3),
            CancellationToken.None);

        result.Description.ShouldBeNull();
    }

    [Fact]
    public async Task Handle_NameAlreadyRegisteredIgnoringCaseAndSpacing_ShouldThrowDuplicateMaterialNameException()
    {
        var sut = CreateSut(Material.Create("PARAFUSO SEXTAVADO", null, 1m, 1));

        var exception = await Should.ThrowAsync<DuplicateMaterialNameException>(() => sut.UseCase.Handle(
            new CreateMaterialCommand("parafuso  sextavado", null, 2.50m, 3),
            CancellationToken.None));

        exception.ErrorType.ShouldBe("duplicate-material-name");
        sut.Materials.Materials.Count.ShouldBe(1);
        sut.UnitWork.CommitCount.ShouldBe(0);
    }

    [Fact]
    public async Task Handle_NegativePrice_ShouldThrowErrorOnValidationExceptionWithoutQueryingRepository()
    {
        var sut = CreateSut();

        await Should.ThrowAsync<ErrorOnValidationException>(() => sut.UseCase.Handle(
            new CreateMaterialCommand("PARAFUSO", null, -1m, 3),
            CancellationToken.None));

        sut.Materials.Materials.Count.ShouldBe(0);
    }

    private static CreateMaterialUseCaseSut CreateSut(params Material[] materials)
    {
        var materialRepository = new FakeMaterialRepository();

        foreach (var material in materials)
        {
            materialRepository.WithMaterial(material);
        }

        var unitWork = new FakeUnitWork();

        var useCase = new CreateMaterialUseCase(materialRepository, materialRepository, unitWork, new StringNormalizer(), MapperBuilder.Build());

        return new CreateMaterialUseCaseSut(useCase, materialRepository, unitWork);
    }

    private sealed record CreateMaterialUseCaseSut(
        CreateMaterialUseCase UseCase,
        FakeMaterialRepository Materials,
        FakeUnitWork UnitWork);
}
