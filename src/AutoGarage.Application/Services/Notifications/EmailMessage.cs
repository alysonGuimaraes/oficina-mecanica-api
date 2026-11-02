namespace AutoGarage.Application.Services.Notifications;

public record EmailMessage(string ToEmail, string ToName, string Subject, string PlainTextBody);
