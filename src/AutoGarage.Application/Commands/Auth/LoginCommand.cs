namespace AutoGarage.Application.Commands.Auth;

public record LoginCommand(
    string Email,
    string Password
);
