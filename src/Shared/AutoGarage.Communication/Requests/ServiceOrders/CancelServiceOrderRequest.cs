namespace AutoGarage.Communication.Requests.ServiceOrders;

public record CancelServiceOrderRequest(
    string Reason,
    string EmployeeId
);
