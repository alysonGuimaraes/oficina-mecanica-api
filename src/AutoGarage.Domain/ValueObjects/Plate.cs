using AutoGarage.Domain.Common;
using AutoGarage.Domain.Enums.Vehicles;
using AutoGarage.Domain.Exceptions.Vehicles.Exceptions;
using System.Text.RegularExpressions;

namespace AutoGarage.Domain.ValueObjects;

public sealed record Plate
{
    private static readonly Regex OldFormat = new("^[A-Z]{3}[0-9]{4}$", RegexOptions.Compiled);

    private static readonly Regex MercosulFormat = new("^[A-Z]{3}[0-9][A-Z][0-9]{2}$", RegexOptions.Compiled);

    public Plate(string value)
    {
        var hygienized = AlphanumericText.Hygienize(value);

        if (OldFormat.IsMatch(hygienized))
        {
            Type = PlateType.OLD;
        }
        else if (MercosulFormat.IsMatch(hygienized))
        {
            Type = PlateType.MERCOSUL;
        }
        else
        {
            throw new InvalidPlateException();
        }

        Value = hygienized;
    }

    public string Value { get; }

    public PlateType Type { get; }
}
