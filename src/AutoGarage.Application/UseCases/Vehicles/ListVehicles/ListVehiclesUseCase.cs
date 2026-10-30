using AutoGarage.Application.Services.Status;
using AutoGarage.Communication.Responses.Vehicles;
using AutoGarage.Domain.Exceptions.Vehicles.Exceptions;
using AutoGarage.Domain.Repositories.Vehicles;
using AutoGarage.Domain.Shared;
using MapsterMapper;

namespace AutoGarage.Application.UseCases.Vehicles.ListVehicles;

public class ListVehiclesUseCase : IListVehiclesUseCase
{
    private readonly IVehicleReadOnlyRepository _vehicleReadOnlyRepository;
    private readonly IMapper _mapper;

    public ListVehiclesUseCase(IVehicleReadOnlyRepository vehicleReadOnlyRepository, IMapper mapper)
    {
        _vehicleReadOnlyRepository = vehicleReadOnlyRepository;
        _mapper = mapper;
    }

    public async Task<IReadOnlyList<VehicleResponse>> Handle(string? status, CancellationToken cancellationToken)
    {
        var filter = EntityStatus.ACTIVE;

        if (!string.IsNullOrWhiteSpace(status) && !EntityStatusParser.TryParse(status, out filter))
        {
            throw new InvalidVehicleStatusException();
        }

        var vehicles = await _vehicleReadOnlyRepository.GetByStatus(filter, cancellationToken);

        return _mapper.Map<List<VehicleResponse>>(vehicles);
    }
}
