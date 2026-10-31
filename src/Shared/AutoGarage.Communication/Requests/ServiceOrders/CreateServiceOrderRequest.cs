namespace AutoGarage.Communication.Requests.ServiceOrders;

public record CreateServiceOrderRequest(
    string CustomerId,
    string VehicleId,
    string EmployeeId
);
