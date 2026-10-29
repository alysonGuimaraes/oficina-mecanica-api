using AutoGarage.Application.Commands.Auth;
using AutoGarage.Application.UseCases.Auth.Login;
using AutoGarage.Communication.Requests.Auth;
using AutoGarage.Communication.Responses.Auth;
using Microsoft.AspNetCore.Mvc;

namespace AutoGarage.Api.Controllers.Auth;

[ApiController]
[Route("auth")]
public class AuthController : ControllerBase
{
    [HttpPost("login")]
    [ProducesResponseType(typeof(LoginResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<LoginResponse>> Login(
        [FromBody] LoginRequest request,
        [FromServices] ILoginUseCase useCase,
        CancellationToken cancellationToken)
    {
        var command = new LoginCommand(request.Email, request.Password);

        var result = await useCase.Handle(command, cancellationToken);

        return Ok(result);
    }
}
