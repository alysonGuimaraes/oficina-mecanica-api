using AutoGarage.Communication.Responses.Services;

namespace AutoGarage.Application.UseCases.Services.ListServices;

public interface IListServicesUseCase
{
    Task<IReadOnlyList<ServiceResponse>> Handle(string? status, CancellationToken cancellationToken);
}
