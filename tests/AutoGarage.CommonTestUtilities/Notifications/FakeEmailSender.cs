using AutoGarage.Application.Services.Notifications;

namespace AutoGarage.CommonTestUtilities.Notifications;

// In-memory sender: records every message, or fails with ThrowOnSend when configured.
public class FakeEmailSender : IEmailSender
{
    private readonly List<EmailMessage> _sent = [];

    public IReadOnlyList<EmailMessage> Sent => _sent;

    public Exception? ThrowOnSend { get; set; }

    public Task SendAsync(EmailMessage message, CancellationToken cancellationToken)
    {
        if (ThrowOnSend is not null)
        {
            throw ThrowOnSend;
        }

        _sent.Add(message);

        return Task.CompletedTask;
    }
}
