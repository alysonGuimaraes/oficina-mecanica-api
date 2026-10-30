using AutoGarage.Application.Commands.Materials;
using AutoGarage.Application.Services.Normalization;
using AutoGarage.Application.UseCases.Materials.UpdateMaterial;
using AutoGarage.CommonTestUtilities.Mapper;
using AutoGarage.CommonTestUtilities.Repositories;
using AutoGarage.CommonTestUtilities.Repositories.Materials;
using AutoGarage.Domain.Common.Exceptions;
using AutoGarage.Domain.Entities;
using AutoGarage.Domain.Exceptions.Materials.Exceptions;
using AutoGarage.Domain.Shared;
using Shouldly;

namespace AutoGarage.Application.UnitTests.UseCases.Materials.UpdateMaterial;

public class UpdateMaterialUseCaseTests
{
    [Fact]
    public async Task Handle_ValidCommand_ShouldUpdateMaterialKeepStockAndCommit()
    {
        var material = Material.Create("PARAFUSO", "Sextavado", 2.50m, 3);
        var sut = CreateSut(material);

        var result = await sut.UseCase.Handle(
            new UpdateMaterialCommand(material.Id.ToString(), "  porca  sextavada  ", "  aco inox  ", 4.00m, 5),
            CancellationToken.None);

        result.Name.ShouldBe("PORCA SEXTAVADA");
        result.Description.ShouldBe("aco inox");
        result.Price.ShouldBe(4.00m);
        result.StockMinimum.ShouldBe(5);
        result.StockQuantity.ShouldBe(0);
        sut.Materials.Materials[0].Name.ShouldBe("PORCA SEXTAVADA");
        sut.UnitWork.CommitCount.ShouldBe(1);
    }

    [Fact]
    public async Task Handle_SameNameOfSameMaterial_ShouldUpdateWithoutConflict()
    {
        var material = Material.Create("PARAFUSO", "Sextavado", 2.50m, 3);
        var sut = CreateSut(material);

        var result = await sut.UseCase.Handle(
            new UpdateMaterialCommand(material.Id.ToString(), "PARAFUSO", "Nova", 3.00m, 3),
            CancellationToken.None);

        result.Price.ShouldBe(3.00m);
        sut.UnitWork.CommitCount.ShouldBe(1);
    }

    [Fact]
    public async Task Handle_NameOfAnotherMaterial_ShouldThrowDuplicateMaterialNameExceptionAndKeepName()
    {
        var material = Material.Create("PARAFUSO", "Sextavado", 2.50m, 3);
        var other = Material.Create("PORCA", "Com filtro", 1m, 1);
        var sut = CreateSut(material, other);

        var exception = await Should.ThrowAsync<DuplicateMaterialNameException>(() => sut.UseCase.Handle(
            new UpdateMaterialCommand(material.Id.ToString(), "porca", "Com filtro", 2.50m, 3),
            CancellationToken.None));

        exception.ErrorType.ShouldBe("duplicate-material-name");
        sut.Materials.Materials[0].Name.ShouldBe("PARAFUSO");
        sut.UnitWork.CommitCount.ShouldBe(0);
    }

    [Fact]
    public async Task Handle_BlankDescription_ShouldStoreNull()
    {
        var material = Material.Create("PARAFUSO", "Antiga", 2.50m, 3);
        var sut = CreateSut(material);

        var result = await sut.UseCase.Handle(
            new UpdateMaterialCommand(material.Id.ToString(), "PARAFUSO", "   ", 2.50m, 3),
            CancellationToken.None);

        result.Description.ShouldBeNull();
    }

    [Fact]
    public async Task Handle_NullDescription_ShouldThrowErrorOnValidationExceptionAndKeepDescription()
    {
        var material = Material.Create("PARAFUSO", "Antiga", 2.50m, 3);
        var sut = CreateSut(material);

        await Should.ThrowAsync<ErrorOnValidationException>(() => sut.UseCase.Handle(
            new UpdateMaterialCommand(material.Id.ToString(), "PARAFUSO", null, 2.50m, 3),
            CancellationToken.None));

        sut.Materials.Materials[0].Description.ShouldBe("Antiga");
        sut.UnitWork.CommitCount.ShouldBe(0);
    }

