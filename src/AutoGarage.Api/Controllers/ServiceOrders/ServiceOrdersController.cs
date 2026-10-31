using AutoGarage.Application.Commands.ServiceOrders;
using AutoGarage.Application.UseCases.ServiceOrders.CreateServiceOrder;
using AutoGarage.Application.UseCases.ServiceOrders.DeliverServiceOrder;
using AutoGarage.Application.UseCases.ServiceOrders.DiagnoseServiceOrder;
using AutoGarage.Application.UseCases.ServiceOrders.GetServiceOrderById;
using AutoGarage.Application.UseCases.ServiceOrders.ListServiceOrders;
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

    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<ServiceOrderResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<ServiceOrderResponse>>> List(
        [FromQuery] string? status,
        [FromServices] IListServiceOrdersUseCase useCase,
        CancellationToken cancellationToken)
    {
        var result = await useCase.Handle(status, cancellationToken);

        return Ok(result);
    }

    [HttpGet("{id}")]
    [ProducesResponseType(typeof(ServiceOrderDetailResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<ServiceOrderDetailResponse>> GetById(
        [FromRoute] string id,
        [FromServices] IGetServiceOrderByIdUseCase useCase,
        CancellationToken cancellationToken)
    {
        var result = await useCase.Handle(id, cancellationToken);

        return Ok(result);
    }

    [HttpPost("{id}/diagnosis")]
    [ProducesResponseType(typeof(ServiceOrderResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<ServiceOrderResponse>> Diagnosis(
        [FromRoute] string id,
        [FromBody] ServiceOrderEmployeeRequest request,
        [FromServices] IDiagnoseServiceOrderUseCase useCase,
        CancellationToken cancellationToken)
    {
        var command = new ServiceOrderEmployeeCommand(id, request.EmployeeId);

        var result = await useCase.Handle(command, cancellationToken);

        return Ok(result);
    }

    [HttpPost("{id}/delivery")]
    [ProducesResponseType(typeof(ServiceOrderResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<ServiceOrderResponse>> Delivery(
        [FromRoute] string id,
        [FromBody] ServiceOrderEmployeeRequest request,
        [FromServices] IDeliverServiceOrderUseCase useCase,
        CancellationToken cancellationToken)
    {
        var command = new ServiceOrderEmployeeCommand(id, request.EmployeeId);

        var result = await useCase.Handle(command, cancellationToken);

        return Ok(result);
    }
}
