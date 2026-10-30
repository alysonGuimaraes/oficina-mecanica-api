namespace AutoGarage.Domain.Exceptions.Vehicles.Exceptions;

using AutoGarage.Domain.Common.Exceptions;

public class VehicleOwnerInactiveException : ConflictDomainException
{
    public VehicleOwnerInactiveException(Guid customerId)
        : base($"Customer with id '{customerId}' is inactive and cannot own vehicles.", "vehicle-owner-inactive")
    {
    }
}
