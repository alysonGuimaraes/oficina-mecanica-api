namespace AutoGarage.Domain.Exceptions.Materials.Exceptions;

using AutoGarage.Domain.Common.Exceptions;

public class InvalidMaterialTransactionTypeException : BadRequestDomainException
{
    public InvalidMaterialTransactionTypeException()
        : base("The provided type is invalid. Allowed values are IN or OUT.", "invalid-material-transaction-type")
    {
    }
}
