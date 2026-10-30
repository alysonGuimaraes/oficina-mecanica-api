using AutoGarage.Communication.Responses.Materials;

namespace AutoGarage.Application.UseCases.Materials.GetMaterialById;

public interface IGetMaterialByIdUseCase
{
    Task<MaterialResponse> Handle(string id, CancellationToken cancellationToken);
}
