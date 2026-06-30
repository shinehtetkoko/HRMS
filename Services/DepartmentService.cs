using HRMS.Data;
using HRMS.Data.Entities;
using HRMS.Interfaces;
using Microsoft.EntityFrameworkCore;
namespace HRMS.Services
{
    public class DepartmentService : IDepartmentService
    {
        private readonly AppDbContext _context;
        public DepartmentService(AppDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Retrieves all departments
        /// </summary>
        /// <returns>
        /// A task that represents the asynchronous operation. 
        /// The task result contains an <see cref="IEnumerable{Department}"/> 
        /// representing all departments in the database.
        /// </returns>
        public async Task<IEnumerable<Department>> GetAllDepartmentsAsync()
        {
            return await _context.Departments.ToListAsync();
        }

        /// <summary>
        /// Adds a new department
        /// </summary>
        /// <param name="department">Department entity</param>
        /// <exception cref="ArgumentNullException">Thrown when the provided <paramref name="department"/> is null.</exception>
        /// <returns>A task that represents the asynchronous save operation.</returns>
        public async Task AddDepartmentAsync(Department department)
        {
            if (department == null)
            {
                throw new ArgumentNullException(nameof(department));
            }
            department.CreatedAt = DateTime.UtcNow;
            _context.Departments.Add(department);
            await _context.SaveChangesAsync();
        }

        /// <summary>
        /// Retrieves a department by ID
        /// </summary>
        /// <param name="id">Department ID</param>
        /// <returns>The found <see cref="Department"/>, or an empty instance if not found.</returns>
        public async Task<Department> GetDepartmentByIdAsync(int id)
        {
            var dept = await _context.Departments.FindAsync(id);
            return dept ?? new Department();
        }

        /// <summary>
        /// Updates an existing department
        /// </summary>
        /// <param name="department">Updated department entity</param>
        /// <exception cref="KeyNotFoundException">Thrown when no department is found with the provided ID.</exception>
        /// <returns>A task that represents the asynchronous update operation.</returns>
        public async Task UpdateDepartmentAsync(Department department)
        {
            var existingDept = await _context.Departments.FindAsync(department.DepartmentId);
            if (existingDept == null)
            {
                throw new KeyNotFoundException($"Department with ID {department.DepartmentId} not found.");
            }
            existingDept.DepartmentName = department.DepartmentName;
            existingDept.updated_at = DateTime.UtcNow;
            await _context.SaveChangesAsync();
        }

        /// <summary>
        /// Deletes a department
        /// </summary>
        /// <param name="id">Department ID</param>
        /// <returns>A task that represents the asynchronous deletion operation.</returns>
        public async Task DeleteDepartmentAsync(int id)
        {
            var dept = await _context.Departments.FindAsync(id);
            if (dept != null)
            {
                _context.Departments.Remove(dept);
                await _context.SaveChangesAsync();
            }
        }
    }
}