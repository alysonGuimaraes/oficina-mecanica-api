namespace AutoGarage.Application.Commands.Customers;

public record UpdateCustomerStatusCommand(
    string Id,
    string Status
);
