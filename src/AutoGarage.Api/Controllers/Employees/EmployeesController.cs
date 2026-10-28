using AutoGarage.Application.Commands.Employees;
using AutoGarage.Application.UseCases.Employees.CreateEmployee;
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
}
