using AutoGarage.Application.Commands.Customers;
using AutoGarage.Application.Services.Normalization;
using AutoGarage.Application.UseCases.Customers.CreateCustomer;
using AutoGarage.CommonTestUtilities.Mapper;
using AutoGarage.CommonTestUtilities.Repositories;
using AutoGarage.CommonTestUtilities.Repositories.Customers;
using AutoGarage.Domain.Common.Exceptions;
using AutoGarage.Domain.Entities;
using AutoGarage.Domain.Enums.Customers;
using AutoGarage.Domain.Exceptions.Customers.Exceptions;
using AutoGarage.Domain.Shared;
using AutoGarage.Domain.ValueObjects;
using Shouldly;

namespace AutoGarage.Application.UnitTests.UseCases.Customers.CreateCustomer;

public class CreateCustomerUseCaseTests
{
    [Fact]
    public async Task Handle_ValidCommand_ShouldCreateActiveCustomerAndReturnResponse()
    {
        var sut = CreateSut();
        var command = new CreateCustomerCommand("John Smith", "529.982.247-25", "john@autogarage.com", "41", "999999999");

        var result = await sut.UseCase.Handle(command, CancellationToken.None);

        result.Name.ShouldBe("John Smith");
        result.Document.ShouldBe("52998224725");
        result.DocumentType.ShouldBe(DocumentType.CPF);
        result.Email.ShouldBe("JOHN@AUTOGARAGE.COM");
        result.Ddd.ShouldBe("41");
        result.PhoneNumber.ShouldBe("999999999");
        result.Status.ShouldBe(EntityStatus.ACTIVE);
        sut.UnitWork.CommitCount.ShouldBe(1);
    }

    [Fact]
    public async Task Handle_LowercaseAlphanumericCnpj_ShouldStoreUpperCaseAndSetCnpjType()
    {
        var sut = CreateSut();
        var command = new CreateCustomerCommand("Company", "12.abc.345/01de-35", "company@autogarage.com", "41", "933334444");

        var result = await sut.UseCase.Handle(command, CancellationToken.None);

        result.Document.ShouldBe("12ABC34501DE35");
        result.DocumentType.ShouldBe(DocumentType.CNPJ);
    }

    [Fact]
    public async Task Handle_NameAndEmailWithExtraSpaces_ShouldBeNormalized()
    {
        var sut = CreateSut();
        var command = new CreateCustomerCommand("  John    Smith  ", "529.982.247-25", "  john@autogarage.com ", "41", "999999999");

        var result = await sut.UseCase.Handle(command, CancellationToken.None);

        result.Name.ShouldBe("John Smith");
        result.Email.ShouldBe("JOHN@AUTOGARAGE.COM");
    }

    [Fact]
    public async Task Handle_DddAndPhoneWithMask_ShouldKeepOnlyDigits()
    {
        var sut = CreateSut();
        var command = new CreateCustomerCommand("John", "529.982.247-25", "john@autogarage.com", "(41)", "99999-9999");

        var result = await sut.UseCase.Handle(command, CancellationToken.None);

        result.Ddd.ShouldBe("41");
        result.PhoneNumber.ShouldBe("999999999");
    }

    [Theory]
    [InlineData("99999999")]
    [InlineData("9999999999")]
    public async Task Handle_PhoneWithoutNineDigits_ShouldThrowErrorOnValidationException(string phone)
    {
        var sut = CreateSut();
        var command = new CreateCustomerCommand("John", "529.982.247-25", "john@autogarage.com", "41", phone);

        var exception = await Should.ThrowAsync<ErrorOnValidationException>(
            () => sut.UseCase.Handle(command, CancellationToken.None));

        exception.ErrorsMessages.ShouldContain("The phone field must have 9 digits.");
        sut.UnitWork.CommitCount.ShouldBe(0);
    }

