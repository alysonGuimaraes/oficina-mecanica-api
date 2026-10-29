using AutoGarage.Application.Commands.Employees;
using AutoGarage.Domain.Enums.Employees;
using Bogus;

namespace AutoGarage.CommonTestUtilities.Commands.Employees;

public class CreateEmployeeCommandBuilder
{
    private string _name;
    private string _email;
    private string _password;
    private EmployeeRole _role;

    public CreateEmployeeCommandBuilder()
    {
        var faker = new Faker();

        _name = faker.Name.FirstName();
        _email = faker.Internet.Email();
        _password = faker.Internet.Password(10);
        _role = EmployeeRole.MECHANIC;
    }

    public CreateEmployeeCommandBuilder WithName(string name)
    {
        _name = name;
        return this;
    }

    public CreateEmployeeCommandBuilder WithEmail(string email)
    {
        _email = email;
        return this;
    }

    public CreateEmployeeCommandBuilder WithPassword(string password)
    {
        _password = password;
        return this;
    }

    public CreateEmployeeCommandBuilder WithRole(EmployeeRole role)
    {
        _role = role;
        return this;
    }

    public CreateEmployeeCommand Build() => new(_name, _email, _password, _role);
}
