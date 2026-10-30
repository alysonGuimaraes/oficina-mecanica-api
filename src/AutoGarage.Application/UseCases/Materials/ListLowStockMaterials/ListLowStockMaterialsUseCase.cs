using AutoGarage.Application.Services.Status;
using AutoGarage.Communication.Responses.Materials;
using AutoGarage.Domain.Exceptions.Materials.Exceptions;
using AutoGarage.Domain.Repositories.Materials;
using AutoGarage.Domain.Shared;
using MapsterMapper;

namespace AutoGarage.Application.UseCases.Materials.ListLowStockMaterials;

public class ListLowStockMaterialsUseCase : IListLowStockMaterialsUseCase
{
    private readonly IMaterialReadOnlyRepository _materialReadOnlyRepository;
    private readonly IMapper _mapper;

    public ListLowStockMaterialsUseCase(IMaterialReadOnlyRepository materialReadOnlyRepository, IMapper mapper)
    {
        _materialReadOnlyRepository = materialReadOnlyRepository;
        _mapper = mapper;
    }

    public async Task<IReadOnlyList<MaterialResponse>> Handle(string? status, CancellationToken cancellationToken)
    {
        var filter = EntityStatus.ACTIVE;

        if (!string.IsNullOrWhiteSpace(status) && !EntityStatusParser.TryParse(status, out filter))
        {
            throw new InvalidMaterialStatusException();
        }

        var materials = await _materialReadOnlyRepository.GetLowStockByStatus(filter, cancellationToken);

        return _mapper.Map<List<MaterialResponse>>(materials);
    }
}
