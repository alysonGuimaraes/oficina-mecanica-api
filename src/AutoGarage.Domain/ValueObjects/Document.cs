using AutoGarage.Domain.Common;
using AutoGarage.Domain.Enums.Customers;
using AutoGarage.Domain.Exceptions.Customers.Exceptions;

namespace AutoGarage.Domain.ValueObjects;

public sealed record Document
{
    private static readonly int[] CpfWeights1 = [10, 9, 8, 7, 6, 5, 4, 3, 2];
    private static readonly int[] CpfWeights2 = [11, 10, 9, 8, 7, 6, 5, 4, 3, 2];
    private static readonly int[] CnpjWeights1 = [5, 4, 3, 2, 9, 8, 7, 6, 5, 4, 3, 2];
    private static readonly int[] CnpjWeights2 = [6, 5, 4, 3, 2, 9, 8, 7, 6, 5, 4, 3, 2];

    public Document(string value)
    {
        var hygienized = AlphanumericText.Hygienize(value);

        Type = hygienized.Length switch
        {
            11 => DocumentType.CPF,
            14 => DocumentType.CNPJ,
            _ => throw new InvalidDocumentException()
        };

        if (!IsValid(hygienized, Type))
        {
            throw new InvalidDocumentException();
        }

        Value = hygienized;
    }

    public string Value { get; }

    public DocumentType Type { get; }

    private static bool IsValid(string value, DocumentType type)
    {
        if (value.Distinct().Count() == 1)
        {
            return false;
        }

        var digits = value.Select(c => c - '0').ToArray();

        return type == DocumentType.CPF ? IsValidCpf(value, digits) : IsValidCnpj(value, digits);
    }

    private static bool IsValidCpf(string value, int[] digits)
    {
        if (!value.All(char.IsDigit))
        {
            return false;
        }

        return Checksum(digits[..9], CpfWeights1) == digits[9]
            && Checksum(digits[..10], CpfWeights2) == digits[10];
    }

    private static bool IsValidCnpj(string value, int[] values)
    {
        if (!value[12..].All(char.IsDigit))
        {
            return false;
        }

        var firstCheckDigit = Checksum(values[..12], CnpjWeights1);
        if (values[12] != firstCheckDigit)
        {
            return false;
        }

        var secondCheckDigit = Checksum(values[..13], CnpjWeights2);
        return values[13] == secondCheckDigit;
    }

    private static int Checksum(int[] values, int[] weights)
    {
        var sum = 0;
        for (var i = 0; i < weights.Length; i++)
        {
            sum += values[i] * weights[i];
        }

        var remainder = sum % 11;
        return remainder < 2 ? 0 : 11 - remainder;
    }
}
