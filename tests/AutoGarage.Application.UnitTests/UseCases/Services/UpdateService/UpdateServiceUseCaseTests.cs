using AutoGarage.Application.Commands.Services;
using AutoGarage.Application.Services.Normalization;
using AutoGarage.Application.UseCases.Services.UpdateService;
using AutoGarage.CommonTestUtilities.Mapper;
using AutoGarage.CommonTestUtilities.Repositories;
using AutoGarage.CommonTestUtilities.Repositories.Services;
using AutoGarage.Domain.Common.Exceptions;
using AutoGarage.Domain.Entities;
using AutoGarage.Domain.Exceptions.Services.Exceptions;
using AutoGarage.Domain.Shared;
using Shouldly;

namespace AutoGarage.Application.UnitTests.UseCases.Services.UpdateService;

public class UpdateServiceUseCaseTests
{
    [Fact]
    public async Task Handle_ValidCommand_ShouldUpdateServiceAndCommit()
    {
        var service = Service.Create("ALINHAMENTO", null, 80m);
        var sut = CreateSut(service);

        var result = await sut.UseCase.Handle(
            new UpdateServiceCommand(service.Id.ToString(), "  troca  de óleo  ", "  com filtro  ", 150.50m),
            CancellationToken.None);

        result.Name.ShouldBe("TROCA DE ÓLEO");
        result.Description.ShouldBe("com filtro");
        result.Price.ShouldBe(150.50m);
        result.Status.ShouldBe(EntityStatus.ACTIVE);
        sut.Services.Services[0].Name.ShouldBe("TROCA DE ÓLEO");
        sut.UnitWork.CommitCount.ShouldBe(1);
    }

    [Fact]
    public async Task Handle_SameNameOfSameService_ShouldUpdateWithoutConflict()
    {
        var service = Service.Create("ALINHAMENTO", null, 80m);
        var sut = CreateSut(service);

        var result = await sut.UseCase.Handle(
            new UpdateServiceCommand(service.Id.ToString(), "ALINHAMENTO", "Nova", 90m),
            CancellationToken.None);

        result.Price.ShouldBe(90m);
        sut.UnitWork.CommitCount.ShouldBe(1);
    }

    [Fact]
    public async Task Handle_NameOfAnotherService_ShouldThrowDuplicateServiceNameException()
    {
        var service = Service.Create("ALINHAMENTO", null, 80m);
        var other = Service.Create("TROCA DE ÓLEO", null, 150m);
        var sut = CreateSut(service, other);

        await Should.ThrowAsync<DuplicateServiceNameException>(() => sut.UseCase.Handle(
            new UpdateServiceCommand(service.Id.ToString(), "troca  de óleo", "Com filtro", 80m),
            CancellationToken.None));

        sut.Services.Services[0].Name.ShouldBe("ALINHAMENTO");
        sut.UnitWork.CommitCount.ShouldBe(0);
    }

    [Fact]
    public async Task Handle_BlankDescription_ShouldStoreNull()
    {
        var service = Service.Create("ALINHAMENTO", "Antiga", 80m);
        var sut = CreateSut(service);

        var result = await sut.UseCase.Handle(
            new UpdateServiceCommand(service.Id.ToString(), "ALINHAMENTO", "   ", 80m),
            CancellationToken.None);

        result.Description.ShouldBeNull();
    }

    [Fact]
    public async Task Handle_InactiveService_ShouldUpdateAndKeepInactiveStatus()
    {
        var service = Service.Create("ALINHAMENTO", null, 80m);
        service.Deactivate();
        var sut = CreateSut(service);

        var result = await sut.UseCase.Handle(
            new UpdateServiceCommand(service.Id.ToString(), "ALINHAMENTO", "Com filtro", 90m),
            CancellationToken.None);

        result.Status.ShouldBe(EntityStatus.INACTIVE);
        result.Price.ShouldBe(90m);
    }

    [Theory]
    [InlineData("abc")]
    [InlineData("123")]
    [InlineData("")]
    public async Task Handle_MalformedId_ShouldThrowInvalidServiceIdException(string id)
    {
        var sut = CreateSut();

        var exception = await Should.ThrowAsync<InvalidServiceIdException>(() => sut.UseCase.Handle(
            new UpdateServiceCommand(id, "ALINHAMENTO", "Com filtro", 80m),
            CancellationToken.None));

        exception.ErrorType.ShouldBe("invalid-service-id");
    }

    [Fact]
    public async Task Handle_UnknownId_ShouldThrowServiceNotFoundException()
    {
        var sut = CreateSut();

        var exception = await Should.ThrowAsync<ServiceNotFoundException>(() => sut.UseCase.Handle(
            new UpdateServiceCommand(Guid.NewGuid().ToString(), "ALINHAMENTO", "Com filtro", 80m),
            CancellationToken.None));

        exception.ErrorType.ShouldBe("service-not-found");
    }

    [Fact]
    public async Task Handle_InvalidPrice_ShouldThrowErrorOnValidationExceptionAndKeepService()
    {
        var service = Service.Create("ALINHAMENTO", null, 80m);
        var sut = CreateSut(service);

        await Should.ThrowAsync<ErrorOnValidationException>(() => sut.UseCase.Handle(
            new UpdateServiceCommand(service.Id.ToString(), "ALINHAMENTO", null, 0m),
            CancellationToken.None));

        sut.Services.Services[0].Price.ShouldBe(80m);
        sut.UnitWork.CommitCount.ShouldBe(0);
    }

    [Fact]
    public async Task Handle_NullDescription_ShouldThrowErrorOnValidationExceptionAndKeepService()
    {
        var service = Service.Create("ALINHAMENTO", "Antiga", 80m);
        var sut = CreateSut(service);

        await Should.ThrowAsync<ErrorOnValidationException>(() => sut.UseCase.Handle(
            new UpdateServiceCommand(service.Id.ToString(), "ALINHAMENTO", null, 80m),
            CancellationToken.None));

        sut.Services.Services[0].Description.ShouldBe("Antiga");
        sut.UnitWork.CommitCount.ShouldBe(0);
    }

    private static UpdateServiceUseCaseSut CreateSut(params Service[] services)
    {
        var serviceRepository = new FakeServiceRepository();

        foreach (var service in services)
        {
            serviceRepository.WithService(service);
        }

        var unitWork = new FakeUnitWork();

        var useCase = new UpdateServiceUseCase(serviceRepository, serviceRepository, unitWork, new StringNormalizer(), MapperBuilder.Build());

        return new UpdateServiceUseCaseSut(useCase, serviceRepository, unitWork);
    }

    private sealed record UpdateServiceUseCaseSut(
        UpdateServiceUseCase UseCase,
        FakeServiceRepository Services,
        FakeUnitWork UnitWork);
}
