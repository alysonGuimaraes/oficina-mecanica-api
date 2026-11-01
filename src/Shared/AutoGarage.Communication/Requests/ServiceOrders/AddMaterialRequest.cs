namespace AutoGarage.Communication.Requests.ServiceOrders;

public record AddMaterialRequest(
    string MaterialId,
    int Quantity,
    string EmployeeId
);
