namespace AutoGarage.Infrastructure.Notifications;

// Carries only the HTTP status. The provider body and the API key are never part of the message.
public class EmailSendException(int statusCode) : Exception($"Brevo returned status {statusCode}.")
{
    public int StatusCode { get; } = statusCode;
}
