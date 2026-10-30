using AutoGarage.Application.Commands.Services;
using AutoGarage.Application.UseCases.Services.CreateService;
using AutoGarage.Application.UseCases.Services.ListServices;
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
}
