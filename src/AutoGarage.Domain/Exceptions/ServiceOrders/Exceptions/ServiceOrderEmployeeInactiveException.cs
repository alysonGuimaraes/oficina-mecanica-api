namespace AutoGarage.Domain.Exceptions.ServiceOrders.Exceptions;

using AutoGarage.Domain.Common.Exceptions;

public class ServiceOrderEmployeeInactiveException : ConflictDomainException
{
    public ServiceOrderEmployeeInactiveException(Guid employeeId)
        : base($"Employee with id '{employeeId}' is inactive and cannot open service orders.", "service-order-employee-inactive")
    {
    }
}
