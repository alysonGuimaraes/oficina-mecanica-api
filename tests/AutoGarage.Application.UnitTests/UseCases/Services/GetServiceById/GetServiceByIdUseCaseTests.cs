using AutoGarage.Application.UseCases.Services.GetServiceById;
using AutoGarage.CommonTestUtilities.Mapper;
using AutoGarage.CommonTestUtilities.Repositories.Services;
using AutoGarage.Domain.Common.Exceptions;
using AutoGarage.Domain.Entities;
using AutoGarage.Domain.Exceptions.Services.Exceptions;
using AutoGarage.Domain.Shared;
using Shouldly;

namespace AutoGarage.Application.UnitTests.UseCases.Services.GetServiceById;

public class GetServiceByIdUseCaseTests
{
    [Fact]
    public async Task Handle_ExistingService_ShouldReturnResponse()
    {
        var service = Service.Create("TROCA DE ÓLEO", "Com filtro", 150.50m);
        var sut = CreateSut(service);

        var result = await sut.UseCase.Handle(service.Id.ToString(), CancellationToken.None);

        result.Id.ShouldBe(service.Id);
        result.Name.ShouldBe("TROCA DE ÓLEO");
        result.Price.ShouldBe(150.50m);
        result.Status.ShouldBe(EntityStatus.ACTIVE);
    }

    [Fact]
    public async Task Handle_InactiveService_ShouldReturnItWithInactiveStatus()
    {
        var service = Service.Create("ALINHAMENTO", null, 80m);
        service.Deactivate();
        var sut = CreateSut(service);

        var result = await sut.UseCase.Handle(service.Id.ToString(), CancellationToken.None);

        result.Status.ShouldBe(EntityStatus.INACTIVE);
    }

    [Theory]
    [InlineData("abc")]
    [InlineData("123")]
    [InlineData("")]
    public async Task Handle_MalformedId_ShouldThrowInvalidServiceIdException(string id)
    {
        var sut = CreateSut();

        var exception = await Should.ThrowAsync<InvalidServiceIdException>(() => sut.UseCase.Handle(id, CancellationToken.None));

        exception.ErrorType.ShouldBe("invalid-service-id");
    }

    [Fact]
    public async Task Handle_UnknownId_ShouldThrowServiceNotFoundException()
    {
        var sut = CreateSut();

        var exception = await Should.ThrowAsync<ServiceNotFoundException>(() => sut.UseCase.Handle(Guid.NewGuid().ToString(), CancellationToken.None));

        exception.ErrorType.ShouldBe("service-not-found");
        exception.ShouldBeAssignableTo<NotFoundDomainException>();
    }

    [Fact]
    public async Task Handle_UppercaseIdWithBraces_ShouldReturnSameService()
    {
        var service = Service.Create("ALINHAMENTO", null, 80m);
        var sut = CreateSut(service);

        var result = await sut.UseCase.Handle("{" + service.Id.ToString().ToUpperInvariant() + "}", CancellationToken.None);

        result.Id.ShouldBe(service.Id);
    }

    private static GetServiceByIdUseCaseSut CreateSut(params Service[] services)
    {
        var serviceRepository = new FakeServiceRepository();

        foreach (var service in services)
        {
            serviceRepository.WithService(service);
        }

        var useCase = new GetServiceByIdUseCase(serviceRepository, MapperBuilder.Build());

        return new GetServiceByIdUseCaseSut(useCase);
    }

    private sealed record GetServiceByIdUseCaseSut(GetServiceByIdUseCase UseCase);
}
