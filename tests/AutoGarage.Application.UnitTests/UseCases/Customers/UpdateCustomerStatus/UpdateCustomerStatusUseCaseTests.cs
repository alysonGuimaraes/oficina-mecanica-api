using AutoGarage.Application.Commands.Customers;
using AutoGarage.Application.UseCases.Customers.UpdateCustomerStatus;
using AutoGarage.CommonTestUtilities.Mapper;
using AutoGarage.CommonTestUtilities.Repositories;
using AutoGarage.CommonTestUtilities.Repositories.Customers;
using AutoGarage.Domain.Common.Exceptions;
using AutoGarage.Domain.Entities;
using AutoGarage.Domain.Exceptions.Customers.Exceptions;
using AutoGarage.Domain.Shared;
using AutoGarage.Domain.ValueObjects;
using Shouldly;

namespace AutoGarage.Application.UnitTests.UseCases.Customers.UpdateCustomerStatus;

public class UpdateCustomerStatusUseCaseTests
{
    [Fact]
    public async Task Handle_ActiveCustomerToInactive_ShouldDeactivate()
    {
        var customer = CreateCustomer();
        var sut = CreateSut(customer);

        var result = await sut.UseCase.Handle(new UpdateCustomerStatusCommand(customer.Id.ToString(), "INACTIVE"), CancellationToken.None);

        result.Status.ShouldBe(EntityStatus.INACTIVE);
        sut.UnitWork.CommitCount.ShouldBe(1);
    }

    [Fact]
    public async Task Handle_InactiveCustomerToActive_ShouldActivate()
    {
        var customer = CreateCustomer();
        customer.Deactivate();
        var sut = CreateSut(customer);

        var result = await sut.UseCase.Handle(new UpdateCustomerStatusCommand(customer.Id.ToString(), "ACTIVE"), CancellationToken.None);

        result.Status.ShouldBe(EntityStatus.ACTIVE);
        sut.UnitWork.CommitCount.ShouldBe(1);
    }

    [Fact]
    public async Task Handle_AlreadyActive_ShouldThrowCustomerAlreadyActiveException()
    {
        var customer = CreateCustomer();
        var sut = CreateSut(customer);

        var exception = await Should.ThrowAsync<CustomerAlreadyActiveException>(
            () => sut.UseCase.Handle(new UpdateCustomerStatusCommand(customer.Id.ToString(), "ACTIVE"), CancellationToken.None));

        exception.ErrorType.ShouldBe("customer-already-active");
        exception.ShouldBeAssignableTo<ConflictDomainException>();
        sut.UnitWork.CommitCount.ShouldBe(0);
    }

    [Fact]
    public async Task Handle_AlreadyInactive_ShouldThrowCustomerAlreadyInactiveException()
    {
        var customer = CreateCustomer();
        customer.Deactivate();
        var sut = CreateSut(customer);

        var exception = await Should.ThrowAsync<CustomerAlreadyInactiveException>(
            () => sut.UseCase.Handle(new UpdateCustomerStatusCommand(customer.Id.ToString(), "INACTIVE"), CancellationToken.None));

        exception.ErrorType.ShouldBe("customer-already-inactive");
        sut.UnitWork.CommitCount.ShouldBe(0);
    }

    [Theory]
    [InlineData("inactive")]
    [InlineData("  INACTIVE  ")]
    public async Task Handle_StatusCaseInsensitiveWithSpaces_ShouldDeactivate(string status)
    {
        var customer = CreateCustomer();
        var sut = CreateSut(customer);

        var result = await sut.UseCase.Handle(new UpdateCustomerStatusCommand(customer.Id.ToString(), status), CancellationToken.None);

        result.Status.ShouldBe(EntityStatus.INACTIVE);
    }

    [Theory]
    [InlineData("ACTIVEE")]
    [InlineData("0")]
    [InlineData("1")]
    [InlineData("")]
    public async Task Handle_InvalidStatus_ShouldThrowInvalidCustomerStatusException(string status)
    {
        var customer = CreateCustomer();
        var sut = CreateSut(customer);

        var exception = await Should.ThrowAsync<InvalidCustomerStatusException>(
            () => sut.UseCase.Handle(new UpdateCustomerStatusCommand(customer.Id.ToString(), status), CancellationToken.None));

        exception.ErrorType.ShouldBe("invalid-customer-status");
        exception.ShouldBeAssignableTo<BadRequestDomainException>();
        sut.UnitWork.CommitCount.ShouldBe(0);
    }

    [Theory]
    [InlineData("abc")]
    [InlineData("")]
    public async Task Handle_MalformedId_ShouldThrowInvalidCustomerIdException(string id)
    {
        var sut = CreateSut(CreateCustomer());

        var exception = await Should.ThrowAsync<InvalidCustomerIdException>(
            () => sut.UseCase.Handle(new UpdateCustomerStatusCommand(id, "INACTIVE"), CancellationToken.None));

        exception.ErrorType.ShouldBe("invalid-customer-id");
    }

    [Fact]
    public async Task Handle_UnknownId_ShouldThrowCustomerNotFoundException()
    {
        var sut = CreateSut();

        var exception = await Should.ThrowAsync<CustomerNotFoundException>(
            () => sut.UseCase.Handle(new UpdateCustomerStatusCommand(Guid.NewGuid().ToString(), "INACTIVE"), CancellationToken.None));

        exception.ErrorType.ShouldBe("customer-not-found");
    }

    private static Customer CreateCustomer()
    {
        return Customer.Create("John", new Document("52998224725"), "john@autogarage.com", "41", "999999999");
    }

    private static UpdateCustomerStatusUseCaseSut CreateSut(params Customer[] customers)
    {
        var repository = new FakeCustomerRepository();

        foreach (var customer in customers)
        {
            repository.WithCustomer(customer);
        }

        var unitWork = new FakeUnitWork();

        var useCase = new UpdateCustomerStatusUseCase(repository, repository, unitWork, MapperBuilder.Build());

        return new UpdateCustomerStatusUseCaseSut(useCase, unitWork);
    }

    private sealed record UpdateCustomerStatusUseCaseSut(
        UpdateCustomerStatusUseCase UseCase,
        FakeUnitWork UnitWork);
}
