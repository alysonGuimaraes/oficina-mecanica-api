namespace AutoGarage.Application.Commands.ServiceOrders;

public record CompleteServiceOrderCommand(
    string ServiceOrderId,
    string Observation,
    string EmployeeId
);
