using AutoGarage.Application.Commands.Vehicles;
using AutoGarage.Application.Services.Normalization;
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

namespace AutoGarage.Application.UseCases.Vehicles.CreateVehicle;

public class CreateVehicleUseCase : ICreateVehicleUseCase
{
    private readonly ICustomerReadOnlyRepository _customerReadOnlyRepository;
    private readonly IVehicleReadOnlyRepository _vehicleReadOnlyRepository;
    private readonly IVehicleWriteOnlyRepository _vehicleWriteOnlyRepository;
    private readonly IUnitWork _workUnit;
    private readonly IStringNormalizer _stringNormalizer;
    private readonly IMapper _mapper;

    public CreateVehicleUseCase(ICustomerReadOnlyRepository customerReadOnlyRepository, IVehicleReadOnlyRepository vehicleReadOnlyRepository, IVehicleWriteOnlyRepository vehicleWriteOnlyRepository, IUnitWork workUnit, IStringNormalizer stringNormalizer, IMapper mapper)
    {
        _customerReadOnlyRepository = customerReadOnlyRepository;
        _vehicleReadOnlyRepository = vehicleReadOnlyRepository;
        _vehicleWriteOnlyRepository = vehicleWriteOnlyRepository;
        _workUnit = workUnit;
        _stringNormalizer = stringNormalizer;
        _mapper = mapper;
    }

    public async Task<VehicleResponse> Handle(CreateVehicleCommand command, CancellationToken cancellationToken)
    {
        command = Normalize(command);

        Validate(command);

        if (!Guid.TryParse(command.CustomerId, out var customerId))
        {
            throw new InvalidCustomerIdException();
        }

        var plate = new Plate(command.Plate);

        var customer = await _customerReadOnlyRepository.GetById(customerId, cancellationToken)
            ?? throw new CustomerNotFoundException(customerId);

        if (customer.Status != EntityStatus.ACTIVE)
        {
            throw new VehicleOwnerInactiveException(customerId);
        }

        var hasPlate = await _vehicleReadOnlyRepository.ExistsByPlate(plate.Value, cancellationToken);
        if (hasPlate)
        {
            throw new DuplicateVehiclePlateException(plate.Value);
        }

        var vehicle = Vehicle.Create(command.Brand, command.Model, plate, command.Year, command.VehicleType, customerId);

        await _vehicleWriteOnlyRepository.Add(vehicle, cancellationToken);
        await _workUnit.Commit();

        return _mapper.Map<VehicleResponse>(vehicle);
    }

    // A placa não é normalizada aqui: a higienização e o formato são responsabilidade do Value Object Plate.
    private CreateVehicleCommand Normalize(CreateVehicleCommand command) =>
        command with
        {
            Brand = _stringNormalizer.Normalize(command.Brand).ToUpperInvariant(),
            Model = _stringNormalizer.Normalize(command.Model).ToUpperInvariant()
        };

    private static void Validate(CreateVehicleCommand command)
    {
        var validator = new CreateVehicleValidator();

        var result = validator.Validate(command);

        if (!result.IsValid)
        {
            var errorMessages = result.Errors.Select(e => e.ErrorMessage).ToList();

            throw new ErrorOnValidationException(errorMessages);
        }
    }
}
