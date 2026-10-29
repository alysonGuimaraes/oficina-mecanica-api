using AutoGarage.Communication.Responses.Employees;
using AutoGarage.Domain.Repositories.Employees;
using MapsterMapper;

namespace AutoGarage.Application.UseCases.Employees.ListEmployees;

public class ListEmployeesUseCase : IListEmployeesUseCase
{
    private readonly IEmployeeReadOnlyRepository _employeeReadOnlyRepository;
    private readonly IMapper _mapper;

    public ListEmployeesUseCase(IEmployeeReadOnlyRepository employeeReadOnlyRepository, IMapper mapper)
    {
        _employeeReadOnlyRepository = employeeReadOnlyRepository;
        _mapper = mapper;
    }

    public async Task<IReadOnlyList<EmployeeResponse>> Handle(CancellationToken cancellationToken)
    {
        var employees = await _employeeReadOnlyRepository.GetAll(cancellationToken);

        return _mapper.Map<List<EmployeeResponse>>(employees);
    }
}
