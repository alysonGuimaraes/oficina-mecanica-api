using AutoGarage.Domain.Shared;

namespace AutoGarage.Application.Services.Status;

public static class EntityStatusParser
{
    public static bool TryParse(string? value, out EntityStatus status)
    {
        switch ((value ?? string.Empty).Trim().ToUpperInvariant())
        {
            case "ACTIVE":
                status = EntityStatus.ACTIVE;
                return true;
            case "INACTIVE":
                status = EntityStatus.INACTIVE;
                return true;
            default:
                status = default;
                return false;
        }
    }
}
