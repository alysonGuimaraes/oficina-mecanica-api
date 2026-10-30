using AutoGarage.Domain.Enums.Materials;

namespace AutoGarage.Application.Services.Transactions;

public static class TransactionTypeParser
{
    public static bool TryParse(string? value, out TransactionType type)
    {
        switch ((value ?? string.Empty).Trim().ToUpperInvariant())
        {
            case "IN":
                type = TransactionType.IN;
                return true;
            case "OUT":
                type = TransactionType.OUT;
                return true;
            default:
                type = default;
                return false;
        }
    }
}
