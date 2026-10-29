using AutoGarage.Domain.Enums.Customers;
using AutoGarage.Domain.Shared;

namespace AutoGarage.Communication.Responses.Customers;

public record CustomerResponse(
    Guid Id,
    string Name,
    string Document,
    DocumentType DocumentType,
    string Email,
    string Ddd,
    string PhoneNumber,
    EntityStatus Status
);
