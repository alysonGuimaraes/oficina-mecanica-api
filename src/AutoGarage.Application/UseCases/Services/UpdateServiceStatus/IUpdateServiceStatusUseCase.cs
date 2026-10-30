using AutoGarage.Application.Commands.Services;
using AutoGarage.Communication.Responses.Services;

namespace AutoGarage.Application.UseCases.Services.UpdateServiceStatus;

public interface IUpdateServiceStatusUseCase
{
    Task<ServiceResponse> Handle(UpdateServiceStatusCommand command, CancellationToken cancellationToken);
}
