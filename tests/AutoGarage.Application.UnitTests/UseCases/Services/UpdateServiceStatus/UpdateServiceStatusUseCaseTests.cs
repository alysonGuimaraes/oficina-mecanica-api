using AutoGarage.Application.Commands.Services;
using AutoGarage.Application.UseCases.Services.UpdateServiceStatus;
using AutoGarage.CommonTestUtilities.Mapper;
using AutoGarage.CommonTestUtilities.Repositories;
using AutoGarage.CommonTestUtilities.Repositories.Services;
using AutoGarage.Domain.Entities;
using AutoGarage.Domain.Exceptions.Services.Exceptions;
using AutoGarage.Domain.Shared;
using Shouldly;

namespace AutoGarage.Application.UnitTests.UseCases.Services.UpdateServiceStatus;

public class UpdateServiceStatusUseCaseTests
{
    [Fact]
    public async Task Handle_ActiveServiceToInactive_ShouldDeactivateAndCommit()
    {
        var service = Service.Create("ALINHAMENTO", "Com filtro", 80m);
        var sut = CreateSut(service);

        var result = await sut.UseCase.Handle(new UpdateServiceStatusCommand(service.Id.ToString(), "INACTIVE"), CancellationToken.None);

        result.Status.ShouldBe(EntityStatus.INACTIVE);
        sut.Services.Services[0].Status.ShouldBe(EntityStatus.INACTIVE);
        sut.UnitWork.CommitCount.ShouldBe(1);
    }

    [Fact]
    public async Task Handle_Deactivate_ShouldKeepNameDescriptionPriceAndCreatedAt()
    {
        var service = Service.Create("ALINHAMENTO", "Com filtro", 80m);
        var sut = CreateSut(service);

        await sut.UseCase.Handle(new UpdateServiceStatusCommand(service.Id.ToString(), "INACTIVE"), CancellationToken.None);

        var stored = sut.Services.Services[0];
        stored.Name.ShouldBe("ALINHAMENTO");
        stored.Description.ShouldBe("Com filtro");
        stored.Price.ShouldBe(80m);
        stored.CreatedAt.ShouldBe(service.CreatedAt);
    }

    [Fact]
    public async Task Handle_InactiveServiceToActive_ShouldActivateAndCommit()
    {
        var service = Service.Create("ALINHAMENTO", null, 80m);
        service.Deactivate();
        var sut = CreateSut(service);

        var result = await sut.UseCase.Handle(new UpdateServiceStatusCommand(service.Id.ToString(), "ACTIVE"), CancellationToken.None);

        result.Status.ShouldBe(EntityStatus.ACTIVE);
        sut.UnitWork.CommitCount.ShouldBe(1);
    }

    [Theory]
    [InlineData("inactive")]
    [InlineData(" Inactive ")]
    public async Task Handle_StatusInAnyCaseOrWithSpaces_ShouldBeAccepted(string status)
    {
        var service = Service.Create("ALINHAMENTO", null, 80m);
        var sut = CreateSut(service);

        var result = await sut.UseCase.Handle(new UpdateServiceStatusCommand(service.Id.ToString(), status), CancellationToken.None);

        result.Status.ShouldBe(EntityStatus.INACTIVE);
    }

    [Theory]
    [InlineData("FOO")]
    [InlineData("DELETED")]
    [InlineData("")]
    public async Task Handle_InvalidStatus_ShouldThrowInvalidServiceStatusExceptionAndKeepService(string status)
    {
        var service = Service.Create("ALINHAMENTO", null, 80m);
        var sut = CreateSut(service);

        var exception = await Should.ThrowAsync<InvalidServiceStatusException>(() =>
            sut.UseCase.Handle(new UpdateServiceStatusCommand(service.Id.ToString(), status), CancellationToken.None));

        exception.ErrorType.ShouldBe("invalid-service-status");
        sut.Services.Services[0].Status.ShouldBe(EntityStatus.ACTIVE);
        sut.UnitWork.CommitCount.ShouldBe(0);
    }

    [Fact]
    public async Task Handle_ActivateAlreadyActive_ShouldThrowServiceAlreadyActiveExceptionAndKeepUpdatedAt()
    {
        var service = Service.Create("ALINHAMENTO", null, 80m);
        var sut = CreateSut(service);

        var exception = await Should.ThrowAsync<ServiceAlreadyActiveException>(() =>
            sut.UseCase.Handle(new UpdateServiceStatusCommand(service.Id.ToString(), "ACTIVE"), CancellationToken.None));

        exception.ErrorType.ShouldBe("service-already-active");
        sut.Services.Services[0].UpdatedAt.ShouldBeNull();
        sut.UnitWork.CommitCount.ShouldBe(0);
    }

    [Fact]
    public async Task Handle_DeactivateAlreadyInactive_ShouldThrowServiceAlreadyInactiveException()
    {
        var service = Service.Create("ALINHAMENTO", null, 80m);
        service.Deactivate();
        var sut = CreateSut(service);

        var exception = await Should.ThrowAsync<ServiceAlreadyInactiveException>(() =>
            sut.UseCase.Handle(new UpdateServiceStatusCommand(service.Id.ToString(), "INACTIVE"), CancellationToken.None));

        exception.ErrorType.ShouldBe("service-already-inactive");
        sut.UnitWork.CommitCount.ShouldBe(0);
    }

    [Theory]
    [InlineData("abc")]
    [InlineData("123")]
    [InlineData("")]
    public async Task Handle_MalformedId_ShouldThrowInvalidServiceIdException(string id)
    {
        var sut = CreateSut();

        var exception = await Should.ThrowAsync<InvalidServiceIdException>(() =>
            sut.UseCase.Handle(new UpdateServiceStatusCommand(id, "INACTIVE"), CancellationToken.None));

        exception.ErrorType.ShouldBe("invalid-service-id");
    }

    [Fact]
    public async Task Handle_UnknownId_ShouldThrowServiceNotFoundException()
    {
        var sut = CreateSut();

        var exception = await Should.ThrowAsync<ServiceNotFoundException>(() =>
            sut.UseCase.Handle(new UpdateServiceStatusCommand(Guid.NewGuid().ToString(), "INACTIVE"), CancellationToken.None));

        exception.ErrorType.ShouldBe("service-not-found");
    }

    private static UpdateServiceStatusUseCaseSut CreateSut(params Service[] services)
    {
        var serviceRepository = new FakeServiceRepository();

        foreach (var service in services)
        {
            serviceRepository.WithService(service);
        }

        var unitWork = new FakeUnitWork();

        var useCase = new UpdateServiceStatusUseCase(serviceRepository, serviceRepository, unitWork, MapperBuilder.Build());

        return new UpdateServiceStatusUseCaseSut(useCase, serviceRepository, unitWork);
    }

    private sealed record UpdateServiceStatusUseCaseSut(
        UpdateServiceStatusUseCase UseCase,
        FakeServiceRepository Services,
        FakeUnitWork UnitWork);
}
