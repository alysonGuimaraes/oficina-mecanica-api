namespace AutoGarage.Application.Commands.ServiceOrders;

public record ServiceOrderEmployeeCommand(
    string ServiceOrderId,
    string EmployeeId
);
