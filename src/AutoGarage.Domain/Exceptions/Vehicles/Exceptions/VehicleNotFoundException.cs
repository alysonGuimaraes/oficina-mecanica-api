namespace AutoGarage.Domain.Exceptions.Vehicles.Exceptions;

using AutoGarage.Domain.Common.Exceptions;

public class VehicleNotFoundException : NotFoundDomainException
{
    public VehicleNotFoundException(Guid id)
        : base($"Vehicle with id '{id}' was not found.", "vehicle-not-found")
    {
    }
}
