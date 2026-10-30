using AutoGarage.Application.Commands.Materials;
using AutoGarage.Communication.Responses.Materials;

namespace AutoGarage.Application.UseCases.Materials.StockEntry;

public interface IStockEntryUseCase
{
    Task<MaterialResponse> Handle(StockEntryCommand command, CancellationToken cancellationToken);
}
