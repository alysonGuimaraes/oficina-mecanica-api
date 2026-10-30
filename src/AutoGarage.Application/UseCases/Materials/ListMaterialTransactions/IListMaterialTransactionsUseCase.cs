using AutoGarage.Communication.Responses.Materials;

namespace AutoGarage.Application.UseCases.Materials.ListMaterialTransactions;

public interface IListMaterialTransactionsUseCase
{
    Task<IReadOnlyList<MaterialTransactionResponse>> Handle(string materialId, string? type, CancellationToken cancellationToken);
}
