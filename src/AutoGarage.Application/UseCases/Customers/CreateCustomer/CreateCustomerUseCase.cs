using AutoGarage.Application.Commands.Customers;
using AutoGarage.Application.Services.Normalization;
using AutoGarage.Communication.Responses.Customers;
using AutoGarage.Domain.Common.Exceptions;
using AutoGarage.Domain.Entities;
using AutoGarage.Domain.Exceptions.Customers.Exceptions;
using AutoGarage.Domain.Repositories;
using AutoGarage.Domain.Repositories.Customers;
using AutoGarage.Domain.ValueObjects;
using MapsterMapper;
using System.Text.RegularExpressions;

namespace AutoGarage.Application.UseCases.Customers.CreateCustomer;

public class CreateCustomerUseCase : ICreateCustomerUseCase
{
    private static readonly Regex NonDigits = new("[^0-9]", RegexOptions.Compiled);

    private readonly ICustomerReadOnlyRepository _customerReadOnlyRepository;
    private readonly ICustomerWriteOnlyRepository _customerWriteOnlyRepository;
    private readonly IUnitWork _workUnit;
    private readonly IStringNormalizer _stringNormalizer;
    private readonly IMapper _mapper;

    public CreateCustomerUseCase(ICustomerReadOnlyRepository customerReadOnlyRepository, ICustomerWriteOnlyRepository customerWriteOnlyRepository, IUnitWork workUnit, IStringNormalizer stringNormalizer, IMapper mapper)
    {
        _customerReadOnlyRepository = customerReadOnlyRepository;
        _customerWriteOnlyRepository = customerWriteOnlyRepository;
        _workUnit = workUnit;
        _stringNormalizer = stringNormalizer;
        _mapper = mapper;
    }

    public async Task<CustomerResponse> Handle(CreateCustomerCommand command, CancellationToken cancellationToken)
    {
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

        var document = new Document(command.Document);

        var hasEmail = await _customerReadOnlyRepository.ExistsByEmail(command.Email, cancellationToken);
        if (hasEmail)
        {
            throw new DuplicateCustomerEmailException(command.Email);
        }

        var hasDocument = await _customerReadOnlyRepository.ExistsByDocument(document.Value, cancellationToken);
        if (hasDocument)
        {
            throw new DuplicateCustomerDocumentException(document.Value);
        }

        var customer = Customer.Create(command.Name, document, command.Email, ddd, phone);

        await _customerWriteOnlyRepository.Add(customer, cancellationToken);
        await _workUnit.Commit();

        return _mapper.Map<CustomerResponse>(customer);
    }

    // O documento não é normalizado aqui: higienização e maiúsculas pertencem ao Value Object Document.
    private CreateCustomerCommand Normalize(CreateCustomerCommand command) =>
        command with
        {
            Name = _stringNormalizer.Normalize(command.Name),
            Email = _stringNormalizer.Normalize(command.Email).ToUpperInvariant()
        };

    private static void Validate(CreateCustomerCommand command)
    {
        var validator = new CreateCustomerValidator();

        var result = validator.Validate(command);

        if (!result.IsValid)
        {
            var errorMessages = result.Errors.Select(e => e.ErrorMessage).ToList();

            throw new ErrorOnValidationException(errorMessages);
        }
    }
}
