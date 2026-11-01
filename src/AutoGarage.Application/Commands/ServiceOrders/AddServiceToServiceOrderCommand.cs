namespace AutoGarage.Application.Commands.ServiceOrders;

public record AddServiceToServiceOrderCommand(
    string ServiceOrderId,
    string ServiceId,
    string EmployeeId
);
