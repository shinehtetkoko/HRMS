using HRMS.Data.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HRMS.Interfaces
{
    public interface IDepartmentService
    {
        Task<IEnumerable<Department>> GetAllDepartmentsAsync();
        Task AddDepartmentAsync(Department department);
        Task<Department> GetDepartmentByIdAsync(int id);
        Task UpdateDepartmentAsync(Department department);
        Task DeleteDepartmentAsync(int id);
    }
}