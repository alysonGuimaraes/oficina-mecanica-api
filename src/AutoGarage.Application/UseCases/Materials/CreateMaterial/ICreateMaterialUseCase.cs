using AutoGarage.Application.Commands.Materials;
using AutoGarage.Communication.Responses.Materials;

namespace AutoGarage.Application.UseCases.Materials.CreateMaterial;

public interface ICreateMaterialUseCase
{
    Task<MaterialResponse> Handle(CreateMaterialCommand command, CancellationToken cancellationToken);
}
