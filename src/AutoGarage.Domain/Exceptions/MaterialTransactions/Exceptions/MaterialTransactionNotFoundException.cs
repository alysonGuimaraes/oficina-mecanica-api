namespace AutoGarage.Domain.Exceptions.MaterialTransactions.Exceptions;

using AutoGarage.Domain.Common.Exceptions;

public class MaterialTransactionNotFoundException : NotFoundDomainException
{
    public MaterialTransactionNotFoundException(Guid id)
        : base($"Material transaction with id '{id}' was not found.", "material-transaction-not-found")
    {
    }
}
