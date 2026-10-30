using AutoGarage.Application.Commands.Auth;
using AutoGarage.Application.Commands.Customers;
using AutoGarage.Application.Commands.Employees;
using AutoGarage.Application.Commands.Materials;
using AutoGarage.Application.Commands.Services;
using AutoGarage.Application.Commands.Vehicles;
using AutoGarage.CommonTestUtilities.Mapper;
using AutoGarage.Communication.Requests.Auth;
using AutoGarage.Communication.Requests.Customers;
using AutoGarage.Communication.Requests.Employees;
using AutoGarage.Communication.Requests.Materials;
using AutoGarage.Communication.Requests.Services;
using AutoGarage.Communication.Requests.Vehicles;
using AutoGarage.Domain.Enums.Employees;
using AutoGarage.Domain.Enums.Vehicles;
using MapsterMapper;
using Shouldly;

namespace AutoGarage.Application.UnitTests.Services.Mapping;

public class RequestToCommandMappingTests
{
    private readonly IMapper _mapper = MapperBuilder.Build();

    [Fact]
    public void Map_LoginRequest_ShouldCreateLoginCommandWithSameValues()
    {
        var request = new LoginRequest("john@autogarage.com", "senha123");

        var command = _mapper.Map<LoginCommand>(request);

        command.Email.ShouldBe("john@autogarage.com");
        command.Password.ShouldBe("senha123");
    }

    [Fact]
    public void Map_CreateCustomerRequest_ShouldKeepValuesUnchanged()
    {
        var request = new CreateCustomerRequest("  John  ", "529.982.247-25", "John@Autogarage.com", "41", "999999999");

        var command = _mapper.Map<CreateCustomerCommand>(request);

        command.Name.ShouldBe("  John  ");
        command.Document.ShouldBe("529.982.247-25");
        command.Email.ShouldBe("John@Autogarage.com");
        command.Ddd.ShouldBe("41");
        command.Phone.ShouldBe("999999999");
    }

    [Fact]
    public void Map_CreateVehicleRequest_ShouldKeepEnumAndValues()
    {
        var customerId = Guid.NewGuid().ToString();
        var request = new CreateVehicleRequest("gol", "1.0", "abc-1d23", 2020, VehicleType.MOTORCYCLE, customerId);

        var command = _mapper.Map<CreateVehicleCommand>(request);

        command.Brand.ShouldBe("gol");
        command.Model.ShouldBe("1.0");
        command.Plate.ShouldBe("abc-1d23");
        command.Year.ShouldBe(2020);
        command.VehicleType.ShouldBe(VehicleType.MOTORCYCLE);
        command.CustomerId.ShouldBe(customerId);
    }

    [Fact]
    public void Map_CreateServiceRequest_ShouldKeepValuesUnchanged()
    {
        var request = new CreateServiceRequest("  Troca  ", "  Com filtro  ", 150.50m);

        var command = _mapper.Map<CreateServiceCommand>(request);

        command.Name.ShouldBe("  Troca  ");
        command.Description.ShouldBe("  Com filtro  ");
        command.Price.ShouldBe(150.50m);
    }

    [Fact]
    public void Map_CreateEmployeeRequest_ShouldKeepRoleAndPassword()
    {
        var request = new CreateEmployeeRequest("Jane", "jane@autogarage.com", "senha123", EmployeeRole.ATTENDANT);

        var command = _mapper.Map<CreateEmployeeCommand>(request);

        command.Name.ShouldBe("Jane");
        command.Email.ShouldBe("jane@autogarage.com");
        command.Password.ShouldBe("senha123");
        command.Role.ShouldBe(EmployeeRole.ATTENDANT);
    }

    [Fact]
    public void Map_CreateMaterialRequest_ShouldKeepValuesUnchanged()
    {
        var request = new CreateMaterialRequest("  Parafuso  ", "  Aco  ", 2.50m, 3);

        var command = _mapper.Map<CreateMaterialCommand>(request);

        command.Name.ShouldBe("  Parafuso  ");
        command.Description.ShouldBe("  Aco  ");
        command.Price.ShouldBe(2.50m);
        command.StockMinimum.ShouldBe(3);
    }
}
