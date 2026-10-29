using AutoGarage.Application.Commands.Auth;
using AutoGarage.Communication.Responses.Auth;

namespace AutoGarage.Application.UseCases.Auth.Login;

public interface ILoginUseCase
{
    Task<LoginResponse> Handle(LoginCommand command, CancellationToken cancellationToken);
}
