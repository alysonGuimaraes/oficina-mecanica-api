using AutoGarage.Domain.Entities;
using AutoGarage.Domain.Enums.Employees;

namespace AutoGarage.CommonTestUtilities.Entities;

public class EmployeeBuilder
{
    public static Employee Build(string email, EmployeeRole role = EmployeeRole.MECHANIC)
    {
        return Employee.Create("Existing Employee", email, "hashed-password", role);
    }
}
