using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EmployeeManagement.Domain.Entities;
using EmployeeManagement.Domain.Models;

namespace EmployeeManagement.Infrastructure.Interfaces
{
    public interface IEmployeeService
    {
        Task<string> AddEmployeeAsync(EmployeeModel employeeModel);
        Task<string> UpdateEmployeeAsync(EmployeeModel employeeModel);
        Task<string> SoftDeleteEmployeeAsync(Guid EmployeeId, Guid updatedBy);
        Task<List<EmployeeModel>> GetFilteredEmployeesAsync(EmployeeQueryParameters employeeQueryParameters);

        Task AddEmployeesAsync(List<EmployeeModelBulkUpload> employees);
    }
}
