using AutoGarage.Application.Commands.Services;
using AutoGarage.Application.Services.Normalization;
using AutoGarage.Communication.Responses.Services;
using AutoGarage.Domain.Common.Exceptions;
using AutoGarage.Domain.Entities;
using AutoGarage.Domain.Exceptions.Services.Exceptions;
using AutoGarage.Domain.Repositories;
using AutoGarage.Domain.Repositories.Services;
using MapsterMapper;

namespace AutoGarage.Application.UseCases.Services.CreateService;

public class CreateServiceUseCase : ICreateServiceUseCase
{
    private readonly IServiceReadOnlyRepository _serviceReadOnlyRepository;
    private readonly IServiceWriteOnlyRepository _serviceWriteOnlyRepository;
    private readonly IUnitWork _workUnit;
    private readonly IStringNormalizer _stringNormalizer;
    private readonly IMapper _mapper;

    public CreateServiceUseCase(IServiceReadOnlyRepository serviceReadOnlyRepository, IServiceWriteOnlyRepository serviceWriteOnlyRepository, IUnitWork workUnit, IStringNormalizer stringNormalizer, IMapper mapper)
    {
        _serviceReadOnlyRepository = serviceReadOnlyRepository;
        _serviceWriteOnlyRepository = serviceWriteOnlyRepository;
        _workUnit = workUnit;
        _stringNormalizer = stringNormalizer;
        _mapper = mapper;
    }

    public async Task<ServiceResponse> Handle(CreateServiceCommand command, CancellationToken cancellationToken)
    {
        command = Normalize(command);

        Validate(command);

        var hasName = await _serviceReadOnlyRepository.ExistsByName(command.Name, cancellationToken);
        if (hasName)
        {
            throw new DuplicateServiceNameException(command.Name);
        }

        var service = Service.Create(command.Name, command.Description, command.Price);

        await _serviceWriteOnlyRepository.Add(service, cancellationToken);
        await _workUnit.Commit();

        return _mapper.Map<ServiceResponse>(service);
    }

    private CreateServiceCommand Normalize(CreateServiceCommand command)
    {
        var description = _stringNormalizer.Normalize(command.Description);

        return command with
        {
            Name = _stringNormalizer.Normalize(command.Name).ToUpperInvariant(),
            Description = string.IsNullOrEmpty(description) ? null : description
        };
    }

    private static void Validate(CreateServiceCommand command)
    {
        var validator = new CreateServiceValidator();

        var result = validator.Validate(command);

        if (!result.IsValid)
        {
            var errorMessages = result.Errors.Select(e => e.ErrorMessage).ToList();

            throw new ErrorOnValidationException(errorMessages);
        }
    }
}
