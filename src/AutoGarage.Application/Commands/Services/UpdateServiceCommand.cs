namespace AutoGarage.Application.Commands.Services;

public record UpdateServiceCommand(
    string Id,
    string Name,
    string? Description,
    decimal Price
);
