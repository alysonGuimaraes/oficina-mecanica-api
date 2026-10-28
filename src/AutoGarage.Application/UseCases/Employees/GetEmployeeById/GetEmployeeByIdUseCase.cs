using AutoGarage.Communication.Responses.Employees;
using AutoGarage.Domain.Exceptions.Employees.Exceptions;
using AutoGarage.Domain.Repositories.Employees;
using MapsterMapper;

namespace AutoGarage.Application.UseCases.Employees.GetEmployeeById;

public class GetEmployeeByIdUseCase : IGetEmployeeByIdUseCase
{
    private readonly IEmployeeReadOnlyRepository _employeeReadOnlyRepository;
    private readonly IMapper _mapper;

    public GetEmployeeByIdUseCase(IEmployeeReadOnlyRepository employeeReadOnlyRepository, IMapper mapper)
    {
        _employeeReadOnlyRepository = employeeReadOnlyRepository;
        _mapper = mapper;
    }

    public async Task<EmployeeResponse> Handle(string id, CancellationToken cancellationToken)
    {
        if (!Guid.TryParse(id, out var employeeId))
        {
            throw new InvalidEmployeeIdException();
        }

        var employee = await _employeeReadOnlyRepository.GetById(employeeId, cancellationToken)
            ?? throw new EmployeeNotFoundException(employeeId);

        return _mapper.Map<EmployeeResponse>(employee);
    }
}
