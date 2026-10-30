using AutoGarage.Communication.Responses.Materials;

namespace AutoGarage.Application.UseCases.Materials.ListMaterials;

public interface IListMaterialsUseCase
{
    Task<IReadOnlyList<MaterialResponse>> Handle(string? status, CancellationToken cancellationToken);
}
