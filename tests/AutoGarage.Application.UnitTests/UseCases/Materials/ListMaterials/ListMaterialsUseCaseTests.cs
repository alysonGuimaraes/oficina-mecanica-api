using AutoGarage.Application.UseCases.Materials.ListMaterials;
using AutoGarage.CommonTestUtilities.Mapper;
using AutoGarage.CommonTestUtilities.Repositories.Materials;
using AutoGarage.Domain.Entities;
using AutoGarage.Domain.Exceptions.Materials.Exceptions;
using AutoGarage.Domain.Shared;
using Shouldly;

namespace AutoGarage.Application.UnitTests.UseCases.Materials.ListMaterials;

public class ListMaterialsUseCaseTests
{
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task Handle_NoStatus_ShouldReturnOnlyActiveMaterials(string? status)
    {
        var sut = CreateSut(
            Material.Create("PARAFUSO", null, 2.50m, 3),
            CreateInactive("PORCA", 1m, 1));

        var result = await sut.UseCase.Handle(status, CancellationToken.None);

        result.Count.ShouldBe(1);
        result[0].Name.ShouldBe("PARAFUSO");
    }

    [Theory]
    [InlineData("INACTIVE")]
    [InlineData("inactive")]
    [InlineData("  Inactive  ")]
    public async Task Handle_InactiveStatus_ShouldReturnOnlyInactiveMaterials(string status)
    {
        var sut = CreateSut(
            Material.Create("PARAFUSO", null, 2.50m, 3),
            CreateInactive("PORCA", 1m, 1));

        var result = await sut.UseCase.Handle(status, CancellationToken.None);

        result.Count.ShouldBe(1);
        result[0].Name.ShouldBe("PORCA");
        result[0].Status.ShouldBe(EntityStatus.INACTIVE);
    }

    [Theory]
    [InlineData("FOO")]
    [InlineData("DELETED")]
    public async Task Handle_InvalidStatus_ShouldThrowInvalidMaterialStatusException(string status)
    {
        var sut = CreateSut(Material.Create("PARAFUSO", null, 2.50m, 3));

        var exception = await Should.ThrowAsync<InvalidMaterialStatusException>(() => sut.UseCase.Handle(status, CancellationToken.None));

        exception.ErrorType.ShouldBe("invalid-material-status");
    }

    [Fact]
    public async Task Handle_NoMaterialsWithStatus_ShouldReturnEmptyList()
    {
        var sut = CreateSut(Material.Create("PARAFUSO", null, 2.50m, 3));

        var result = await sut.UseCase.Handle("INACTIVE", CancellationToken.None);

        result.ShouldBeEmpty();
    }

    private static Material CreateInactive(string name, decimal price, int stockMinimum)
    {
        var material = Material.Create(name, null, price, stockMinimum);
        material.Deactivate();

        return material;
    }

    private static ListMaterialsUseCaseSut CreateSut(params Material[] materials)
    {
        var materialRepository = new FakeMaterialRepository();

        foreach (var material in materials)
        {
            materialRepository.WithMaterial(material);
        }

        var useCase = new ListMaterialsUseCase(materialRepository, MapperBuilder.Build());

        return new ListMaterialsUseCaseSut(useCase);
    }

    private sealed record ListMaterialsUseCaseSut(ListMaterialsUseCase UseCase);
}
