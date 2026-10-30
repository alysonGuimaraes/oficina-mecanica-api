using System.Text.RegularExpressions;

namespace AutoGarage.Domain.Common;

public static class AlphanumericText
{
    private static readonly Regex NonAlphanumeric = new("[^A-Za-z0-9]", RegexOptions.Compiled);

    // Mantém só letras e dígitos, em maiúsculas. Usado pelos Value Objects que aceitam máscara.
    public static string Hygienize(string? value) =>
        NonAlphanumeric.Replace(value ?? string.Empty, string.Empty).ToUpperInvariant();
}
