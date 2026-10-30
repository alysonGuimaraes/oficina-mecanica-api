namespace AutoGarage.Application.Commands.Services;

public record CreateServiceCommand(
    string Name,
    string? Description,
    decimal Price
);
