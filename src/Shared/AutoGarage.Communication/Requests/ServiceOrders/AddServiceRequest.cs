namespace AutoGarage.Communication.Requests.ServiceOrders;

public record AddServiceRequest(
    string ServiceId,
    string EmployeeId
);
