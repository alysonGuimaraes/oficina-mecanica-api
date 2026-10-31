namespace AutoGarage.Communication.Requests.ServiceOrders;

public record RemoveMaterialRequest(
    string ItemId,
    string EmployeeId
);
