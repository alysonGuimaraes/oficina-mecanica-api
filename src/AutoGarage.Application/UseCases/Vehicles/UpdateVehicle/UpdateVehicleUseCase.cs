using AutoGarage.Application.Commands.Vehicles;
using AutoGarage.Application.Services.Normalization;
using AutoGarage.Communication.Responses.Vehicles;
using AutoGarage.Domain.Common.Exceptions;
using AutoGarage.Domain.Exceptions.Customers.Exceptions;
using AutoGarage.Domain.Exceptions.Vehicles.Exceptions;
using AutoGarage.Domain.Repositories;
using AutoGarage.Domain.Repositories.Customers;
using AutoGarage.Domain.Repositories.Vehicles;
using AutoGarage.Domain.Shared;
using AutoGarage.Domain.ValueObjects;
using MapsterMapper;

namespace AutoGarage.Application.UseCases.Vehicles.UpdateVehicle;

public class UpdateVehicleUseCase : IUpdateVehicleUseCase
{
    private readonly IVehicleReadOnlyRepository _vehicleReadOnlyRepository;
    private readonly IVehicleWriteOnlyRepository _vehicleWriteOnlyRepository;
    private readonly ICustomerReadOnlyRepository _customerReadOnlyRepository;
    private readonly IUnitWork _workUnit;
    private readonly IStringNormalizer _stringNormalizer;
    private readonly IMapper _mapper;

    public UpdateVehicleUseCase(IVehicleReadOnlyRepository vehicleReadOnlyRepository, IVehicleWriteOnlyRepository vehicleWriteOnlyRepository, ICustomerReadOnlyRepository customerReadOnlyRepository, IUnitWork workUnit, IStringNormalizer stringNormalizer, IMapper mapper)
    {
        _vehicleReadOnlyRepository = vehicleReadOnlyRepository;
        _vehicleWriteOnlyRepository = vehicleWriteOnlyRepository;
        _customerReadOnlyRepository = customerReadOnlyRepository;
        _workUnit = workUnit;
        _stringNormalizer = stringNormalizer;
        _mapper = mapper;
    }

    public async Task<VehicleResponse> Handle(UpdateVehicleCommand command, CancellationToken cancellationToken)
    {
        if (!Guid.TryParse(command.Id, out var vehicleId))
        {
            throw new InvalidVehicleIdException();
        }

        command = Normalize(command);

        Validate(command);

        if (!Guid.TryParse(command.CustomerId, out var customerId))
        {
            throw new InvalidCustomerIdException();
        }

        // A placa não é normalizada aqui: a higienização e o formato são responsabilidade do Value Object Plate.
        var plate = new Plate(command.Plate);

        var vehicle = await _vehicleReadOnlyRepository.GetById(vehicleId, cancellationToken)
            ?? throw new VehicleNotFoundException(vehicleId);

        var customer = await _customerReadOnlyRepository.GetById(customerId, cancellationToken)
            ?? throw new CustomerNotFoundException(customerId);

        if (customer.Status != EntityStatus.ACTIVE)
        {
            throw new VehicleOwnerInactiveException(customerId);
        }

        var hasPlateOfAnotherVehicle = await _vehicleReadOnlyRepository.ExistsByPlateExcludingId(plate.Value, vehicleId, cancellationToken);
        if (hasPlateOfAnotherVehicle)
        {
            throw new DuplicateVehiclePlateException(plate.Value);
        }

        // A trava de placa mercosul fica na entidade, que lança PlateUpdateNotAllowedException antes de alterar qualquer campo.
        vehicle.UpdateProfile(command.Brand, command.Model, plate, command.Year);
        vehicle.ChangeOwner(customerId);

        await _vehicleWriteOnlyRepository.Update(vehicle, cancellationToken);
        await _workUnit.Commit();

        return _mapper.Map<VehicleResponse>(vehicle);
    }

    private UpdateVehicleCommand Normalize(UpdateVehicleCommand command) =>
        command with
        {
            Brand = _stringNormalizer.Normalize(command.Brand).ToUpperInvariant(),
            Model = _stringNormalizer.Normalize(command.Model).ToUpperInvariant()
        };

    private static void Validate(UpdateVehicleCommand command)
    {
        var validator = new UpdateVehicleValidator();

        var result = validator.Validate(command);

        if (!result.IsValid)
        {
            var errorMessages = result.Errors.Select(e => e.ErrorMessage).ToList();

            throw new ErrorOnValidationException(errorMessages);
        }
    }
}
