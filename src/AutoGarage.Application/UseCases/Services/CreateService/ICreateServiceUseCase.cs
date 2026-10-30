using AutoGarage.Application.Commands.Services;
using AutoGarage.Communication.Responses.Services;

namespace AutoGarage.Application.UseCases.Services.CreateService;

public interface ICreateServiceUseCase
{
    Task<ServiceResponse> Handle(CreateServiceCommand command, CancellationToken cancellationToken);
}
