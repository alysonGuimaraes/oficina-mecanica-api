using AutoGarage.Application.UseCases.ServiceOrders.ListPublicServiceOrders;
using AutoGarage.Communication.Responses.ServiceOrders;
using Microsoft.AspNetCore.Mvc;

namespace AutoGarage.Api.Controllers.ServiceOrders;

[ApiController]
[Route("public/service-orders")]
public class PublicServiceOrdersController : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<ServiceOrderResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<ServiceOrderResponse>>> List(
        [FromQuery] string? document,
        [FromServices] IListPublicServiceOrdersUseCase useCase,
        CancellationToken cancellationToken)
    {
        var result = await useCase.Handle(document, cancellationToken);

        return Ok(result);
    }
}
