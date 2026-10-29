namespace AutoGarage.Domain.Common;

public static class DocumentMask
{
    // Mostra apenas os dois primeiros caracteres; o restante vira 'X', mantendo o tamanho.
    public static string Mask(string value) =>
        value.Length <= 2 ? value : value[..2] + new string('X', value.Length - 2);
}
