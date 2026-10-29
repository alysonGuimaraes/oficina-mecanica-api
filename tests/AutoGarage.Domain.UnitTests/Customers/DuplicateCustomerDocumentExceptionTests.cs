using AutoGarage.Domain.Exceptions.Customers.Exceptions;
using Shouldly;

namespace AutoGarage.Domain.UnitTests.Customers;

public class DuplicateCustomerDocumentExceptionTests
{
    [Fact]
    public void Message_Cpf_ShouldShowOnlyFirstTwoDigits()
    {
        var exception = new DuplicateCustomerDocumentException("52998224725");

        exception.Message.ShouldBe("The document '52XXXXXXXXX' is already registered for a customer.");
        exception.ErrorType.ShouldBe("duplicate-customer-document");
    }

    [Fact]
    public void Message_AlphanumericCnpj_ShouldShowOnlyFirstTwoCharacters()
    {
        var exception = new DuplicateCustomerDocumentException("12ABC34501DE35");

        exception.Message.ShouldBe("The document '12XXXXXXXXXXXX' is already registered for a customer.");
    }
}
