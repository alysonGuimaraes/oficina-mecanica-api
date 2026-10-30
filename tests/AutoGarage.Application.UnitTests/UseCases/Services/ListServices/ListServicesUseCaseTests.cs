using AutoGarage.Application.UseCases.Services.ListServices;
using AutoGarage.CommonTestUtilities.Mapper;
using AutoGarage.CommonTestUtilities.Repositories.Services;
using AutoGarage.Domain.Entities;
using AutoGarage.Domain.Exceptions.Services.Exceptions;
using AutoGarage.Domain.Shared;
using Shouldly;

namespace AutoGarage.Application.UnitTests.UseCases.Services.ListServices;

public class ListServicesUseCaseTests
{
    [Fact]
    public async Task Handle_NoStatus_ShouldReturnOnlyActiveServices()
    {
        var sut = CreateSut(
            Service.Create("ALINHAMENTO", null, 80m),
            CreateInactive("TROCA DE ÓLEO", 150m));

        var result = await sut.UseCase.Handle(null, CancellationToken.None);

        result.Count.ShouldBe(1);
        result[0].Name.ShouldBe("ALINHAMENTO");
    }

    [Theory]
    [InlineData("INACTIVE")]
    [InlineData("inactive")]
    [InlineData("  Inactive  ")]
    public async Task Handle_InactiveStatus_ShouldReturnOnlyInactiveServices(string status)
    {
        var sut = CreateSut(
            Service.Create("ALINHAMENTO", null, 80m),
            CreateInactive("TROCA DE ÓLEO", 150m));

        var result = await sut.UseCase.Handle(status, CancellationToken.None);

        result.Count.ShouldBe(1);
        result[0].Name.ShouldBe("TROCA DE ÓLEO");
        result[0].Status.ShouldBe(EntityStatus.INACTIVE);
    }

    [Theory]
    [InlineData("FOO")]
    [InlineData("DELETED")]
    public async Task Handle_InvalidStatus_ShouldThrowInvalidServiceStatusException(string status)
    {
        var sut = CreateSut(Service.Create("ALINHAMENTO", null, 80m));

        await Should.ThrowAsync<InvalidServiceStatusException>(() => sut.UseCase.Handle(status, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_NoServicesWithStatus_ShouldReturnEmptyList()
    {
        var sut = CreateSut(Service.Create("ALINHAMENTO", null, 80m));

        var result = await sut.UseCase.Handle("INACTIVE", CancellationToken.None);

        result.ShouldBeEmpty();
    }

    private static Service CreateInactive(string name, decimal price)
    {
        var service = Service.Create(name, null, price);
        service.Deactivate();

        return service;
    }

    private static ListServicesUseCaseSut CreateSut(params Service[] services)
    {
        var serviceRepository = new FakeServiceRepository();

        foreach (var service in services)
        {
            serviceRepository.WithService(service);
        }

        var useCase = new ListServicesUseCase(serviceRepository, MapperBuilder.Build());

        return new ListServicesUseCaseSut(useCase);
    }

    private sealed record ListServicesUseCaseSut(ListServicesUseCase UseCase);
}
