using AutoGarage.Application.Commands.Materials;
using AutoGarage.Communication.Responses.Materials;

namespace AutoGarage.Application.UseCases.Materials.UpdateMaterial;

public interface IUpdateMaterialUseCase
{
    Task<MaterialResponse> Handle(UpdateMaterialCommand command, CancellationToken cancellationToken);
}
