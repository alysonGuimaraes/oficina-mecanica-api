using AutoGarage.Application.Commands.Employees;
using AutoGarage.Application.UseCases.Employees.CreateEmployee;
using AutoGarage.Application.UseCases.Employees.GetEmployeeById;
using AutoGarage.Application.UseCases.Employees.ListEmployees;
using AutoGarage.Application.UseCases.Employees.UpdateEmployee;
using AutoGarage.Communication.Requests.Employees;
using AutoGarage.Communication.Responses.Employees;
using MapsterMapper;
using Microsoft.AspNetCore.Mvc;

namespace AutoGarage.Api.Controllers.Employees;

[ApiController]
[Route("api/employees")]
public class EmployeesController : ControllerBase
{
    private readonly IMapper _mapper;

    public EmployeesController(IMapper mapper)
    {
        _mapper = mapper;
    }

    [HttpPost]
    [ProducesResponseType(typeof(EmployeeResponse), StatusCodes.Status201Created)]
    public async Task<ActionResult> Create(
        [FromServices] ICreateEmployeeUseCase useCase,
        [FromBody] CreateEmployeeRequest request,
        CancellationToken cancellationToken)
    {
        var command = _mapper.Map<CreateEmployeeCommand>(request);

        var result = await useCase.Handle(command, cancellationToken);

        return Created(string.Empty, result);
    }

    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<EmployeeResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<EmployeeResponse>>> GetAll(
        [FromServices] IListEmployeesUseCase useCase,
        CancellationToken cancellationToken)
    {
        var result = await useCase.Handle(cancellationToken);

        return Ok(result);
    }

    [HttpGet("{id}")]
    [ProducesResponseType(typeof(EmployeeResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<EmployeeResponse>> GetById(
        [FromRoute] string id,
        [FromServices] IGetEmployeeByIdUseCase useCase,
        CancellationToken cancellationToken)
    {
        var result = await useCase.Handle(id, cancellationToken);

        return Ok(result);
    }

    [HttpPut("{id}")]
    [ProducesResponseType(typeof(EmployeeResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<EmployeeResponse>> Update(
        [FromRoute] string id,
        [FromBody] UpdateEmployeeRequest request,
        [FromServices] IUpdateEmployeeUseCase useCase,
        CancellationToken cancellationToken)
    {
        var command = new UpdateEmployeeCommand(id, request.Name, request.Email, request.Role);

        var result = await useCase.Handle(command, cancellationToken);

        return Ok(result);
    }
}
