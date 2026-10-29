namespace AutoGarage.Application.Commands.Customers;

public record UpdateCustomerCommand(
    string Id,
    string Name,
    string Email,
    string Ddd,
    string Phone
);
