using AutoGarage.Application.Services.Notifications;
using AutoGarage.Infrastructure.Notifications;
using Microsoft.Extensions.Logging.Abstractions;
using Shouldly;
using System.Net;
using System.Text.Json;

namespace AutoGarage.IntegrationTests.Notifications;

public class BrevoEmailSenderTests
{
    private static readonly BrevoSettings Settings = new()
    {
        ApiKey = "test-key",
        FromEmail = "oficina@autogarage.local",
        FromName = "AutoGarage",
        Enabled = true
    };

    [Fact]
    public async Task SendAsync_ShouldPostJsonWithApiKeyHeaderAndRecipient()
    {
        var handler = new CapturingHandler(HttpStatusCode.Created, "{}");
        var sut = new BrevoEmailSender(new HttpClient(handler), Settings, NullLogger<BrevoEmailSender>.Instance);
        var message = new EmailMessage("ana@autogarage.local", "Ana", "Ordem de serviço: recebida", "Olá, Ana.");

        await sut.SendAsync(message, CancellationToken.None);

        var request = handler.Request!;
        request.Method.ShouldBe(HttpMethod.Post);
        request.RequestUri!.ToString().ShouldBe("https://api.brevo.com/v3/smtp/email");
        request.Headers.GetValues("api-key").Single().ShouldBe("test-key");

        using var body = JsonDocument.Parse(handler.Body!);
        body.RootElement.GetProperty("to")[0].GetProperty("email").GetString().ShouldBe("ana@autogarage.local");
        body.RootElement.GetProperty("subject").GetString().ShouldBe("Ordem de serviço: recebida");
    }

    [Fact]
    public async Task SendAsync_NonSuccessStatus_ShouldNotExposeApiKey()
    {
        var handler = new CapturingHandler(HttpStatusCode.Unauthorized, "invalid api key test-key");
        var sut = new BrevoEmailSender(new HttpClient(handler), Settings, NullLogger<BrevoEmailSender>.Instance);
        var message = new EmailMessage("ana@autogarage.local", "Ana", "Ordem de serviço: recebida", "Olá, Ana.");

        var exception = await Should.ThrowAsync<EmailSendException>(() => sut.SendAsync(message, CancellationToken.None));

        exception.Message.ShouldContain("401");
        exception.Message.ShouldNotContain("test-key");
    }

    private sealed class CapturingHandler(HttpStatusCode statusCode, string responseBody) : HttpMessageHandler
    {
        public HttpRequestMessage? Request { get; private set; }

        public string? Body { get; private set; }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            Request = request;
            Body = request.Content is null ? null : await request.Content.ReadAsStringAsync(cancellationToken);

            return new HttpResponseMessage(statusCode) { Content = new StringContent(responseBody) };
        }
    }
}
