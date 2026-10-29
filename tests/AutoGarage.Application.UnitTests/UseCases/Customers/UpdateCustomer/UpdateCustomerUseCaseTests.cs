using AutoGarage.Application.Commands.Customers;
using AutoGarage.Application.Services.Normalization;
using AutoGarage.Application.UseCases.Customers.UpdateCustomer;
using AutoGarage.CommonTestUtilities.Mapper;
using AutoGarage.CommonTestUtilities.Repositories;
using AutoGarage.CommonTestUtilities.Repositories.Customers;
using AutoGarage.Domain.Common.Exceptions;
using AutoGarage.Domain.Entities;
using AutoGarage.Domain.Exceptions.Customers.Exceptions;
using AutoGarage.Domain.Shared;
using AutoGarage.Domain.ValueObjects;
using Shouldly;

namespace AutoGarage.Application.UnitTests.UseCases.Customers.UpdateCustomer;

public class UpdateCustomerUseCaseTests
{
    [Fact]
    public async Task Handle_ValidCommand_ShouldUpdateProfileAndCommit()
    {
        var customer = CreateCustomer("john@autogarage.com");
        var sut = CreateSut(customer);

        var result = await sut.UseCase.Handle(
            new UpdateCustomerCommand(customer.Id.ToString(), "Jane  Doe", "jane@autogarage.com", "11", "98888-8888"),
            CancellationToken.None);

        result.Name.ShouldBe("Jane Doe");
        result.Email.ShouldBe("JANE@AUTOGARAGE.COM");
        result.Ddd.ShouldBe("11");
        result.PhoneNumber.ShouldBe("988888888");
        result.Document.ShouldBe("52998224725");
        sut.UnitWork.CommitCount.ShouldBe(1);
    }

    [Fact]
    public async Task Handle_DddWithMask_ShouldStoreOnlyDigits()
    {
        var customer = CreateCustomer("john@autogarage.com");
        var sut = CreateSut(customer);

        var result = await sut.UseCase.Handle(
            new UpdateCustomerCommand(customer.Id.ToString(), "Jane", "jane@autogarage.com", "(41)", "999999999"),
            CancellationToken.None);

        result.Ddd.ShouldBe("41");
    }

    [Theory]
    [InlineData("4")]
    [InlineData("411")]
    public async Task Handle_DddWithoutTwoDigits_ShouldThrowErrorOnValidationException(string ddd)
    {
        var customer = CreateCustomer("john@autogarage.com");
        var sut = CreateSut(customer);

        var exception = await Should.ThrowAsync<ErrorOnValidationException>(
            () => sut.UseCase.Handle(
                new UpdateCustomerCommand(customer.Id.ToString(), "Jane", "jane@autogarage.com", ddd, "999999999"),
                CancellationToken.None));

        exception.ErrorsMessages.ShouldContain("The ddd field must have 2 digits.");
        sut.UnitWork.CommitCount.ShouldBe(0);
    }

    [Fact]
    public async Task Handle_SameEmailOfSameCustomerInDifferentCase_ShouldUpdate()
    {
        var customer = CreateCustomer("john@autogarage.com");
        var sut = CreateSut(customer);

        var result = await sut.UseCase.Handle(
            new UpdateCustomerCommand(customer.Id.ToString(), "John", "JOHN@autogarage.com", "41", "999999999"),
            CancellationToken.None);

        result.Email.ShouldBe("JOHN@AUTOGARAGE.COM");
        sut.UnitWork.CommitCount.ShouldBe(1);
    }

    [Fact]
    public async Task Handle_EmailOfAnotherCustomer_ShouldThrowDuplicateCustomerEmailException()
    {
        var customerA = CreateCustomer("john@autogarage.com");
        var customerB = Customer.Create("Company", new Document("12ABC34501DE35"), "company@autogarage.com", "41", "933334444");
        var sut = CreateSut(customerA, customerB);

        var exception = await Should.ThrowAsync<DuplicateCustomerEmailException>(
            () => sut.UseCase.Handle(
                new UpdateCustomerCommand(customerA.Id.ToString(), "John", "company@autogarage.com", "41", "999999999"),
                CancellationToken.None));

        exception.ErrorType.ShouldBe("duplicate-customer-email");
        exception.ShouldBeAssignableTo<ConflictDomainException>();
        sut.UnitWork.CommitCount.ShouldBe(0);
    }

