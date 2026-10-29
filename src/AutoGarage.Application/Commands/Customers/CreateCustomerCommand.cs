namespace AutoGarage.Application.Commands.Customers;

public record CreateCustomerCommand(
    string Name,
    string Document,
    string Email,
    string Ddd,
    string Phone
);
