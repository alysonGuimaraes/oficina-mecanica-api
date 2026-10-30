using AutoGarage.Communication.Responses.Materials;

namespace AutoGarage.Application.UseCases.MaterialTransactions.ListAllMaterialTransactions;

public interface IListAllMaterialTransactionsUseCase
{
    Task<IReadOnlyList<MaterialTransactionResponse>> Handle(string? type, CancellationToken cancellationToken);
}
