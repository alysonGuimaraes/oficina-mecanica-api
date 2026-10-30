using AutoGarage.Application.Commands.Materials;
using AutoGarage.Communication.Responses.Materials;

namespace AutoGarage.Application.UseCases.Materials.UpdateMaterialStatus;

public interface IUpdateMaterialStatusUseCase
{
    Task<MaterialResponse> Handle(UpdateMaterialStatusCommand command, CancellationToken cancellationToken);
}
