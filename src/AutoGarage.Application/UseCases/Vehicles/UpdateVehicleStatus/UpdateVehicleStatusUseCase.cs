using AutoGarage.Application.Commands.Vehicles;
using AutoGarage.Application.Services.Status;
using AutoGarage.Communication.Responses.Vehicles;
using AutoGarage.Domain.Exceptions.Vehicles.Exceptions;
using AutoGarage.Domain.Repositories;
using AutoGarage.Domain.Repositories.Vehicles;
using AutoGarage.Domain.Shared;
using MapsterMapper;

namespace AutoGarage.Application.UseCases.Vehicles.UpdateVehicleStatus;

public class UpdateVehicleStatusUseCase : IUpdateVehicleStatusUseCase
{
    private readonly IVehicleReadOnlyRepository _vehicleReadOnlyRepository;
    private readonly IVehicleWriteOnlyRepository _vehicleWriteOnlyRepository;
    private readonly IUnitWork _workUnit;
    private readonly IMapper _mapper;

    public UpdateVehicleStatusUseCase(IVehicleReadOnlyRepository vehicleReadOnlyRepository, IVehicleWriteOnlyRepository vehicleWriteOnlyRepository, IUnitWork workUnit, IMapper mapper)
    {
        _vehicleReadOnlyRepository = vehicleReadOnlyRepository;
        _vehicleWriteOnlyRepository = vehicleWriteOnlyRepository;
        _workUnit = workUnit;
        _mapper = mapper;
    }

    public async Task<VehicleResponse> Handle(UpdateVehicleStatusCommand command, CancellationToken cancellationToken)
    {
        if (!Guid.TryParse(command.Id, out var vehicleId))
        {
            throw new InvalidVehicleIdException();
        }

        if (!EntityStatusParser.TryParse(command.Status, out var status))
        {
            throw new InvalidVehicleStatusException();
        }

        var vehicle = await _vehicleReadOnlyRepository.GetById(vehicleId, cancellationToken)
            ?? throw new VehicleNotFoundException(vehicleId);

        if (status == EntityStatus.ACTIVE)
        {
            vehicle.Activate();
        }
        else
        {
            vehicle.Deactivate();
        }

        await _vehicleWriteOnlyRepository.Update(vehicle, cancellationToken);
        await _workUnit.Commit();

        return _mapper.Map<VehicleResponse>(vehicle);
    }
}
