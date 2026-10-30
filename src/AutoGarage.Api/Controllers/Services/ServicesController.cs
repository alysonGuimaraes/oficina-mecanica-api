using AutoGarage.Application.Commands.Services;
using AutoGarage.Application.UseCases.Services.CreateService;
using AutoGarage.Application.UseCases.Services.GetServiceById;
using AutoGarage.Application.UseCases.Services.ListServices;
using AutoGarage.Application.UseCases.Services.UpdateService;
using AutoGarage.Application.UseCases.Services.UpdateServiceStatus;
using AutoGarage.Communication.Requests.Services;
using AutoGarage.Communication.Responses.Services;
using MapsterMapper;
using Microsoft.AspNetCore.Mvc;

namespace AutoGarage.Api.Controllers.Services;

[ApiController]
[Route("api/services")]
public class ServicesController : ControllerBase
{
    private readonly IMapper _mapper;

    public ServicesController(IMapper mapper)
    {
        _mapper = mapper;
    }

    [HttpPost]
    [ProducesResponseType(typeof(ServiceResponse), StatusCodes.Status201Created)]
    public async Task<ActionResult> Create(
        [FromServices] ICreateServiceUseCase useCase,
        [FromBody] CreateServiceRequest request,
        CancellationToken cancellationToken)
    {
        var command = _mapper.Map<CreateServiceCommand>(request);

        var result = await useCase.Handle(command, cancellationToken);

        return Created(string.Empty, result);
    }

    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<ServiceResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<ServiceResponse>>> List(
        [FromQuery] string? status,
        [FromServices] IListServicesUseCase useCase,
        CancellationToken cancellationToken)
    {
        var result = await useCase.Handle(status, cancellationToken);

        return Ok(result);
    }

    [HttpGet("{id}")]
    [ProducesResponseType(typeof(ServiceResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<ServiceResponse>> GetById(
        [FromRoute] string id,
        [FromServices] IGetServiceByIdUseCase useCase,
        CancellationToken cancellationToken)
    {
        var result = await useCase.Handle(id, cancellationToken);

        return Ok(result);
    }

    [HttpPut("{id}")]
    [ProducesResponseType(typeof(ServiceResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<ServiceResponse>> Update(
        [FromRoute] string id,
        [FromBody] UpdateServiceRequest request,
        [FromServices] IUpdateServiceUseCase useCase,
        CancellationToken cancellationToken)
    {
        var command = new UpdateServiceCommand(id, request.Name, request.Description, request.Price);

        var result = await useCase.Handle(command, cancellationToken);

        return Ok(result);
    }

    [HttpPatch("{id}/status")]
    [ProducesResponseType(typeof(ServiceResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<ServiceResponse>> UpdateStatus(
        [FromRoute] string id,
        [FromBody] UpdateServiceStatusRequest request,
        [FromServices] IUpdateServiceStatusUseCase useCase,
        CancellationToken cancellationToken)
    {
        var command = new UpdateServiceStatusCommand(id, request.Status);

        var result = await useCase.Handle(command, cancellationToken);

        return Ok(result);
    }
}
