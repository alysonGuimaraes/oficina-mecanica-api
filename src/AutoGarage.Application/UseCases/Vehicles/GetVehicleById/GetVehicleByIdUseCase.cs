using AutoGarage.Communication.Responses.Vehicles;
using AutoGarage.Domain.Exceptions.Vehicles.Exceptions;
using AutoGarage.Domain.Repositories.Vehicles;
using MapsterMapper;

namespace AutoGarage.Application.UseCases.Vehicles.GetVehicleById;

public class GetVehicleByIdUseCase : IGetVehicleByIdUseCase
{
    private readonly IVehicleReadOnlyRepository _vehicleReadOnlyRepository;
    private readonly IMapper _mapper;

    public GetVehicleByIdUseCase(IVehicleReadOnlyRepository vehicleReadOnlyRepository, IMapper mapper)
    {
        _vehicleReadOnlyRepository = vehicleReadOnlyRepository;
        _mapper = mapper;
    }

    public async Task<VehicleResponse> Handle(string id, CancellationToken cancellationToken)
    {
        if (!Guid.TryParse(id, out var vehicleId))
        {
            throw new InvalidVehicleIdException();
        }

        var vehicle = await _vehicleReadOnlyRepository.GetById(vehicleId, cancellationToken)
            ?? throw new VehicleNotFoundException(vehicleId);

        return _mapper.Map<VehicleResponse>(vehicle);
    }
}
