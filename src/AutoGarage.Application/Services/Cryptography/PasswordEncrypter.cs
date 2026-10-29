using System.Security.Cryptography;
using System.Text;

namespace AutoGarage.Application.Services.Cryptography;

public class PasswordEncripter(String additionalKey)
{
    private readonly string _additionalKey = additionalKey;

    public string Encrypt(string password)
    {
        var newPassword = $"{password}{_additionalKey}";

        var bytes = Encoding.UTF8.GetBytes(newPassword);
        var hashBytes = SHA512.HashData(bytes);

        return StringBytes(hashBytes);
    }

    public bool Verify(string password, string passwordHash)
    {
        var computedHash = Encoding.UTF8.GetBytes(Encrypt(password));
        var storedHash = Encoding.UTF8.GetBytes(passwordHash);

        return CryptographicOperations.FixedTimeEquals(computedHash, storedHash);
    }

    private static string StringBytes(byte[] bytes)
    {
        var sb = new StringBuilder();
        foreach (byte b in bytes)
        {
            var hex = b.ToString("x2");
            sb.Append(hex);
        }

        return sb.ToString();
    }
}
