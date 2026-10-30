namespace AutoGarage.Application.Commands.Materials;

public record UpdateMaterialStatusCommand(
    string Id,
    string Status
);
