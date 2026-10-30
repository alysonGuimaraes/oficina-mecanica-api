namespace AutoGarage.Domain.Exceptions.Vehicles.Exceptions;

using AutoGarage.Domain.Common.Exceptions;

public class VehicleAlreadyInactiveException : ConflictDomainException
{
    public VehicleAlreadyInactiveException(Guid id)
        : base($"Vehicle with id '{id}' is already inactive.", "vehicle-already-inactive")
    {
    }
}
