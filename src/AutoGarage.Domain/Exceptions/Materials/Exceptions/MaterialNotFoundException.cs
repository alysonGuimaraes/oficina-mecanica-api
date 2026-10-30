namespace AutoGarage.Domain.Exceptions.Materials.Exceptions;

using AutoGarage.Domain.Common.Exceptions;

public class MaterialNotFoundException : NotFoundDomainException
{
    public MaterialNotFoundException(Guid id)
        : base($"Material with id '{id}' was not found.", "material-not-found")
    {
    }
}
