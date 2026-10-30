using AutoGarage.Application.Commands.Materials;
using AutoGarage.Communication.Responses.Materials;
using AutoGarage.Domain.Common.Exceptions;
using AutoGarage.Domain.Entities;
using AutoGarage.Domain.Enums.MaterialTransactions;
using AutoGarage.Domain.Exceptions.Materials.Exceptions;
using AutoGarage.Domain.Repositories;
using AutoGarage.Domain.Repositories.Materials;
using AutoGarage.Domain.Repositories.MaterialTransactions;
using MapsterMapper;

namespace AutoGarage.Application.UseCases.Materials.StockEntry;

public class StockEntryUseCase : IStockEntryUseCase
{
    private readonly IMaterialReadOnlyRepository _materialReadOnlyRepository;
    private readonly IMaterialWriteOnlyRepository _materialWriteOnlyRepository;
    private readonly IMaterialTransactionWriteOnlyRepository _materialTransactionWriteOnlyRepository;
    private readonly IUnitWork _workUnit;
    private readonly IMapper _mapper;

    public StockEntryUseCase(
        IMaterialReadOnlyRepository materialReadOnlyRepository,
        IMaterialWriteOnlyRepository materialWriteOnlyRepository,
        IMaterialTransactionWriteOnlyRepository materialTransactionWriteOnlyRepository,
        IUnitWork workUnit,
        IMapper mapper)
    {
        _materialReadOnlyRepository = materialReadOnlyRepository;
        _materialWriteOnlyRepository = materialWriteOnlyRepository;
        _materialTransactionWriteOnlyRepository = materialTransactionWriteOnlyRepository;
        _workUnit = workUnit;
        _mapper = mapper;
    }

    public async Task<MaterialResponse> Handle(StockEntryCommand command, CancellationToken cancellationToken)
    {
        if (!Guid.TryParse(command.MaterialId, out var materialId))
        {
            throw new InvalidMaterialIdException();
        }

        if (command.Quantity <= 0)
        {
            throw new ErrorOnValidationException(["The quantity field must be greater than zero."]);
        }

        var material = await _materialReadOnlyRepository.GetById(materialId, cancellationToken)
            ?? throw new MaterialNotFoundException(materialId);

        material.IncreaseStock(command.Quantity);

        await _materialWriteOnlyRepository.Update(material, cancellationToken);

        var transaction = MaterialTransaction.Create(material.Id, TransactionType.IN, command.Quantity, material.StockQuantity);

        await _materialTransactionWriteOnlyRepository.Add(transaction, cancellationToken);
        await _workUnit.Commit();

        return _mapper.Map<MaterialResponse>(material);
    }
}
