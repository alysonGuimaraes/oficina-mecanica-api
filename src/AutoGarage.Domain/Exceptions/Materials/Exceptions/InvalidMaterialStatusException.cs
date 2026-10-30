namespace AutoGarage.Domain.Exceptions.Materials.Exceptions;

using AutoGarage.Domain.Common.Exceptions;

public class InvalidMaterialStatusException : BadRequestDomainException
{
    public InvalidMaterialStatusException()
        : base("The provided status is invalid. Allowed values are ACTIVE or INACTIVE.", "invalid-material-status")
    {
    }
}
