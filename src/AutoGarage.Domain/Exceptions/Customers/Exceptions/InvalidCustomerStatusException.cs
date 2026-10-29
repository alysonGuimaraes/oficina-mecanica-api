namespace AutoGarage.Domain.Exceptions.Customers.Exceptions;

using AutoGarage.Domain.Common.Exceptions;

public class InvalidCustomerStatusException : BadRequestDomainException
{
    public InvalidCustomerStatusException()
        : base("The provided status is invalid. Allowed values are ACTIVE or INACTIVE.", "invalid-customer-status")
    {
    }
}
