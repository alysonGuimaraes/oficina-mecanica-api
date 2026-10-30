using AutoGarage.Application.UseCases.Materials.GetMaterialById;
using AutoGarage.CommonTestUtilities.Mapper;
using AutoGarage.CommonTestUtilities.Repositories.Materials;
using AutoGarage.Domain.Common.Exceptions;
using AutoGarage.Domain.Entities;
using AutoGarage.Domain.Exceptions.Materials.Exceptions;
using AutoGarage.Domain.Shared;
using Shouldly;

namespace AutoGarage.Application.UnitTests.UseCases.Materials.GetMaterialById;

public class GetMaterialByIdUseCaseTests
{
    [Fact]
    public async Task Handle_ExistingMaterial_ShouldReturnResponse()
    {
        var material = Material.Create("PARAFUSO", "Sextavado", 2.50m, 3);
        var sut = CreateSut(material);

        var result = await sut.UseCase.Handle(material.Id.ToString(), CancellationToken.None);

        result.Id.ShouldBe(material.Id);
        result.Name.ShouldBe("PARAFUSO");
        result.Price.ShouldBe(2.50m);
        result.StockQuantity.ShouldBe(0);
        result.StockMinimum.ShouldBe(3);
        result.Status.ShouldBe(EntityStatus.ACTIVE);
    }

    [Fact]
    public async Task Handle_InactiveMaterial_ShouldReturnItWithInactiveStatus()
    {
        var material = Material.Create("PARAFUSO", null, 2.50m, 3);
        material.Deactivate();
        var sut = CreateSut(material);

        var result = await sut.UseCase.Handle(material.Id.ToString(), CancellationToken.None);

        result.Status.ShouldBe(EntityStatus.INACTIVE);
    }

    [Theory]
    [InlineData("abc")]
    [InlineData("123")]
    [InlineData("")]
    public async Task Handle_MalformedId_ShouldThrowInvalidMaterialIdException(string id)
    {
        var sut = CreateSut();

        var exception = await Should.ThrowAsync<InvalidMaterialIdException>(() => sut.UseCase.Handle(id, CancellationToken.None));

        exception.ErrorType.ShouldBe("invalid-material-id");
    }

    [Fact]
    public async Task Handle_UnknownId_ShouldThrowMaterialNotFoundException()
    {
        var sut = CreateSut();

        var exception = await Should.ThrowAsync<MaterialNotFoundException>(() => sut.UseCase.Handle(Guid.NewGuid().ToString(), CancellationToken.None));

        exception.ErrorType.ShouldBe("material-not-found");
        exception.ShouldBeAssignableTo<NotFoundDomainException>();
    }

    [Fact]
    public async Task Handle_UppercaseIdWithBraces_ShouldReturnSameMaterial()
    {
        var material = Material.Create("PARAFUSO", null, 2.50m, 3);
        var sut = CreateSut(material);

        var result = await sut.UseCase.Handle("{" + material.Id.ToString().ToUpperInvariant() + "}", CancellationToken.None);

        result.Id.ShouldBe(material.Id);
    }

    private static GetMaterialByIdUseCaseSut CreateSut(params Material[] materials)
    {
        var materialRepository = new FakeMaterialRepository();

        foreach (var material in materials)
        {
            materialRepository.WithMaterial(material);
        }

        var useCase = new GetMaterialByIdUseCase(materialRepository, MapperBuilder.Build());

        return new GetMaterialByIdUseCaseSut(useCase);
    }

    private sealed record GetMaterialByIdUseCaseSut(GetMaterialByIdUseCase UseCase);
}
