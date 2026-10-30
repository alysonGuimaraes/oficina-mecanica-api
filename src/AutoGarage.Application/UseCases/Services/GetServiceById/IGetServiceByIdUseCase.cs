using AutoGarage.Communication.Responses.Services;

namespace AutoGarage.Application.UseCases.Services.GetServiceById;

public interface IGetServiceByIdUseCase
{
    Task<ServiceResponse> Handle(string id, CancellationToken cancellationToken);
}
