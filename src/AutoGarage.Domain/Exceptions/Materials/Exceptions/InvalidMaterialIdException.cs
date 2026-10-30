namespace AutoGarage.Domain.Exceptions.Materials.Exceptions;

using AutoGarage.Domain.Common.Exceptions;

public class InvalidMaterialIdException : BadRequestDomainException
{
    public InvalidMaterialIdException()
        : base("The provided id is invalid.", "invalid-material-id")
    {
    }
}
