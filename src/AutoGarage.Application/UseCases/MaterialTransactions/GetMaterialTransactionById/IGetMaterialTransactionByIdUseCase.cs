using AutoGarage.Communication.Responses.Materials;

namespace AutoGarage.Application.UseCases.MaterialTransactions.GetMaterialTransactionById;

public interface IGetMaterialTransactionByIdUseCase
{
    Task<MaterialTransactionResponse> Handle(string id, CancellationToken cancellationToken);
}
