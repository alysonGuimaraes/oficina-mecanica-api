namespace AutoGarage.Communication.Requests.ServiceOrders;

public record RemoveServiceRequest(
    string ItemId,
    string EmployeeId
);
