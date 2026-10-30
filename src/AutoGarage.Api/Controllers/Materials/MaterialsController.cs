using AutoGarage.Application.Commands.Materials;
using AutoGarage.Application.UseCases.Materials.CreateMaterial;
using AutoGarage.Application.UseCases.Materials.GetMaterialById;
using AutoGarage.Application.UseCases.Materials.ListMaterials;
using AutoGarage.Communication.Requests.Materials;
using AutoGarage.Communication.Responses.Materials;
using MapsterMapper;
using Microsoft.AspNetCore.Mvc;

namespace AutoGarage.Api.Controllers.Materials;

[ApiController]
[Route("api/materials")]
public class MaterialsController : ControllerBase
{
    private readonly IMapper _mapper;

    public MaterialsController(IMapper mapper)
    {
        _mapper = mapper;
    }

    [HttpPost]
    [ProducesResponseType(typeof(MaterialResponse), StatusCodes.Status201Created)]
    public async Task<ActionResult> Create(
        [FromServices] ICreateMaterialUseCase useCase,
        [FromBody] CreateMaterialRequest request,
        CancellationToken cancellationToken)
    {
        var command = _mapper.Map<CreateMaterialCommand>(request);

        var result = await useCase.Handle(command, cancellationToken);

        return Created(string.Empty, result);
    }

    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<MaterialResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<MaterialResponse>>> List(
        [FromQuery] string? status,
        [FromServices] IListMaterialsUseCase useCase,
        CancellationToken cancellationToken)
    {
        var result = await useCase.Handle(status, cancellationToken);

        return Ok(result);
    }

    [HttpGet("{id}")]
    [ProducesResponseType(typeof(MaterialResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<MaterialResponse>> GetById(
        [FromRoute] string id,
        [FromServices] IGetMaterialByIdUseCase useCase,
        CancellationToken cancellationToken)
    {
        var result = await useCase.Handle(id, cancellationToken);

        return Ok(result);
    }
}
