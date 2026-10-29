namespace AutoGarage.Communication.Requests.Customers;

public record CreateCustomerRequest(
    string Name,
    string Document,
    string Email,
    string Ddd,
    string Phone
);
