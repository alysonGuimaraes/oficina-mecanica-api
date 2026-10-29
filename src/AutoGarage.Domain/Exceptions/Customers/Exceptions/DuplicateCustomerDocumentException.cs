namespace AutoGarage.Domain.Exceptions.Customers.Exceptions;

using AutoGarage.Domain.Common.Exceptions;

public class DuplicateCustomerDocumentException : ConflictDomainException
{
    public DuplicateCustomerDocumentException(string value)
        : base($"The document '{Mask(value)}' is already registered for a customer.", "duplicate-customer-document")
    {
    }

    // Mostra apenas os dois primeiros caracteres; o restante vira 'X', mantendo o tamanho.
    private static string Mask(string value) =>
        value.Length <= 2 ? value : value[..2] + new string('X', value.Length - 2);
}
