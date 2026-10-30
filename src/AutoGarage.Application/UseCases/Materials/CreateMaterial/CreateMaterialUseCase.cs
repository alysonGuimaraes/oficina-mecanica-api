using AutoGarage.Application.Commands.Materials;
using AutoGarage.Application.Services.Normalization;
using AutoGarage.Communication.Responses.Materials;
using AutoGarage.Domain.Common.Exceptions;
using AutoGarage.Domain.Entities;
using AutoGarage.Domain.Exceptions.Materials.Exceptions;
using AutoGarage.Domain.Repositories;
using AutoGarage.Domain.Repositories.Materials;
using MapsterMapper;

namespace AutoGarage.Application.UseCases.Materials.CreateMaterial;

public class CreateMaterialUseCase : ICreateMaterialUseCase
{
    private readonly IMaterialReadOnlyRepository _materialReadOnlyRepository;
    private readonly IMaterialWriteOnlyRepository _materialWriteOnlyRepository;
    private readonly IUnitWork _workUnit;
    private readonly IStringNormalizer _stringNormalizer;
    private readonly IMapper _mapper;

    public CreateMaterialUseCase(IMaterialReadOnlyRepository materialReadOnlyRepository, IMaterialWriteOnlyRepository materialWriteOnlyRepository, IUnitWork workUnit, IStringNormalizer stringNormalizer, IMapper mapper)
    {
        _materialReadOnlyRepository = materialReadOnlyRepository;
        _materialWriteOnlyRepository = materialWriteOnlyRepository;
        _workUnit = workUnit;
        _stringNormalizer = stringNormalizer;
        _mapper = mapper;
    }

    public async Task<MaterialResponse> Handle(CreateMaterialCommand command, CancellationToken cancellationToken)
    {
        command = Normalize(command);

        Validate(command);

        var hasName = await _materialReadOnlyRepository.ExistsByName(command.Name, cancellationToken);
        if (hasName)
        {
            throw new DuplicateMaterialNameException(command.Name);
        }

        var material = Material.Create(command.Name, command.Description, command.Price, command.StockMinimum);

        await _materialWriteOnlyRepository.Add(material, cancellationToken);
        await _workUnit.Commit();

        return _mapper.Map<MaterialResponse>(material);
    }

    private CreateMaterialCommand Normalize(CreateMaterialCommand command)
    {
        var description = _stringNormalizer.Normalize(command.Description);

        return command with
        {
            Name = _stringNormalizer.Normalize(command.Name).ToUpperInvariant(),
            Description = string.IsNullOrEmpty(description) ? null : description
        };
    }

    private static void Validate(CreateMaterialCommand command)
    {
        var validator = new CreateMaterialValidator();

        var result = validator.Validate(command);

        if (!result.IsValid)
        {
            var errorMessages = result.Errors.Select(e => e.ErrorMessage).ToList();

            throw new ErrorOnValidationException(errorMessages);
        }
    }
}
