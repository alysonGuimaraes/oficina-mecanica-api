namespace AutoGarage.Communication.Responses.Auth;

public record LoginResponse(
    string Token,
    DateTime ExpiresAt
);
