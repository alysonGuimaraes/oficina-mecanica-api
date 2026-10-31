using AutoGarage.Application.Commands.ServiceOrders;
using AutoGarage.Application.UseCases.ServiceOrders.CreateServiceOrder;
using AutoGarage.Communication.Requests.ServiceOrders;
using AutoGarage.Communication.Responses.ServiceOrders;
using MapsterMapper;
using Microsoft.AspNetCore.Mvc;

namespace AutoGarage.Api.Controllers.ServiceOrders;

[ApiController]
[Route("api/service-orders")]
public class ServiceOrdersController : ControllerBase
{
    private readonly IMapper _mapper;

    public ServiceOrdersController(IMapper mapper)
    {
        _mapper = mapper;
    }

    [HttpPost]
    [ProducesResponseType(typeof(ServiceOrderResponse), StatusCodes.Status201Created)]
    public async Task<ActionResult> Create(
        [FromServices] ICreateServiceOrderUseCase useCase,
        [FromBody] CreateServiceOrderRequest request,
        CancellationToken cancellationToken)
    {
        var command = _mapper.Map<CreateServiceOrderCommand>(request);

        var result = await useCase.Handle(command, cancellationToken);

        return Created(string.Empty, result);
    }
}
