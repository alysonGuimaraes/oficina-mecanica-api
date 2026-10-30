using AutoGarage.Application.Commands.Services;
using AutoGarage.Application.Services.Normalization;
using AutoGarage.Communication.Responses.Services;
using AutoGarage.Domain.Common.Exceptions;
using AutoGarage.Domain.Exceptions.Services.Exceptions;
using AutoGarage.Domain.Repositories;
using AutoGarage.Domain.Repositories.Services;
using MapsterMapper;

namespace AutoGarage.Application.UseCases.Services.UpdateService;

public class UpdateServiceUseCase : IUpdateServiceUseCase
{
    private readonly IServiceReadOnlyRepository _serviceReadOnlyRepository;
    private readonly IServiceWriteOnlyRepository _serviceWriteOnlyRepository;
    private readonly IUnitWork _workUnit;
    private readonly IStringNormalizer _stringNormalizer;
    private readonly IMapper _mapper;

    public UpdateServiceUseCase(IServiceReadOnlyRepository serviceReadOnlyRepository, IServiceWriteOnlyRepository serviceWriteOnlyRepository, IUnitWork workUnit, IStringNormalizer stringNormalizer, IMapper mapper)
    {
        _serviceReadOnlyRepository = serviceReadOnlyRepository;
        _serviceWriteOnlyRepository = serviceWriteOnlyRepository;
        _workUnit = workUnit;
        _stringNormalizer = stringNormalizer;
        _mapper = mapper;
    }

    public async Task<ServiceResponse> Handle(UpdateServiceCommand command, CancellationToken cancellationToken)
    {
        if (!Guid.TryParse(command.Id, out var serviceId))
        {
            throw new InvalidServiceIdException();
        }

        command = Normalize(command);

        Validate(command);

        var service = await _serviceReadOnlyRepository.GetById(serviceId, cancellationToken)
            ?? throw new ServiceNotFoundException(serviceId);

        var hasNameOfAnotherService = await _serviceReadOnlyRepository.ExistsByNameExcludingId(command.Name, serviceId, cancellationToken);
        if (hasNameOfAnotherService)
        {
            throw new DuplicateServiceNameException(command.Name);
        }

        // Descrição vazia apaga a descrição atual: a validação já aceitou a string vazia, e aqui ela vira nula.
        var description = string.IsNullOrEmpty(command.Description) ? null : command.Description;

        service.UpdateProfile(command.Name, description, command.Price);

        await _serviceWriteOnlyRepository.Update(service, cancellationToken);
        await _workUnit.Commit();

        return _mapper.Map<ServiceResponse>(service);
    }

    // Nula continua nula, para a validação rejeitar a ausência do campo; string vazia ou só com espaços vira "".
    private UpdateServiceCommand Normalize(UpdateServiceCommand command) =>
        command with
        {
            Name = _stringNormalizer.Normalize(command.Name).ToUpperInvariant(),
            Description = command.Description is null ? null : _stringNormalizer.Normalize(command.Description)
        };

    private static void Validate(UpdateServiceCommand command)
    {
        var validator = new UpdateServiceValidator();

        var result = validator.Validate(command);

        if (!result.IsValid)
        {
            var errorMessages = result.Errors.Select(e => e.ErrorMessage).ToList();

            throw new ErrorOnValidationException(errorMessages);
        }
    }
}
