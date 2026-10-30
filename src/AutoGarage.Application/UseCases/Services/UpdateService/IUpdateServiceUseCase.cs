using AutoGarage.Application.Commands.Services;
using AutoGarage.Communication.Responses.Services;

namespace AutoGarage.Application.UseCases.Services.UpdateService;

public interface IUpdateServiceUseCase
{
    Task<ServiceResponse> Handle(UpdateServiceCommand command, CancellationToken cancellationToken);
}