    [Theory]
    [InlineData("9888-88")]
    [InlineData("88888888")]
    public async Task Handle_PhoneWithoutNineDigits_ShouldThrowErrorOnValidationException(string phone)
    {
        var customer = CreateCustomer("john@autogarage.com");
        var sut = CreateSut(customer);

        var exception = await Should.ThrowAsync<ErrorOnValidationException>(
            () => sut.UseCase.Handle(
                new UpdateCustomerCommand(customer.Id.ToString(), "Jane", "jane@autogarage.com", "41", phone),
                CancellationToken.None));

        exception.ErrorsMessages.ShouldContain("The phone field must have 9 digits.");
        sut.UnitWork.CommitCount.ShouldBe(0);
    }

    [Fact]
    public async Task Handle_InactiveCustomer_ShouldUpdateAndKeepStatus()
    {
        var customer = CreateCustomer("john@autogarage.com");
        customer.Deactivate();
        var sut = CreateSut(customer);

        var result = await sut.UseCase.Handle(
            new UpdateCustomerCommand(customer.Id.ToString(), "Jane", "jane@autogarage.com", "41", "999999999"),
            CancellationToken.None);

        result.Status.ShouldBe(EntityStatus.INACTIVE);
        result.Name.ShouldBe("Jane");
    }

    [Fact]
    public async Task Handle_MalformedId_ShouldThrowInvalidCustomerIdException()
    {
        var sut = CreateSut();

        var exception = await Should.ThrowAsync<InvalidCustomerIdException>(
            () => sut.UseCase.Handle(
                new UpdateCustomerCommand("abc", "Jane", "jane@autogarage.com", "41", "999999999"),
                CancellationToken.None));

        exception.ErrorType.ShouldBe("invalid-customer-id");
        sut.UnitWork.CommitCount.ShouldBe(0);
    }

    [Fact]
    public async Task Handle_UnknownId_ShouldThrowCustomerNotFoundException()
    {
        var sut = CreateSut();

        var exception = await Should.ThrowAsync<CustomerNotFoundException>(
            () => sut.UseCase.Handle(
                new UpdateCustomerCommand(Guid.NewGuid().ToString(), "Jane", "jane@autogarage.com", "41", "999999999"),
                CancellationToken.None));

        exception.ErrorType.ShouldBe("customer-not-found");
        sut.UnitWork.CommitCount.ShouldBe(0);
    }

    [Fact]
    public async Task Handle_EmptyName_ShouldThrowErrorOnValidationException()
    {
        var customer = CreateCustomer("john@autogarage.com");
        var sut = CreateSut(customer);

        var exception = await Should.ThrowAsync<ErrorOnValidationException>(
            () => sut.UseCase.Handle(
                new UpdateCustomerCommand(customer.Id.ToString(), "", "jane@autogarage.com", "41", "999999999"),
                CancellationToken.None));

        exception.ErrorsMessages.ShouldContain("The name field is required.");
        sut.UnitWork.CommitCount.ShouldBe(0);
    }

    private static Customer CreateCustomer(string email)
    {
        return Customer.Create("John", new Document("52998224725"), email, "41", "999999999");
    }

    private static UpdateCustomerUseCaseSut CreateSut(params Customer[] customers)
    {
        var repository = new FakeCustomerRepository();

        foreach (var customer in customers)
        {
            repository.WithCustomer(customer);
        }

        var unitWork = new FakeUnitWork();

        var useCase = new UpdateCustomerUseCase(repository, repository, unitWork, new StringNormalizer(), MapperBuilder.Build());

        return new UpdateCustomerUseCaseSut(useCase, unitWork);
    }

    private sealed record UpdateCustomerUseCaseSut(
        UpdateCustomerUseCase UseCase,
        FakeUnitWork UnitWork);
}
