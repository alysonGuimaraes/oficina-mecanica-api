using AutoGarage.Application.UseCases.Materials.ListLowStockMaterials;
using AutoGarage.CommonTestUtilities.Mapper;
using AutoGarage.CommonTestUtilities.Repositories.Materials;
using AutoGarage.Domain.Entities;
using AutoGarage.Domain.Exceptions.Materials.Exceptions;
using AutoGarage.Domain.Shared;
using Shouldly;

namespace AutoGarage.Application.UnitTests.UseCases.Materials.ListLowStockMaterials;

public class ListLowStockMaterialsUseCaseTests
{
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task Handle_NoStatus_ShouldReturnOnlyActiveCriticalMaterials(string? status)
    {
        var sut = CreateSut(
            Material.Create("PARAFUSO", null, 2.50m, 3),
            Material.Create("ARRUELA", null, 0.50m, 0),
            CreateInactive("PORCA", 1m, 2));

        var result = await sut.UseCase.Handle(status, CancellationToken.None);

        result.Count.ShouldBe(1);
        result[0].Name.ShouldBe("PARAFUSO");
    }

    [Theory]
    [InlineData("INACTIVE")]
    [InlineData("inactive")]
    [InlineData("  Inactive  ")]
    public async Task Handle_InactiveStatus_ShouldReturnOnlyInactiveCriticalMaterials(string status)
    {
        var sut = CreateSut(
            Material.Create("PARAFUSO", null, 2.50m, 3),
            Material.Create("ARRUELA", null, 0.50m, 0),
            CreateInactive("PORCA", 1m, 2));

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
    public async Task Handle_ZeroMinimumMaterial_ShouldNotBeCritical()
    {
        var sut = CreateSut(Material.Create("ARRUELA", null, 0.50m, 0));

        var result = await sut.UseCase.Handle(null, CancellationToken.None);

        result.ShouldBeEmpty();
    }

    [Fact]
    public async Task Handle_NoCriticalMaterials_ShouldReturnEmptyList()
    {
        var sut = CreateSut();

        var result = await sut.UseCase.Handle("ACTIVE", CancellationToken.None);

        result.ShouldBeEmpty();
    }

    private static Material CreateInactive(string name, decimal price, int stockMinimum)
    {
        var material = Material.Create(name, null, price, stockMinimum);
        material.Deactivate();

        return material;
    }

    private static ListLowStockMaterialsUseCaseSut CreateSut(params Material[] materials)
    {
        var materialRepository = new FakeMaterialRepository();

        foreach (var material in materials)
        {
            materialRepository.WithMaterial(material);
        }

        var useCase = new ListLowStockMaterialsUseCase(materialRepository, MapperBuilder.Build());

        return new ListLowStockMaterialsUseCaseSut(useCase);
    }

    private sealed record ListLowStockMaterialsUseCaseSut(ListLowStockMaterialsUseCase UseCase);
}
