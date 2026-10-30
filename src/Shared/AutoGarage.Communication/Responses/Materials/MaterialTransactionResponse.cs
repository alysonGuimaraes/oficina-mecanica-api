using AutoGarage.Domain.Enums.Materials;

namespace AutoGarage.Communication.Responses.Materials;

public record MaterialTransactionResponse(
    Guid Id,
    Guid MaterialId,
    TransactionType Type,
    int Quantity,
    int BalanceAfter,
    DateTime CreatedAt
);
