namespace AutoGarage.Communication.Requests.Services;

public record CreateServiceRequest(
    string Name,
    string? Description,
    decimal Price
);
