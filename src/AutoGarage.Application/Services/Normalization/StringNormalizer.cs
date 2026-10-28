using System.Text.RegularExpressions;

namespace AutoGarage.Application.Services.Normalization;

public class StringNormalizer : IStringNormalizer
{
    private static readonly Regex MultipleWhitespace = new(@"\s+", RegexOptions.Compiled);

    public string Normalize(string? value) =>
        MultipleWhitespace.Replace(value ?? string.Empty, " ").Trim();
}
