namespace AutoGarage.Domain.Exceptions.Materials.Exceptions;

using AutoGarage.Domain.Common.Exceptions;

public class InsufficientStockException : ConflictDomainException
{
    public InsufficientStockException(Guid id)
        : base($"Material with id '{id}' does not have enough stock for this operation.", "insufficient-stock")
    {
    }
}
