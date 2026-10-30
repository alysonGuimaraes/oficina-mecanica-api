using AutoGarage.Application.Commands.Vehicles;
using AutoGarage.Application.UseCases.Vehicles.CreateVehicle;
using AutoGarage.Application.UseCases.Vehicles.GetVehicleById;
using AutoGarage.Application.UseCases.Vehicles.ListVehicles;
using AutoGarage.Application.UseCases.Vehicles.UpdateVehicle;
using AutoGarage.Communication.Requests.Vehicles;
using AutoGarage.Communication.Responses.Vehicles;
using Microsoft.AspNetCore.Mvc;

namespace AutoGarage.Api.Controllers.Vehicles;

[ApiController]
[Route("api/vehicles")]
public class VehiclesController : ControllerBase
{
    [HttpPost]
    [ProducesResponseType(typeof(VehicleResponse), StatusCodes.Status201Created)]
    public async Task<ActionResult> Create(
        [FromServices] ICreateVehicleUseCase useCase,
        [FromBody] CreateVehicleRequest request,
        CancellationToken cancellationToken)
    {
        var command = new CreateVehicleCommand(request.Brand, request.Model, request.Plate, request.Year, request.VehicleType, request.CustomerId);

        var result = await useCase.Handle(command, cancellationToken);

        return Created(string.Empty, result);
    }

    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<VehicleResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<VehicleResponse>>> List(
        [FromQuery] string? status,
        [FromServices] IListVehiclesUseCase useCase,
        CancellationToken cancellationToken)
    {
        var result = await useCase.Handle(status, cancellationToken);

        return Ok(result);
    }

    [HttpGet("{id}")]
    [ProducesResponseType(typeof(VehicleResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<VehicleResponse>> GetById(
        [FromRoute] string id,
        [FromServices] IGetVehicleByIdUseCase useCase,
        CancellationToken cancellationToken)
    {
        var result = await useCase.Handle(id, cancellationToken);

        return Ok(result);
    }

    [HttpPut("{id}")]
    [ProducesResponseType(typeof(VehicleResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<VehicleResponse>> Update(
        [FromRoute] string id,
        [FromBody] UpdateVehicleRequest request,
        [FromServices] IUpdateVehicleUseCase useCase,
        CancellationToken cancellationToken)
    {
        var command = new UpdateVehicleCommand(id, request.Brand, request.Model, request.Plate, request.Year, request.CustomerId);

        var result = await useCase.Handle(command, cancellationToken);

        return Ok(result);
    }
}
