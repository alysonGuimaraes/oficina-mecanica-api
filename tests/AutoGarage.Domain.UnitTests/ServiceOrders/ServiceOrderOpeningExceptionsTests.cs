using AutoGarage.Domain.Exceptions.ServiceOrders.Exceptions;
using Shouldly;

namespace AutoGarage.Domain.UnitTests.ServiceOrders;

public class ServiceOrderOpeningExceptionsTests
{
    [Fact]
    public void CustomerInactive_ShouldCarryErrorTypeAndCustomerId()
    {
        var customerId = Guid.NewGuid();

        var exception = new ServiceOrderCustomerInactiveException(customerId);

        exception.ErrorType.ShouldBe("service-order-customer-inactive");
        exception.Message.ShouldContain(customerId.ToString());
    }

    [Fact]
    public void VehicleInactive_ShouldCarryErrorTypeAndVehicleId()
    {
        var vehicleId = Guid.NewGuid();

        var exception = new ServiceOrderVehicleInactiveException(vehicleId);

        exception.ErrorType.ShouldBe("service-order-vehicle-inactive");
        exception.Message.ShouldContain(vehicleId.ToString());
    }

    [Fact]
    public void VehicleCustomerMismatch_ShouldCarryErrorTypeAndBothIds()
    {
        var vehicleId = Guid.NewGuid();
        var customerId = Guid.NewGuid();

        var exception = new ServiceOrderVehicleCustomerMismatchException(vehicleId, customerId);

        exception.ErrorType.ShouldBe("service-order-vehicle-customer-mismatch");
        exception.Message.ShouldContain(vehicleId.ToString());
        exception.Message.ShouldContain(customerId.ToString());
    }

    [Fact]
    public void EmployeeInactive_ShouldCarryErrorTypeAndEmployeeId()
    {
        var employeeId = Guid.NewGuid();

        var exception = new ServiceOrderEmployeeInactiveException(employeeId);

        exception.ErrorType.ShouldBe("service-order-employee-inactive");
        exception.Message.ShouldContain(employeeId.ToString());
    }
}
