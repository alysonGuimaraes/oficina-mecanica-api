using AutoGarage.Communication.Responses.Materials;

namespace AutoGarage.Application.UseCases.Materials.ListLowStockMaterials;

public interface IListLowStockMaterialsUseCase
{
    Task<IReadOnlyList<MaterialResponse>> Handle(string? status, CancellationToken cancellationToken);
}
