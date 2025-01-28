using ShiftManager.Models;
using ShiftManager.Models.ViewModels;

namespace ShiftManager.Services.Interfaces
{
    public interface IEmployeeRepository
    {
        Task<Employee> GetEmployeeByIdAsync(int? id);
        Task<IEnumerable<Employee>> GetAllEmployeesAsync();
        Task AddAsync(EmployeeVM employee);
        Task UpdateAsync(EmployeeVM updatedEmployee);
        Task DeleteAsync(int? id);
    }
}
