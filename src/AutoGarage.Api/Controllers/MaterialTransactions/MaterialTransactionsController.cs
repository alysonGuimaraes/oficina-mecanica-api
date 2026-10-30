using AutoGarage.Application.UseCases.MaterialTransactions.GetMaterialTransactionById;
using AutoGarage.Application.UseCases.MaterialTransactions.ListAllMaterialTransactions;
using AutoGarage.Communication.Responses.Materials;
using Microsoft.AspNetCore.Mvc;

namespace AutoGarage.Api.Controllers.MaterialTransactions;

[ApiController]
[Route("api/material-transactions")]
public class MaterialTransactionsController : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<MaterialTransactionResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<MaterialTransactionResponse>>> List(
        [FromQuery] string? type,
        [FromServices] IListAllMaterialTransactionsUseCase useCase,
        CancellationToken cancellationToken)
    {
        var result = await useCase.Handle(type, cancellationToken);

        return Ok(result);
    }

    [HttpGet("{id}")]
    [ProducesResponseType(typeof(MaterialTransactionResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<MaterialTransactionResponse>> GetById(
        [FromRoute] string id,
        [FromServices] IGetMaterialTransactionByIdUseCase useCase,
        CancellationToken cancellationToken)
    {
        var result = await useCase.Handle(id, cancellationToken);

        return Ok(result);
    }
}
