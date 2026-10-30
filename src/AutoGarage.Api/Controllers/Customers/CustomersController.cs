using AutoGarage.Application.Commands.Customers;
using AutoGarage.Application.UseCases.Customers.CreateCustomer;
using AutoGarage.Application.UseCases.Customers.GetCustomerByDocument;
using AutoGarage.Application.UseCases.Customers.GetCustomerById;
using AutoGarage.Application.UseCases.Customers.ListCustomers;
using AutoGarage.Application.UseCases.Customers.UpdateCustomer;
using AutoGarage.Application.UseCases.Customers.UpdateCustomerStatus;
using AutoGarage.Communication.Requests.Customers;
using AutoGarage.Communication.Responses.Customers;
using MapsterMapper;
using Microsoft.AspNetCore.Mvc;

namespace AutoGarage.Api.Controllers.Customers;

[ApiController]
[Route("api/customers")]
public class CustomersController : ControllerBase
{
    private readonly IMapper _mapper;

    public CustomersController(IMapper mapper)
    {
        _mapper = mapper;
    }

    [HttpPost]
    [ProducesResponseType(typeof(CustomerResponse), StatusCodes.Status201Created)]
    public async Task<ActionResult> Create(
        [FromServices] ICreateCustomerUseCase useCase,
        [FromBody] CreateCustomerRequest request,
        CancellationToken cancellationToken)
    {
        var command = _mapper.Map<CreateCustomerCommand>(request);

        var result = await useCase.Handle(command, cancellationToken);

        return Created(string.Empty, result);
    }

    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<CustomerResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<CustomerResponse>>> List(
        [FromQuery] string? status,
        [FromServices] IListCustomersUseCase useCase,
        CancellationToken cancellationToken)
    {
        var result = await useCase.Handle(status, cancellationToken);

        return Ok(result);
    }

    [HttpGet("document")]
    [ProducesResponseType(typeof(CustomerResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<CustomerResponse>> GetByDocument(
        [FromQuery] string? document,
        [FromServices] IGetCustomerByDocumentUseCase useCase,
        CancellationToken cancellationToken)
    {
        var result = await useCase.Handle(document, cancellationToken);

        return Ok(result);
    }

    [HttpGet("{id}")]
    [ProducesResponseType(typeof(CustomerResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<CustomerResponse>> GetById(
        [FromRoute] string id,
        [FromServices] IGetCustomerByIdUseCase useCase,
        CancellationToken cancellationToken)
    {
        var result = await useCase.Handle(id, cancellationToken);

        return Ok(result);
    }

    [HttpPut("{id}")]
    [ProducesResponseType(typeof(CustomerResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<CustomerResponse>> Update(
        [FromRoute] string id,
        [FromBody] UpdateCustomerRequest request,
        [FromServices] IUpdateCustomerUseCase useCase,
        CancellationToken cancellationToken)
    {
        var command = new UpdateCustomerCommand(id, request.Name, request.Email, request.Ddd, request.Phone);

        var result = await useCase.Handle(command, cancellationToken);

        return Ok(result);
    }

    [HttpPatch("{id}/status")]
    [ProducesResponseType(typeof(CustomerResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<CustomerResponse>> UpdateStatus(
        [FromRoute] string id,
        [FromBody] UpdateCustomerStatusRequest request,
        [FromServices] IUpdateCustomerStatusUseCase useCase,
        CancellationToken cancellationToken)
    {
        var command = new UpdateCustomerStatusCommand(id, request.Status);

        var result = await useCase.Handle(command, cancellationToken);

        return Ok(result);
    }
}
