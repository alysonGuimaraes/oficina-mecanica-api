namespace AutoGarage.Infrastructure.Notifications;

// Settings:Brevo. ApiKey comes from a GitHub secret; FromEmail and FromName from GitHub variables.
public class BrevoSettings
{
    public string ApiKey { get; init; } = string.Empty;

    public string FromEmail { get; init; } = string.Empty;

    public string FromName { get; init; } = string.Empty;

    public bool Enabled { get; init; }
}
