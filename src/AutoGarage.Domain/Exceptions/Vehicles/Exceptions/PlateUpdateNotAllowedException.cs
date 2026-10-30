namespace AutoGarage.Domain.Exceptions.Vehicles.Exceptions;

using AutoGarage.Domain.Common.Exceptions;

public class PlateUpdateNotAllowedException : ConflictDomainException
{
    public PlateUpdateNotAllowedException()
        : base("The plate of a vehicle with a Mercosul plate cannot be updated.", "plate-update-not-allowed")
    {
    }
}
