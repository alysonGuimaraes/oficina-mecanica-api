using AutoGarage.Application.UseCases.Customers.GetCustomerById;
using AutoGarage.CommonTestUtilities.Mapper;
using AutoGarage.CommonTestUtilities.Repositories.Customers;
using AutoGarage.Domain.Common.Exceptions;
using AutoGarage.Domain.Entities;
using AutoGarage.Domain.Exceptions.Customers.Exceptions;
using AutoGarage.Domain.Shared;
using AutoGarage.Domain.ValueObjects;
using Shouldly;

namespace AutoGarage.Application.UnitTests.UseCases.Customers.GetCustomerById;

public class GetCustomerByIdUseCaseTests
{
    [Fact]
    public async Task Handle_ExistingCustomer_ShouldReturnResponse()
    {
        var customer = CreateCustomer("john@autogarage.com");
        var sut = CreateSut(customer);

        var result = await sut.UseCase.Handle(customer.Id.ToString(), CancellationToken.None);

        result.Id.ShouldBe(customer.Id);
        result.Document.ShouldBe("52998224725");
        result.Status.ShouldBe(EntityStatus.ACTIVE);
    }

    [Fact]
    public async Task Handle_InactiveCustomer_ShouldReturnWithInactiveStatus()
    {
        var customer = CreateCustomer("inactive@autogarage.com");
        customer.Deactivate();
        var sut = CreateSut(customer);

        var result = await sut.UseCase.Handle(customer.Id.ToString(), CancellationToken.None);

        result.Status.ShouldBe(EntityStatus.INACTIVE);
    }

    [Fact]
    public async Task Handle_UnknownId_ShouldThrowCustomerNotFoundException()
    {
        var sut = CreateSut();
        var id = Guid.NewGuid();

        var exception = await Should.ThrowAsync<CustomerNotFoundException>(
            () => sut.UseCase.Handle(id.ToString(), CancellationToken.None));

        exception.ErrorType.ShouldBe("customer-not-found");
        exception.Message.ShouldBe($"Customer with id '{id}' was not found.");
    }

    [Fact]
    public async Task Handle_EmptyGuid_ShouldThrowCustomerNotFoundException()
    {
        var sut = CreateSut();

        var exception = await Should.ThrowAsync<CustomerNotFoundException>(
            () => sut.UseCase.Handle(Guid.Empty.ToString(), CancellationToken.None));

        exception.ErrorType.ShouldBe("customer-not-found");
    }

    [Theory]
    [InlineData("abc")]
    [InlineData("")]
    [InlineData("12345")]
    public async Task Handle_MalformedId_ShouldThrowInvalidCustomerIdException(string id)
    {
        var sut = CreateSut();

        var exception = await Should.ThrowAsync<InvalidCustomerIdException>(
            () => sut.UseCase.Handle(id, CancellationToken.None));

        exception.ErrorType.ShouldBe("invalid-customer-id");
        exception.Message.ShouldBe("The provided id is invalid.");
        exception.ShouldBeAssignableTo<BadRequestDomainException>();
    }

    private static Customer CreateCustomer(string email)
    {
        return Customer.Create("John", new Document("52998224725"), email, "41", "999999999");
    }

    private static GetCustomerByIdUseCaseSut CreateSut(params Customer[] customers)
    {
        var repository = new FakeCustomerRepository();

        foreach (var customer in customers)
        {
            repository.WithCustomer(customer);
        }

        var useCase = new GetCustomerByIdUseCase(repository, MapperBuilder.Build());

        return new GetCustomerByIdUseCaseSut(useCase);
    }

    private sealed record GetCustomerByIdUseCaseSut(GetCustomerByIdUseCase UseCase);
}
