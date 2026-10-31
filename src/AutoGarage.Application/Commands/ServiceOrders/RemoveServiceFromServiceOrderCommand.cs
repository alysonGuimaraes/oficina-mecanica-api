namespace AutoGarage.Application.Commands.ServiceOrders;

public record RemoveServiceFromServiceOrderCommand(
    string ServiceOrderId,
    string ItemId,
    string EmployeeId
);
