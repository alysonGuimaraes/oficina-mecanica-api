using AutoGarage.Domain.Enums.ServiceOrders;

namespace AutoGarage.Application.Services.ServiceOrders;

public static class ServiceOrderStatusParser
{
    public static bool TryParse(string? value, out ServiceOrderStatus status)
    {
        var text = (value ?? string.Empty).Trim();

        foreach (var name in Enum.GetNames<ServiceOrderStatus>())
        {
            if (string.Equals(name, text, StringComparison.OrdinalIgnoreCase))
            {
                status = Enum.Parse<ServiceOrderStatus>(name);
                return true;
            }
        }

        status = default;
        return false;
    }
}
