namespace AutoGarage.Application.Commands.ServiceOrders;

public record RemoveMaterialFromServiceOrderCommand(
    string ServiceOrderId,
    string ItemId,
    string EmployeeId
);
