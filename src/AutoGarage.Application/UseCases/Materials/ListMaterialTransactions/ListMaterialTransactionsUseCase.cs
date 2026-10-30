using AutoGarage.Application.Services.Transactions;
using AutoGarage.Communication.Responses.Materials;
using AutoGarage.Domain.Enums.Materials;
using AutoGarage.Domain.Exceptions.Materials.Exceptions;
using AutoGarage.Domain.Repositories.Materials;
using MapsterMapper;

namespace AutoGarage.Application.UseCases.Materials.ListMaterialTransactions;

public class ListMaterialTransactionsUseCase : IListMaterialTransactionsUseCase
{
    private readonly IMaterialReadOnlyRepository _materialReadOnlyRepository;
    private readonly IMaterialTransactionReadOnlyRepository _materialTransactionReadOnlyRepository;
    private readonly IMapper _mapper;

    public ListMaterialTransactionsUseCase(
        IMaterialReadOnlyRepository materialReadOnlyRepository,
        IMaterialTransactionReadOnlyRepository materialTransactionReadOnlyRepository,
        IMapper mapper)
    {
        _materialReadOnlyRepository = materialReadOnlyRepository;
        _materialTransactionReadOnlyRepository = materialTransactionReadOnlyRepository;
        _mapper = mapper;
    }

    public async Task<IReadOnlyList<MaterialTransactionResponse>> Handle(string materialId, string? type, CancellationToken cancellationToken)
    {
        if (!Guid.TryParse(materialId, out var id))
        {
            throw new InvalidMaterialIdException();
        }

        // type ausente ou em branco traz os dois tipos. Valor presente e inválido é rejeitado antes da consulta.
        TransactionType? filter = null;

        if (!string.IsNullOrWhiteSpace(type))
        {
            if (!TransactionTypeParser.TryParse(type, out var parsed))
            {
                throw new InvalidMaterialTransactionTypeException();
            }

            filter = parsed;
        }

        _ = await _materialReadOnlyRepository.GetById(id, cancellationToken)
            ?? throw new MaterialNotFoundException(id);

        var transactions = await _materialTransactionReadOnlyRepository.GetByMaterialId(id, filter, cancellationToken);

        return _mapper.Map<List<MaterialTransactionResponse>>(transactions);
    }
}