    [Fact]
    public async Task Handle_InactiveMaterial_ShouldUpdateAndKeepInactiveStatus()
    {
        var material = Material.Create("PARAFUSO", "Sextavado", 2.50m, 3);
        material.Deactivate();
        var sut = CreateSut(material);

        var result = await sut.UseCase.Handle(
            new UpdateMaterialCommand(material.Id.ToString(), "PARAFUSO", "Com filtro", 3.00m, 3),
            CancellationToken.None);

        result.Status.ShouldBe(EntityStatus.INACTIVE);
        result.Price.ShouldBe(3.00m);
    }

    [Theory]
    [InlineData("abc")]
    [InlineData("123")]
    [InlineData("")]
    public async Task Handle_MalformedId_ShouldThrowInvalidMaterialIdException(string id)
    {
        var sut = CreateSut();

        var exception = await Should.ThrowAsync<InvalidMaterialIdException>(() => sut.UseCase.Handle(
            new UpdateMaterialCommand(id, "PARAFUSO", "Com filtro", 2.50m, 3),
            CancellationToken.None));

        exception.ErrorType.ShouldBe("invalid-material-id");
    }

    [Fact]
    public async Task Handle_UnknownId_ShouldThrowMaterialNotFoundException()
    {
        var sut = CreateSut();

        var exception = await Should.ThrowAsync<MaterialNotFoundException>(() => sut.UseCase.Handle(
            new UpdateMaterialCommand(Guid.NewGuid().ToString(), "PARAFUSO", "Com filtro", 2.50m, 3),
            CancellationToken.None));

        exception.ErrorType.ShouldBe("material-not-found");
    }

    [Fact]
    public async Task Handle_InvalidPrice_ShouldThrowErrorOnValidationExceptionAndKeepPrice()
    {
        var material = Material.Create("PARAFUSO", "Sextavado", 2.50m, 3);
        var sut = CreateSut(material);

        await Should.ThrowAsync<ErrorOnValidationException>(() => sut.UseCase.Handle(
            new UpdateMaterialCommand(material.Id.ToString(), "PARAFUSO", "Com filtro", 0m, 3),
            CancellationToken.None));

        sut.Materials.Materials[0].Price.ShouldBe(2.50m);
        sut.UnitWork.CommitCount.ShouldBe(0);
    }

    [Fact]
    public async Task Handle_ValidUpdate_ShouldNotChangeStockQuantity()
    {
        var material = Material.Create("PARAFUSO", "Sextavado", 2.50m, 3);
        var sut = CreateSut(material);

        var result = await sut.UseCase.Handle(
            new UpdateMaterialCommand(material.Id.ToString(), "PARAFUSO", "Com filtro", 2.50m, 4),
            CancellationToken.None);

        result.StockQuantity.ShouldBe(0);
        sut.Materials.Materials[0].StockQuantity.ShouldBe(0);
    }

    private static UpdateMaterialUseCaseSut CreateSut(params Material[] materials)
    {
        var materialRepository = new FakeMaterialRepository();

        foreach (var material in materials)
        {
            materialRepository.WithMaterial(material);
        }

        var unitWork = new FakeUnitWork();

        var useCase = new UpdateMaterialUseCase(materialRepository, materialRepository, unitWork, new StringNormalizer(), MapperBuilder.Build());

        return new UpdateMaterialUseCaseSut(useCase, materialRepository, unitWork);
    }

    private sealed record UpdateMaterialUseCaseSut(
        UpdateMaterialUseCase UseCase,
        FakeMaterialRepository Materials,
        FakeUnitWork UnitWork);
}
