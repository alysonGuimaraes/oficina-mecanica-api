using AutoGarage.Application.Commands.Services;
using AutoGarage.Application.Services.Normalization;
using AutoGarage.Application.UseCases.Services.CreateService;
using AutoGarage.CommonTestUtilities.Mapper;
using AutoGarage.CommonTestUtilities.Repositories;
using AutoGarage.CommonTestUtilities.Repositories.Services;
using AutoGarage.Domain.Common.Exceptions;
using AutoGarage.Domain.Entities;
using AutoGarage.Domain.Exceptions.Services.Exceptions;
using AutoGarage.Domain.Shared;
using Shouldly;

namespace AutoGarage.Application.UnitTests.UseCases.Services.CreateService;

public class CreateServiceUseCaseTests
{
    [Fact]
    public async Task Handle_ValidCommand_ShouldCreateActiveServiceAndCommit()
    {
        var sut = CreateSut();

        var result = await sut.UseCase.Handle(
            new CreateServiceCommand("  troca de óleo  ", "  com filtro  ", 150.50m),
            CancellationToken.None);

        result.Name.ShouldBe("TROCA DE ÓLEO");
        result.Description.ShouldBe("com filtro");
        result.Price.ShouldBe(150.50m);
        result.Status.ShouldBe(EntityStatus.ACTIVE);
        sut.Services.Services.Count.ShouldBe(1);
        sut.UnitWork.CommitCount.ShouldBe(1);
    }

    [Fact]
    public async Task Handle_BlankDescription_ShouldStoreNull()
    {
        var sut = CreateSut();

        var result = await sut.UseCase.Handle(
            new CreateServiceCommand("ALINHAMENTO", "   ", 80m),
            CancellationToken.None);

        result.Description.ShouldBeNull();
    }

    [Fact]
    public async Task Handle_NameAlreadyRegisteredIgnoringCaseAndSpacing_ShouldThrowDuplicateServiceNameException()
    {
        var sut = CreateSut(Service.Create("TROCA DE ÓLEO", null, 150m));

        await Should.ThrowAsync<DuplicateServiceNameException>(() => sut.UseCase.Handle(
            new CreateServiceCommand("troca  de óleo", null, 120m),
            CancellationToken.None));

        sut.Services.Services.Count.ShouldBe(1);
        sut.UnitWork.CommitCount.ShouldBe(0);
    }

    [Fact]
    public async Task Handle_InvalidPrice_ShouldThrowErrorOnValidationExceptionWithoutQueryingRepository()
    {
        var sut = CreateSut();

        await Should.ThrowAsync<ErrorOnValidationException>(() => sut.UseCase.Handle(
            new CreateServiceCommand("ALINHAMENTO", null, 0m),
            CancellationToken.None));

        sut.Services.Services.Count.ShouldBe(0);
    }

    private static CreateServiceUseCaseSut CreateSut(params Service[] services)
    {
        var serviceRepository = new FakeServiceRepository();

        foreach (var service in services)
        {
            serviceRepository.WithService(service);
        }

        var unitWork = new FakeUnitWork();

        var useCase = new CreateServiceUseCase(serviceRepository, serviceRepository, unitWork, new StringNormalizer(), MapperBuilder.Build());

        return new CreateServiceUseCaseSut(useCase, serviceRepository, unitWork);
    }

    private sealed record CreateServiceUseCaseSut(
        CreateServiceUseCase UseCase,
        FakeServiceRepository Services,
        FakeUnitWork UnitWork);
}
