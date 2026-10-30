namespace AutoGarage.Domain.Exceptions.Materials.Exceptions;

using AutoGarage.Domain.Common.Exceptions;

public class MaterialAlreadyActiveException : ConflictDomainException
{
    public MaterialAlreadyActiveException(Guid id)
        : base($"Material with id '{id}' is already active.", "material-already-active")
    {
    }
}
