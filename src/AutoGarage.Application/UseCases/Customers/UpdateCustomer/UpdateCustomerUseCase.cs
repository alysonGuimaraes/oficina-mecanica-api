using AutoGarage.Application.Commands.Customers;
using AutoGarage.Application.Services.Normalization;
using AutoGarage.Communication.Responses.Customers;
using AutoGarage.Domain.Common.Exceptions;
using AutoGarage.Domain.Exceptions.Customers.Exceptions;
using AutoGarage.Domain.Repositories;
using AutoGarage.Domain.Repositories.Customers;
using MapsterMapper;
using System.Text.RegularExpressions;

namespace AutoGarage.Application.UseCases.Customers.UpdateCustomer;

public class UpdateCustomerUseCase : IUpdateCustomerUseCase
{
    private static readonly Regex NonDigits = new("[^0-9]", RegexOptions.Compiled);

    private readonly ICustomerReadOnlyRepository _customerReadOnlyRepository;
    private readonly ICustomerWriteOnlyRepository _customerWriteOnlyRepository;
    private readonly IUnitWork _workUnit;
    private readonly IStringNormalizer _stringNormalizer;
    private readonly IMapper _mapper;

    public UpdateCustomerUseCase(ICustomerReadOnlyRepository customerReadOnlyRepository, ICustomerWriteOnlyRepository customerWriteOnlyRepository, IUnitWork workUnit, IStringNormalizer stringNormalizer, IMapper mapper)
    {
        _customerReadOnlyRepository = customerReadOnlyRepository;
        _customerWriteOnlyRepository = customerWriteOnlyRepository;
        _workUnit = workUnit;
        _stringNormalizer = stringNormalizer;
        _mapper = mapper;
    }

    public async Task<CustomerResponse> Handle(UpdateCustomerCommand command, CancellationToken cancellationToken)
    {
        if (!Guid.TryParse(command.Id, out var customerId))
        {
            throw new InvalidCustomerIdException();
        }

        command = Normalize(command);

        Validate(command);

        var ddd = NonDigits.Replace(command.Ddd, string.Empty);
        if (ddd.Length != 2)
        {
            throw new ErrorOnValidationException(["The ddd field must have 2 digits."]);
        }

        var phone = NonDigits.Replace(command.Phone, string.Empty);
        if (phone.Length != 9)
        {
            throw new ErrorOnValidationException(["The phone field must have 9 digits."]);
        }

        var customer = await _customerReadOnlyRepository.GetById(customerId, cancellationToken)
            ?? throw new CustomerNotFoundException(customerId);

        var hasEmailOfAnotherCustomer = await _customerReadOnlyRepository
            .ExistsByEmailExcludingId(command.Email, customerId, cancellationToken);
        if (hasEmailOfAnotherCustomer)
        {
            throw new DuplicateCustomerEmailException(command.Email);
        }

        customer.UpdateProfile(command.Name, command.Email, ddd, phone);

        await _customerWriteOnlyRepository.Update(customer, cancellationToken);
        await _workUnit.Commit();

        return _mapper.Map<CustomerResponse>(customer);
    }

    // O documento não é normalizado nem alterado aqui: ele é único e imutável por cliente.
    private UpdateCustomerCommand Normalize(UpdateCustomerCommand command) =>
        command with
        {
            Name = _stringNormalizer.Normalize(command.Name),
            Email = _stringNormalizer.Normalize(command.Email).ToUpperInvariant()
        };

    private static void Validate(UpdateCustomerCommand command)
    {
        var validator = new UpdateCustomerValidator();

        var result = validator.Validate(command);

        if (!result.IsValid)
        {
            var errorMessages = result.Errors.Select(e => e.ErrorMessage).ToList();

            throw new ErrorOnValidationException(errorMessages);
        }
    }
}
