namespace AutoGarage.Domain.Exceptions.Vehicles.Exceptions;

using AutoGarage.Domain.Common.Exceptions;

public class VehicleAlreadyActiveException : ConflictDomainException
{
    public VehicleAlreadyActiveException(Guid id)
        : base($"Vehicle with id '{id}' is already active.", "vehicle-already-active")
    {
    }
}
