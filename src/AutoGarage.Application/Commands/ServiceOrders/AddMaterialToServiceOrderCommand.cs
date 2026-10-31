namespace AutoGarage.Application.Commands.ServiceOrders;

public record AddMaterialToServiceOrderCommand(
    string ServiceOrderId,
    string MaterialId,
    int Quantity,
    string EmployeeId
);
