namespace AutoGarage.Communication.Requests.Customers;

public record UpdateCustomerRequest(
    string Name,
    string Email,
    string Ddd,
    string Phone
);
