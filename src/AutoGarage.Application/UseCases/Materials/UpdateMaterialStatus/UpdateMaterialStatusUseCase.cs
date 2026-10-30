using AutoGarage.Application.Commands.Materials;
using AutoGarage.Application.Services.Status;
using AutoGarage.Communication.Responses.Materials;
using AutoGarage.Domain.Exceptions.Materials.Exceptions;
using AutoGarage.Domain.Repositories;
using AutoGarage.Domain.Repositories.Materials;
using AutoGarage.Domain.Shared;
using MapsterMapper;

namespace AutoGarage.Application.UseCases.Materials.UpdateMaterialStatus;

public class UpdateMaterialStatusUseCase : IUpdateMaterialStatusUseCase
{
    private readonly IMaterialReadOnlyRepository _materialReadOnlyRepository;
    private readonly IMaterialWriteOnlyRepository _materialWriteOnlyRepository;
    private readonly IUnitWork _workUnit;
    private readonly IMapper _mapper;

    public UpdateMaterialStatusUseCase(IMaterialReadOnlyRepository materialReadOnlyRepository, IMaterialWriteOnlyRepository materialWriteOnlyRepository, IUnitWork workUnit, IMapper mapper)
    {
        _materialReadOnlyRepository = materialReadOnlyRepository;
        _materialWriteOnlyRepository = materialWriteOnlyRepository;
        _workUnit = workUnit;
        _mapper = mapper;
    }

    public async Task<MaterialResponse> Handle(UpdateMaterialStatusCommand command, CancellationToken cancellationToken)
    {
        if (!Guid.TryParse(command.Id, out var materialId))
        {
            throw new InvalidMaterialIdException();
        }

        // Status ausente, vazio ou inválido é rejeitado: não existe padrão para a transição.
        if (!EntityStatusParser.TryParse(command.Status, out var status))
        {
            throw new InvalidMaterialStatusException();
        }

        var material = await _materialReadOnlyRepository.GetById(materialId, cancellationToken)
            ?? throw new MaterialNotFoundException(materialId);

        if (status == EntityStatus.ACTIVE)
        {
            material.Activate();
        }
        else
        {
            material.Deactivate();
        }

        await _materialWriteOnlyRepository.Update(material, cancellationToken);
        await _workUnit.Commit();

        return _mapper.Map<MaterialResponse>(material);
    }
}
