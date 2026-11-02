using AutoGarage.Application.Services.Notifications;
using Microsoft.Extensions.Logging;

namespace AutoGarage.Infrastructure.Notifications;

// Used when Settings:Brevo:Enabled is false (local development, tests). Nothing leaves the process.
public class LogOnlyEmailSender(ILogger<LogOnlyEmailSender> logger) : IEmailSender
{
    public Task SendAsync(EmailMessage message, CancellationToken cancellationToken)
    {
        logger.LogInformation("E-mail not sent (Settings:Brevo:Enabled is false) to {ToEmail} with subject {Subject}.", message.ToEmail, message.Subject);

        return Task.CompletedTask;
    }
}
