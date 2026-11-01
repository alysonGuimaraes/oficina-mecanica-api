using AutoGarage.Application.Commands.ServiceOrders;
using AutoGarage.Application.UseCases.ServiceOrders.ListPublicServiceOrders;
using AutoGarage.Application.UseCases.ServiceOrders.PublicServiceOrderApproval;
using AutoGarage.Communication.Requests.ServiceOrders;
using AutoGarage.Communication.Responses.ServiceOrders;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AutoGarage.Api.Controllers.ServiceOrders;

[ApiController]
[AllowAnonymous]
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

    [HttpPost("{id}/approval")]
    [ProducesResponseType(typeof(ServiceOrderApprovalResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<ServiceOrderApprovalResponse>> Approval(
        [FromRoute] string id,
        [FromBody] PublicServiceOrderApprovalRequest request,
        [FromServices] IPublicServiceOrderApprovalUseCase useCase,
        CancellationToken cancellationToken)
    {
        var command = new PublicServiceOrderApprovalCommand(id, request.Approved);

        var result = await useCase.Handle(command, cancellationToken);

        return Ok(result);
    }
}
