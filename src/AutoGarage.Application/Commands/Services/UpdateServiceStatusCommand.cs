namespace AutoGarage.Application.Commands.Services;

public record UpdateServiceStatusCommand(
    string Id,
    string Status
);
