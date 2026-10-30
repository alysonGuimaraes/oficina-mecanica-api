using AutoGarage.Communication.Responses.Materials;
using AutoGarage.Domain.Exceptions.Materials.Exceptions;
using AutoGarage.Domain.Repositories.Materials;
using MapsterMapper;

namespace AutoGarage.Application.UseCases.Materials.GetMaterialById;

public class GetMaterialByIdUseCase : IGetMaterialByIdUseCase
{
    private readonly IMaterialReadOnlyRepository _materialReadOnlyRepository;
    private readonly IMapper _mapper;

    public GetMaterialByIdUseCase(IMaterialReadOnlyRepository materialReadOnlyRepository, IMapper mapper)
    {
        _materialReadOnlyRepository = materialReadOnlyRepository;
        _mapper = mapper;
    }

    public async Task<MaterialResponse> Handle(string id, CancellationToken cancellationToken)
    {
        if (!Guid.TryParse(id, out var materialId))
        {
            throw new InvalidMaterialIdException();
        }

        var material = await _materialReadOnlyRepository.GetById(materialId, cancellationToken)
            ?? throw new MaterialNotFoundException(materialId);

        return _mapper.Map<MaterialResponse>(material);
    }
}
