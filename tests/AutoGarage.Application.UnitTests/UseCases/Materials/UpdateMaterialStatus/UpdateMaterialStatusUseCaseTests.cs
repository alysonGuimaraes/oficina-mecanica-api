using AutoGarage.Application.Commands.Materials;
using AutoGarage.Application.UseCases.Materials.UpdateMaterialStatus;
using AutoGarage.CommonTestUtilities.Mapper;
using AutoGarage.CommonTestUtilities.Repositories;
using AutoGarage.CommonTestUtilities.Repositories.Materials;
using AutoGarage.Domain.Entities;
using AutoGarage.Domain.Exceptions.Materials.Exceptions;
using AutoGarage.Domain.Shared;
using Shouldly;

namespace AutoGarage.Application.UnitTests.UseCases.Materials.UpdateMaterialStatus;

public class UpdateMaterialStatusUseCaseTests
{
    [Fact]
    public async Task Handle_ActiveMaterialToInactive_ShouldDeactivateAndCommit()
    {
        var material = Material.Create("PARAFUSO", "Sextavado", 2.50m, 3);
        var sut = CreateSut(material);

        var result = await sut.UseCase.Handle(new UpdateMaterialStatusCommand(material.Id.ToString(), "INACTIVE"), CancellationToken.None);

        result.Status.ShouldBe(EntityStatus.INACTIVE);
        sut.Materials.Materials[0].Status.ShouldBe(EntityStatus.INACTIVE);
        sut.UnitWork.CommitCount.ShouldBe(1);
    }

    [Fact]
    public async Task Handle_Deactivate_ShouldKeepNameDescriptionPriceStockMinimumStockAndCreatedAt()
    {
        var material = Material.Create("PARAFUSO", "Sextavado", 2.50m, 3);
        var sut = CreateSut(material);

        await sut.UseCase.Handle(new UpdateMaterialStatusCommand(material.Id.ToString(), "INACTIVE"), CancellationToken.None);

        var stored = sut.Materials.Materials[0];
        stored.Name.ShouldBe("PARAFUSO");
        stored.Description.ShouldBe("Sextavado");
        stored.Price.ShouldBe(2.50m);
        stored.StockMinimum.ShouldBe(3);
        stored.StockQuantity.ShouldBe(0);
        stored.CreatedAt.ShouldBe(material.CreatedAt);
    }

    [Fact]
    public async Task Handle_InactiveMaterialToActive_ShouldActivateAndCommit()
    {
        var material = Material.Create("PARAFUSO", null, 2.50m, 3);
        material.Deactivate();
        var sut = CreateSut(material);

        var result = await sut.UseCase.Handle(new UpdateMaterialStatusCommand(material.Id.ToString(), "ACTIVE"), CancellationToken.None);

        result.Status.ShouldBe(EntityStatus.ACTIVE);
        sut.UnitWork.CommitCount.ShouldBe(1);
    }

    [Theory]
    [InlineData("inactive")]
    [InlineData(" Inactive ")]
    public async Task Handle_StatusInAnyCaseOrWithSpaces_ShouldBeAccepted(string status)
    {
        var material = Material.Create("PARAFUSO", null, 2.50m, 3);
        var sut = CreateSut(material);

        var result = await sut.UseCase.Handle(new UpdateMaterialStatusCommand(material.Id.ToString(), status), CancellationToken.None);

        result.Status.ShouldBe(EntityStatus.INACTIVE);
    }

    [Theory]
    [InlineData("FOO")]
    [InlineData("DELETED")]
    [InlineData("")]
    public async Task Handle_InvalidStatus_ShouldThrowInvalidMaterialStatusExceptionAndKeepMaterial(string status)
    {
        var material = Material.Create("PARAFUSO", null, 2.50m, 3);
        var sut = CreateSut(material);

        var exception = await Should.ThrowAsync<InvalidMaterialStatusException>(() =>
            sut.UseCase.Handle(new UpdateMaterialStatusCommand(material.Id.ToString(), status), CancellationToken.None));

        exception.ErrorType.ShouldBe("invalid-material-status");
        sut.Materials.Materials[0].Status.ShouldBe(EntityStatus.ACTIVE);
        sut.UnitWork.CommitCount.ShouldBe(0);
    }

    [Fact]
    public async Task Handle_ActivateAlreadyActive_ShouldThrowMaterialAlreadyActiveExceptionAndKeepUpdatedAt()
    {
        var material = Material.Create("PARAFUSO", null, 2.50m, 3);
        var sut = CreateSut(material);

        var exception = await Should.ThrowAsync<MaterialAlreadyActiveException>(() =>
            sut.UseCase.Handle(new UpdateMaterialStatusCommand(material.Id.ToString(), "ACTIVE"), CancellationToken.None));

        exception.ErrorType.ShouldBe("material-already-active");
        sut.Materials.Materials[0].UpdatedAt.ShouldBeNull();
        sut.UnitWork.CommitCount.ShouldBe(0);
    }

    [Fact]
    public async Task Handle_DeactivateAlreadyInactive_ShouldThrowMaterialAlreadyInactiveException()
    {
        var material = Material.Create("PARAFUSO", null, 2.50m, 3);
        material.Deactivate();
        var sut = CreateSut(material);

        var exception = await Should.ThrowAsync<MaterialAlreadyInactiveException>(() =>
            sut.UseCase.Handle(new UpdateMaterialStatusCommand(material.Id.ToString(), "INACTIVE"), CancellationToken.None));

        exception.ErrorType.ShouldBe("material-already-inactive");
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
            sut.UseCase.Handle(new UpdateMaterialStatusCommand(id, "INACTIVE"), CancellationToken.None));

        exception.ErrorType.ShouldBe("invalid-material-id");
    }

    [Fact]
    public async Task Handle_UnknownId_ShouldThrowMaterialNotFoundException()
    {
        var sut = CreateSut();

        var exception = await Should.ThrowAsync<MaterialNotFoundException>(() =>
            sut.UseCase.Handle(new UpdateMaterialStatusCommand(Guid.NewGuid().ToString(), "INACTIVE"), CancellationToken.None));

        exception.ErrorType.ShouldBe("material-not-found");
    }

    private static UpdateMaterialStatusUseCaseSut CreateSut(params Material[] materials)
    {
        var materialRepository = new FakeMaterialRepository();

        foreach (var material in materials)
        {
            materialRepository.WithMaterial(material);
        }

        var unitWork = new FakeUnitWork();

        var useCase = new UpdateMaterialStatusUseCase(materialRepository, materialRepository, unitWork, MapperBuilder.Build());

        return new UpdateMaterialStatusUseCaseSut(useCase, materialRepository, unitWork);
    }

    private sealed record UpdateMaterialStatusUseCaseSut(
        UpdateMaterialStatusUseCase UseCase,
        FakeMaterialRepository Materials,
        FakeUnitWork UnitWork);
}
