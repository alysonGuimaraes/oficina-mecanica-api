using AutoGarage.Application.UseCases.Customers.GetCustomerByDocument;
using AutoGarage.CommonTestUtilities.Mapper;
using AutoGarage.CommonTestUtilities.Repositories.Customers;
using AutoGarage.Domain.Common.Exceptions;
using AutoGarage.Domain.Entities;
using AutoGarage.Domain.Enums.Customers;
using AutoGarage.Domain.Exceptions.Customers.Exceptions;
using AutoGarage.Domain.Shared;
using AutoGarage.Domain.ValueObjects;
using Shouldly;

namespace AutoGarage.Application.UnitTests.UseCases.Customers.GetCustomerByDocument;

public class GetCustomerByDocumentUseCaseTests
{
    [Theory]
    [InlineData("529.982.247-25")]
    [InlineData("52998224725")]
    public async Task Handle_CpfFormattedOrNot_ShouldReturnSameCustomer(string document)
    {
        var sut = CreateSut(CreateCpfCustomer("john@autogarage.com"));

        var result = await sut.UseCase.Handle(document, CancellationToken.None);

        result.Document.ShouldBe("52998224725");
        result.DocumentType.ShouldBe(DocumentType.CPF);
    }

    [Theory]
    [InlineData("12ABC34501DE35")]
    [InlineData("12.abc.345/01de-35")]
    public async Task Handle_CnpjInAnyFormattingOrCase_ShouldReturnCustomer(string document)
    {
        var sut = CreateSut(CreateCnpjCustomer());

        var result = await sut.UseCase.Handle(document, CancellationToken.None);

        result.Document.ShouldBe("12ABC34501DE35");
        result.DocumentType.ShouldBe(DocumentType.CNPJ);
    }

    [Fact]
    public async Task Handle_InactiveCustomer_ShouldReturnWithInactiveStatus()
    {
        var customer = CreateCpfCustomer("inactive@autogarage.com");
        customer.Deactivate();
        var sut = CreateSut(customer);

        var result = await sut.UseCase.Handle("52998224725", CancellationToken.None);

        result.Status.ShouldBe(EntityStatus.INACTIVE);
    }

    [Fact]
    public async Task Handle_InvalidCpfCheckDigit_ShouldThrowInvalidDocumentException()
    {
        var sut = CreateSut();

        var exception = await Should.ThrowAsync<InvalidDocumentException>(
            () => sut.UseCase.Handle("529.982.247-26", CancellationToken.None));

        exception.ErrorType.ShouldBe("invalid-document");
        exception.ShouldBeAssignableTo<BadRequestDomainException>();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task Handle_MissingDocument_ShouldThrowErrorOnValidationException(string? document)
    {
        var sut = CreateSut();

        var exception = await Should.ThrowAsync<ErrorOnValidationException>(
            () => sut.UseCase.Handle(document, CancellationToken.None));

        exception.ErrorsMessages.ShouldContain("The document field is required.");
    }

    [Fact]
    public async Task Handle_ValidDocumentNotRegistered_ShouldThrowCustomerDocumentNotFoundException()
    {
        var sut = CreateSut();

        var exception = await Should.ThrowAsync<CustomerDocumentNotFoundException>(
            () => sut.UseCase.Handle("11222333000181", CancellationToken.None));

        exception.ErrorType.ShouldBe("customer-document-not-found");
        exception.Message.ShouldBe("Customer with document '11XXXXXXXXXXXX' was not found.");
        exception.ShouldBeAssignableTo<NotFoundDomainException>();
    }

    private static Customer CreateCpfCustomer(string email)
    {
        return Customer.Create("John", new Document("52998224725"), email, "41", "999999999");
    }

    private static Customer CreateCnpjCustomer()
    {
        return Customer.Create("Company", new Document("12ABC34501DE35"), "company@autogarage.com", "41", "933334444");
    }

    private static GetCustomerByDocumentUseCaseSut CreateSut(params Customer[] customers)
    {
        var repository = new FakeCustomerRepository();

        foreach (var customer in customers)
        {
            repository.WithCustomer(customer);
        }

        var useCase = new GetCustomerByDocumentUseCase(repository, MapperBuilder.Build());

        return new GetCustomerByDocumentUseCaseSut(useCase);
    }

    private sealed record GetCustomerByDocumentUseCaseSut(GetCustomerByDocumentUseCase UseCase);
}
