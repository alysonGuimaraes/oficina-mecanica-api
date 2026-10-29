namespace AutoGarage.Domain.Exceptions.Customers.Exceptions;

using AutoGarage.Domain.Common.Exceptions;

public class CustomerDocumentNotFoundException : NotFoundDomainException
{
    public CustomerDocumentNotFoundException(string document)
        : base($"Customer with document '{Mask(document)}' was not found.", "customer-document-not-found")
    {
    }

    // Mostra apenas os dois primeiros caracteres; o restante vira 'X', mantendo o tamanho.
    private static string Mask(string value) =>
        value.Length <= 2 ? value : value[..2] + new string('X', value.Length - 2);
}
