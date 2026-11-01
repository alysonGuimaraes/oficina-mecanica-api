namespace AutoGarage.Application.Commands.ServiceOrders;

public record CreateServiceOrderCommand(
    string CustomerId,
    string VehicleId,
    string EmployeeId
);
