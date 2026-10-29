namespace AutoGarage.Domain.Exceptions.Customers.Exceptions;

using AutoGarage.Domain.Common.Exceptions;

public class InvalidCustomerIdException : BadRequestDomainException
{
    public InvalidCustomerIdException()
        : base("The provided id is invalid.", "invalid-customer-id")
    {
    }
}