    [Theory]
    [InlineData("4")]
    [InlineData("411")]
    public async Task Handle_DddWithoutTwoDigits_ShouldThrowErrorOnValidationException(string ddd)
    {
        var sut = CreateSut();
        var command = new CreateCustomerCommand("John", "529.982.247-25", "john@autogarage.com", ddd, "999999999");

        var exception = await Should.ThrowAsync<ErrorOnValidationException>(
            () => sut.UseCase.Handle(command, CancellationToken.None));

        exception.ErrorsMessages.ShouldContain("The ddd field must have 2 digits.");
        sut.UnitWork.CommitCount.ShouldBe(0);
    }

    [Fact]
    public async Task Handle_DocumentWithDifferentFormattingOrCase_ShouldThrowDuplicateDocument()
    {
        var sut = CreateSut(ExistingCustomer("12ABC34501DE35", "john@autogarage.com"));
        var command = new CreateCustomerCommand("Other", "12.abc.345/01de-35", "other@autogarage.com", "41", "999999999");

        var exception = await Should.ThrowAsync<DuplicateCustomerDocumentException>(
            () => sut.UseCase.Handle(command, CancellationToken.None));

        exception.ErrorType.ShouldBe("duplicate-customer-document");
        sut.UnitWork.CommitCount.ShouldBe(0);
    }

    [Fact]
    public async Task Handle_EmailInDifferentCase_ShouldThrowDuplicateEmail()
    {
        var sut = CreateSut(ExistingCustomer("52998224725", "john@autogarage.com"));
        var command = new CreateCustomerCommand("John", "11222333000181", "  JOHN@autogarage.com ", "41", "999999999");

        var exception = await Should.ThrowAsync<DuplicateCustomerEmailException>(
            () => sut.UseCase.Handle(command, CancellationToken.None));

        exception.ErrorType.ShouldBe("duplicate-customer-email");
    }

    [Fact]
    public async Task Handle_InvalidCpfCheckDigit_ShouldThrowInvalidDocumentException()
    {
        var sut = CreateSut();
        var command = new CreateCustomerCommand("John", "529.982.247-26", "john@autogarage.com", "41", "999999999");

        var exception = await Should.ThrowAsync<InvalidDocumentException>(
            () => sut.UseCase.Handle(command, CancellationToken.None));

        exception.ErrorType.ShouldBe("invalid-document");
        sut.UnitWork.CommitCount.ShouldBe(0);
    }

    [Theory]
    [InlineData("", "529.982.247-25", "john@autogarage.com", "41", "999999999", "The name field is required.")]
    [InlineData("John", "", "john@autogarage.com", "41", "999999999", "The document field is required.")]
    [InlineData("John", "529.982.247-25", "", "41", "999999999", "The email field is required.")]
    [InlineData("John", "529.982.247-25", "invalid-email", "41", "999999999", "The provided email format is invalid.")]
    [InlineData("John", "529.982.247-25", "john@autogarage.com", "", "999999999", "The ddd field is required.")]
    [InlineData("John", "529.982.247-25", "john@autogarage.com", "41", "", "The phone field is required.")]
    public async Task Handle_InvalidBody_ShouldThrowErrorOnValidationException(
        string name, string document, string email, string ddd, string phone, string expectedMessage)
    {
        var sut = CreateSut();
        var command = new CreateCustomerCommand(name, document, email, ddd, phone);

        var exception = await Should.ThrowAsync<ErrorOnValidationException>(
            () => sut.UseCase.Handle(command, CancellationToken.None));

        exception.ErrorsMessages.ShouldContain(expectedMessage);
    }

    private static Customer ExistingCustomer(string document, string email) =>
        Customer.Create("Existing", new Document(document), email, "41", "999999999");

    private static CreateCustomerUseCaseSut CreateSut(params Customer[] customers)
    {
        var repository = new FakeCustomerRepository();

        foreach (var customer in customers)
        {
            repository.WithCustomer(customer);
        }

        var unitWork = new FakeUnitWork();

        var useCase = new CreateCustomerUseCase(
            repository,
            repository,
            unitWork,
            new StringNormalizer(),
            MapperBuilder.Build());

        return new CreateCustomerUseCaseSut(useCase, unitWork);
    }

    private sealed record CreateCustomerUseCaseSut(CreateCustomerUseCase UseCase, FakeUnitWork UnitWork);
}
