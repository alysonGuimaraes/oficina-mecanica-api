using AutoGarage.Communication.Responses.Customers;
using AutoGarage.Domain.Common.Exceptions;
using AutoGarage.Domain.Exceptions.Customers.Exceptions;
using AutoGarage.Domain.Repositories.Customers;
using AutoGarage.Domain.ValueObjects;
using MapsterMapper;

namespace AutoGarage.Application.UseCases.Customers.GetCustomerByDocument;

public class GetCustomerByDocumentUseCase : IGetCustomerByDocumentUseCase
{
    private readonly ICustomerReadOnlyRepository _customerReadOnlyRepository;
    private readonly IMapper _mapper;

    public GetCustomerByDocumentUseCase(ICustomerReadOnlyRepository customerReadOnlyRepository, IMapper mapper)
    {
        _customerReadOnlyRepository = customerReadOnlyRepository;
        _mapper = mapper;
    }

    public async Task<CustomerResponse> Handle(string? document, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(document))
        {
            throw new ErrorOnValidationException(["The document field is required."]);
        }

        var documentValue = new Document(document);

        var customer = await _customerReadOnlyRepository.GetByDocument(documentValue.Value, cancellationToken)
            ?? throw new CustomerDocumentNotFoundException(documentValue.Value);

        return _mapper.Map<CustomerResponse>(customer);
    }
}
