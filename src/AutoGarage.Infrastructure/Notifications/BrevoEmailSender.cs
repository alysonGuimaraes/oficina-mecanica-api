using AutoGarage.Application.Services.Notifications;
using Microsoft.Extensions.Logging;
using System.Net.Http.Json;

namespace AutoGarage.Infrastructure.Notifications;

public class BrevoEmailSender : IEmailSender
{
    private const string SendEmailUrl = "https://api.brevo.com/v3/smtp/email";

    private readonly HttpClient _http;
    private readonly BrevoSettings _settings;
    private readonly ILogger<BrevoEmailSender> _logger;

    public BrevoEmailSender(HttpClient http, BrevoSettings settings, ILogger<BrevoEmailSender> logger)
    {
        _http = http;
        _settings = settings;
        _logger = logger;
    }

    public async Task SendAsync(EmailMessage message, CancellationToken cancellationToken)
    {
        using var request = new HttpRequestMessage(HttpMethod.Post, SendEmailUrl)
        {
            Content = JsonContent.Create(new
            {
                to = new[] { new { email = message.ToEmail, name = message.ToName } },
                sender = new { email = _settings.FromEmail, name = _settings.FromName },
                subject = message.Subject,
                textContent = message.PlainTextBody
            })
        };
        request.Headers.Add("api-key", _settings.ApiKey);

        using var response = await _http.SendAsync(request, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            throw new EmailSendException((int)response.StatusCode);
        }

        _logger.LogInformation("Brevo accepted e-mail with subject {Subject}.", message.Subject);
    }
}
