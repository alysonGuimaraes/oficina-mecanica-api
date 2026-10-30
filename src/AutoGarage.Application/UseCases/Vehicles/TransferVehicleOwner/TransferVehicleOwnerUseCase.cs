using AutoGarage.Application.Commands.Vehicles;
using AutoGarage.Communication.Responses.Vehicles;
using AutoGarage.Domain.Common.Exceptions;
using AutoGarage.Domain.Entities;
using AutoGarage.Domain.Exceptions.Customers.Exceptions;
using AutoGarage.Domain.Exceptions.Vehicles.Exceptions;
using AutoGarage.Domain.Repositories;
using AutoGarage.Domain.Repositories.Customers;
using AutoGarage.Domain.Repositories.Vehicles;
using AutoGarage.Domain.Shared;
using AutoGarage.Domain.ValueObjects;
using MapsterMapper;

namespace AutoGarage.Application.UseCases.Vehicles.TransferVehicleOwner;

public class TransferVehicleOwnerUseCase : ITransferVehicleOwnerUseCase
{
    private readonly IVehicleReadOnlyRepository _vehicleReadOnlyRepository;
    private readonly IVehicleWriteOnlyRepository _vehicleWriteOnlyRepository;
    private readonly ICustomerReadOnlyRepository _customerReadOnlyRepository;
    private readonly IUnitWork _workUnit;
    private readonly IMapper _mapper;

    public TransferVehicleOwnerUseCase(IVehicleReadOnlyRepository vehicleReadOnlyRepository, IVehicleWriteOnlyRepository vehicleWriteOnlyRepository, ICustomerReadOnlyRepository customerReadOnlyRepository, IUnitWork workUnit, IMapper mapper)
    {
        _vehicleReadOnlyRepository = vehicleReadOnlyRepository;
        _vehicleWriteOnlyRepository = vehicleWriteOnlyRepository;
        _customerReadOnlyRepository = customerReadOnlyRepository;
        _workUnit = workUnit;
        _mapper = mapper;
    }

    public async Task<VehicleResponse> Handle(TransferVehicleOwnerCommand command, CancellationToken cancellationToken)
    {
        if (!Guid.TryParse(command.Id, out var vehicleId))
        {
            throw new InvalidVehicleIdException();
        }

        Validate(command);

        // O Value Object Document aceita formatado ou não, e lança InvalidDocumentException se o documento for inválido.
        var document = new Document(command.NewOwnerDocument);

        var vehicle = await _vehicleReadOnlyRepository.GetById(vehicleId, cancellationToken)
            ?? throw new VehicleNotFoundException(vehicleId);

        var newOwner = await _customerReadOnlyRepository.GetByDocument(document.Value, cancellationToken)
            ?? throw new CustomerDocumentNotFoundException(document.Value);

        if (newOwner.Status != EntityStatus.ACTIVE)
        {
            throw new VehicleOwnerInactiveException(newOwner.Id);
        }

        vehicle.ChangeOwner(newOwner.Id);

        await _vehicleWriteOnlyRepository.Update(vehicle, cancellationToken);
        await _workUnit.Commit();

        return _mapper.Map<VehicleResponse>(vehicle);
    }

    private static void Validate(TransferVehicleOwnerCommand command)
    {
        var validator = new TransferVehicleOwnerValidator();

        var result = validator.Validate(command);

        if (!result.IsValid)
        {
            var errorMessages = result.Errors.Select(e => e.ErrorMessage).ToList();

            throw new ErrorOnValidationException(errorMessages);
        }
    }
}
