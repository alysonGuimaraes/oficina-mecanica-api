using AutoGarage.Application.Services.Cryptography;

namespace AutoGarage.CommonTestUtilities.Cryptography;

public class PasswordEncripterBuilder
{
    public static PasswordEncripter Build() => new("test-key");
}
