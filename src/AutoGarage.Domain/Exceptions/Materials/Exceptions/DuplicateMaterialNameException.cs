namespace AutoGarage.Domain.Exceptions.Materials.Exceptions;

using AutoGarage.Domain.Common.Exceptions;

public class DuplicateMaterialNameException : ConflictDomainException
{
    public DuplicateMaterialNameException(string name)
        : base($"The material name '{name}' is already registered.", "duplicate-material-name")
    {
    }
}
