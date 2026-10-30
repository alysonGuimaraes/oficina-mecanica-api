using AutoGarage.Application.Services.Transactions;
using AutoGarage.Communication.Responses.Materials;
using AutoGarage.Domain.Enums.MaterialTransactions;
using AutoGarage.Domain.Exceptions.MaterialTransactions.Exceptions;
using AutoGarage.Domain.Repositories.MaterialTransactions;
using MapsterMapper;

namespace AutoGarage.Application.UseCases.MaterialTransactions.ListAllMaterialTransactions;

public class ListAllMaterialTransactionsUseCase : IListAllMaterialTransactionsUseCase
{
    private readonly IMaterialTransactionReadOnlyRepository _materialTransactionReadOnlyRepository;
    private readonly IMapper _mapper;

    public ListAllMaterialTransactionsUseCase(IMaterialTransactionReadOnlyRepository materialTransactionReadOnlyRepository, IMapper mapper)
    {
        _materialTransactionReadOnlyRepository = materialTransactionReadOnlyRepository;
        _mapper = mapper;
    }

    public async Task<IReadOnlyList<MaterialTransactionResponse>> Handle(string? type, CancellationToken cancellationToken)
    {
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

        var transactions = await _materialTransactionReadOnlyRepository.GetAll(filter, cancellationToken);

        return _mapper.Map<List<MaterialTransactionResponse>>(transactions);
    }
}
