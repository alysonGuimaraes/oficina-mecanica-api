namespace AutoGarage.Domain.Exceptions.MaterialTransactions.Exceptions;

using AutoGarage.Domain.Common.Exceptions;

public class InvalidMaterialTransactionIdException : BadRequestDomainException
{
    public InvalidMaterialTransactionIdException()
        : base("The provided transaction id is invalid.", "invalid-material-transaction-id")
    {
    }
}
