using AutoGarage.Domain.Enums.Customers;
using AutoGarage.Domain.Exceptions.Customers.Exceptions;
using AutoGarage.Domain.ValueObjects;
using Shouldly;

namespace AutoGarage.Domain.UnitTests.Customers;

public class DocumentTests
{
    [Theory]
    [InlineData("529.982.247-25", "52998224725")]
    [InlineData("__52998224725__", "52998224725")]
    [InlineData("52998224725", "52998224725")]
    public void Constructor_ValidCpf_ShouldHygienizeAndSetCpfType(string input, string expected)
    {
        var document = new Document(input);

        document.Value.ShouldBe(expected);
        document.Type.ShouldBe(DocumentType.CPF);
    }

    [Fact]
    public void Constructor_ValidNumericCnpj_ShouldSetCnpjType()
    {
        var document = new Document("11.222.333/0001-81");

        document.Value.ShouldBe("11222333000181");
        document.Type.ShouldBe(DocumentType.CNPJ);
    }

    [Fact]
    public void Constructor_ValidAlphanumericCnpjInUpperCase_ShouldKeepValueAndSetCnpjType()
    {
        var document = new Document("12.ABC.345/01DE-35");

        document.Value.ShouldBe("12ABC34501DE35");
        document.Type.ShouldBe(DocumentType.CNPJ);
    }

    [Fact]
    public void Constructor_ValidAlphanumericCnpjInLowerCase_ShouldUppercaseValue()
    {
        var document = new Document("12.abc.345/01de-35");

        document.Value.ShouldBe("12ABC34501DE35");
        document.Type.ShouldBe(DocumentType.CNPJ);
    }

    [Theory]
    [InlineData("529.982.247-26")]
    [InlineData("111.111.111-11")]
    [InlineData("00000000000000")]
    [InlineData("12ABC34501DE36")]
    [InlineData("123")]
    [InlineData("")]
    public void Constructor_InvalidDocument_ShouldThrowInvalidDocumentException(string input)
    {
        var exception = Should.Throw<InvalidDocumentException>(() => new Document(input));

        exception.ErrorType.ShouldBe("invalid-document");
        exception.Message.ShouldBe("The provided document is invalid.");
    }
}
