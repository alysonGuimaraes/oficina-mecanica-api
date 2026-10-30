using AutoGarage.Application.Commands.Materials;
using AutoGarage.Communication.Responses.Materials;

namespace AutoGarage.Application.UseCases.Materials.StockOut;

public interface IStockOutUseCase
{
    Task<MaterialResponse> Handle(StockOutCommand command, CancellationToken cancellationToken);
}
