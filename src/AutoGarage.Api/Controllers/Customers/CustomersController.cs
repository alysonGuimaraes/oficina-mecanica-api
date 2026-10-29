using AutoGarage.Application.Commands.Customers;
using AutoGarage.Application.UseCases.Customers.CreateCustomer;
using AutoGarage.Application.UseCases.Customers.ListCustomers;
using AutoGarage.Communication.Requests.Customers;
using AutoGarage.Communication.Responses.Customers;
using Microsoft.AspNetCore.Mvc;

namespace AutoGarage.Api.Controllers.Customers;

[ApiController]
[Route("api/customers")]
public class CustomersController : ControllerBase
{
    [HttpPost]
    [ProducesResponseType(typeof(CustomerResponse), StatusCodes.Status201Created)]
    public async Task<ActionResult> Create(
        [FromServices] ICreateCustomerUseCase useCase,
        [FromBody] CreateCustomerRequest request,
        CancellationToken cancellationToken)
    {
        var command = new CreateCustomerCommand(request.Name, request.Document, request.Email, request.Ddd, request.Phone);

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
}
