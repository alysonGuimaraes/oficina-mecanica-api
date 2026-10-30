using AutoGarage.Communication.Responses.Materials;
using AutoGarage.Domain.Exceptions.MaterialTransactions.Exceptions;
using AutoGarage.Domain.Repositories.MaterialTransactions;
using MapsterMapper;

namespace AutoGarage.Application.UseCases.MaterialTransactions.GetMaterialTransactionById;

public class GetMaterialTransactionByIdUseCase : IGetMaterialTransactionByIdUseCase
{
    private readonly IMaterialTransactionReadOnlyRepository _materialTransactionReadOnlyRepository;
    private readonly IMapper _mapper;

    public GetMaterialTransactionByIdUseCase(IMaterialTransactionReadOnlyRepository materialTransactionReadOnlyRepository, IMapper mapper)
    {
        _materialTransactionReadOnlyRepository = materialTransactionReadOnlyRepository;
        _mapper = mapper;
    }

    public async Task<MaterialTransactionResponse> Handle(string id, CancellationToken cancellationToken)
    {
        if (!Guid.TryParse(id, out var transactionId))
        {
            throw new InvalidMaterialTransactionIdException();
        }

        var transaction = await _materialTransactionReadOnlyRepository.GetById(transactionId, cancellationToken)
            ?? throw new MaterialTransactionNotFoundException(transactionId);

        return _mapper.Map<MaterialTransactionResponse>(transaction);
    }
}
