namespace AutoGarage.Domain.Exceptions.Materials.Exceptions;

using AutoGarage.Domain.Common.Exceptions;

public class MaterialAlreadyInactiveException : ConflictDomainException
{
    public MaterialAlreadyInactiveException(Guid id)
        : base($"Material with id '{id}' is already inactive.", "material-already-inactive")
    {
    }
}
