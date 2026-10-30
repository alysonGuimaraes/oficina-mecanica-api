using AutoGarage.Application.Commands.Materials;
using AutoGarage.Application.Services.Normalization;
using AutoGarage.Communication.Responses.Materials;
using AutoGarage.Domain.Common.Exceptions;
using AutoGarage.Domain.Exceptions.Materials.Exceptions;
using AutoGarage.Domain.Repositories;
using AutoGarage.Domain.Repositories.Materials;
using MapsterMapper;

namespace AutoGarage.Application.UseCases.Materials.UpdateMaterial;

public class UpdateMaterialUseCase : IUpdateMaterialUseCase
{
    private readonly IMaterialReadOnlyRepository _materialReadOnlyRepository;
    private readonly IMaterialWriteOnlyRepository _materialWriteOnlyRepository;
    private readonly IUnitWork _workUnit;
    private readonly IStringNormalizer _stringNormalizer;
    private readonly IMapper _mapper;

    public UpdateMaterialUseCase(IMaterialReadOnlyRepository materialReadOnlyRepository, IMaterialWriteOnlyRepository materialWriteOnlyRepository, IUnitWork workUnit, IStringNormalizer stringNormalizer, IMapper mapper)
    {
        _materialReadOnlyRepository = materialReadOnlyRepository;
        _materialWriteOnlyRepository = materialWriteOnlyRepository;
        _workUnit = workUnit;
        _stringNormalizer = stringNormalizer;
        _mapper = mapper;
    }

    public async Task<MaterialResponse> Handle(UpdateMaterialCommand command, CancellationToken cancellationToken)
    {
        if (!Guid.TryParse(command.Id, out var materialId))
        {
            throw new InvalidMaterialIdException();
        }

        command = Normalize(command);

        Validate(command);

        var material = await _materialReadOnlyRepository.GetById(materialId, cancellationToken)
            ?? throw new MaterialNotFoundException(materialId);

        var hasNameOfAnotherMaterial = await _materialReadOnlyRepository.ExistsByNameExcludingId(command.Name, materialId, cancellationToken);
        if (hasNameOfAnotherMaterial)
        {
            throw new DuplicateMaterialNameException(command.Name);
        }

        // Descrição vazia apaga a descrição atual: a validação já aceitou a string vazia, e aqui ela vira nula.
        var description = string.IsNullOrEmpty(command.Description) ? null : command.Description;

        material.UpdateProfile(command.Name, description, command.Price, command.StockMinimum);

        await _materialWriteOnlyRepository.Update(material, cancellationToken);
        await _workUnit.Commit();

        return _mapper.Map<MaterialResponse>(material);
    }

    // Nula continua nula, para a validação rejeitar a ausência do campo; string vazia ou só com espaços vira "".
    private UpdateMaterialCommand Normalize(UpdateMaterialCommand command) =>
        command with
        {
            Name = _stringNormalizer.Normalize(command.Name).ToUpperInvariant(),
            Description = command.Description is null ? null : _stringNormalizer.Normalize(command.Description)
        };

    private static void Validate(UpdateMaterialCommand command)
    {
        var validator = new UpdateMaterialValidator();

        var result = validator.Validate(command);

        if (!result.IsValid)
        {
            var errorMessages = result.Errors.Select(e => e.ErrorMessage).ToList();

            throw new ErrorOnValidationException(errorMessages);
        }
    }
}
