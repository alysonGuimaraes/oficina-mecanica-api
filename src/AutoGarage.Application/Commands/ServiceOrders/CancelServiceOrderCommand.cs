namespace AutoGarage.Application.Commands.ServiceOrders;

public record CancelServiceOrderCommand(
    string ServiceOrderId,
    string Reason,
    string EmployeeId
);
