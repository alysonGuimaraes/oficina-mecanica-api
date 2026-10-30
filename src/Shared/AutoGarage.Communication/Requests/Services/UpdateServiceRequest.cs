namespace AutoGarage.Communication.Requests.Services;

public record UpdateServiceRequest(
    string Name,
    string? Description,
    decimal Price
);
