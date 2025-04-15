using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dapper;
using EmployeeManagement.Domain.Entities;
using EmployeeManagement.Domain.Models;
using EmployeeManagement.Infrastructure.Data;
using EmployeeManagement.Infrastructure.Interfaces;
using EmployeeManagement.Infrastructure.Services.Security;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace EmployeeManagement.Infrastructure.Services
{
    public class EmployeeService : IEmployeeService
    {
        private readonly AppDbContext _appDbContext;
        private readonly IConfiguration _configuration;
        public EmployeeService(AppDbContext appDbContext, IConfiguration configuration)
        {
            _appDbContext = appDbContext;
            _configuration = configuration;
        }
        public async Task<string> AddEmployeeAsync(EmployeeModel employeeModel)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(employeeModel.FullName))
                {
                    return "Full name is required";
                }
                if (string.IsNullOrWhiteSpace(employeeModel.Department))
                {
                    return "Department is required";
                }
                if (string.IsNullOrWhiteSpace(employeeModel.SSN))
                {
                    return "SSN is required";
                }
                var encryptedSSN = AESHelper.Encrypt(employeeModel.SSN);
                var exists = await _appDbContext.Employees.AsNoTracking().FirstOrDefaultAsync(e =>
                    e.FullName == employeeModel.FullName &&
                    e.Department == employeeModel.Department &&
                    e.EncryptedSSN == encryptedSSN
                );
                if (exists == null)
                {
                    var employee = new Employee
                    {
                        Id = Guid.NewGuid(),
                        FullName = employeeModel.FullName,
                        Department = employeeModel.Department,
                        Salary = employeeModel.Salary,
                        EncryptedSSN = encryptedSSN,
                        CreatedBy = employeeModel.CreatedBy,
                        CreatedAt = exists.CreatedAt,
                        UpdatedBy = employeeModel.UpdatedBy
                    };
                    _appDbContext.Employees.Add(employee);
                    await _appDbContext.SaveChangesAsync();
                    return "Employee created successfully";
                }
                else if (exists != null && exists.IsDeleted == true)
                {
                    return "An employee with same details already exits but is marked as deleted.";
                }
                else
                {
                    return "Am Employee with same details already exists.";
                }
            }
            catch (DbUpdateException dbEx)
            {
                return "Database error occured while saving the employee.";
            }
            catch (Exception ex)
            {
                return $"Unexpected error : {ex.Message}";
            }
        }

        public async Task<string> UpdateEmployeeAsync(EmployeeModel employeeModel)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(employeeModel.FullName))
                {
                    return "Full name is required";
                }
                if (string.IsNullOrWhiteSpace(employeeModel.Department))
                {
                    return "Department is required";
                }
                if (string.IsNullOrWhiteSpace(employeeModel.SSN))
                {
                    return "SSN is required";
                }
                var encryptedSSN = AESHelper.Encrypt(employeeModel.SSN);
                var exists = await _appDbContext.Employees.AsNoTracking().FirstOrDefaultAsync(e =>
                    e.Id == employeeModel.Id
                );
                if (exists != null)
                {
                    var employee = new Employee
                    {
                        Id = employeeModel.Id,
                        FullName = employeeModel.FullName,
                        Department = employeeModel.Department,
                        Salary = employeeModel.Salary,
                        IsDeleted = employeeModel.IsDeleted,
                        EncryptedSSN = encryptedSSN,
                        CreatedBy = exists.CreatedBy,
                        CreatedAt = exists.CreatedAt,
                        UpdatedBy = employeeModel.UpdatedBy
                    };
                    _appDbContext.Employees.Update(employee);
                    await _appDbContext.SaveChangesAsync();
                    return "Employee Updated successfully";
                }
                else
                {
                    return "An employee with same details not found.";
                }
            }
            catch (DbUpdateException dbEx)
            {
                return "Database error occured while saving the employee.";
            }
            catch (Exception ex)
            {
                return $"Unexpected error : {ex.Message}";
            }
        }

        public async Task<string> SoftDeleteEmployeeAsync(Guid EmployeeId, Guid updatedBy)
        {
            try
            { 
                if (EmployeeId==Guid.Empty)
                {
                    return "Employee id is required.";
                }
                var exists = await _appDbContext.Employees.AsNoTracking().FirstOrDefaultAsync(e =>
                    e.Id == EmployeeId
                );
                if (exists != null)
                {
                    var employee = new Employee
                    {
                        Id = EmployeeId,
                        FullName = exists.FullName,
                        Department = exists.Department,
                        Salary = exists.Salary,
                        IsDeleted = true,
                        EncryptedSSN = exists.EncryptedSSN,
                        CreatedBy = exists.CreatedBy,
                        CreatedAt = exists.CreatedAt,
                        UpdatedBy = updatedBy
                    };
                    _appDbContext.Employees.Update(employee);
                    await _appDbContext.SaveChangesAsync();
                    return "Employee Updated successfully";
                }
                else
                {
                    return "An employee with same id not found.";
                }
            }
            catch (DbUpdateException dbEx)
            {
                return "Database error occured while saving the employee.";
            }
            catch (Exception ex)
            {
                return $"Unexpected error : {ex.Message}";
            }
        }

        public async Task<List<EmployeeModel>> GetFilteredEmployeesAsync(EmployeeQueryParameters employeeQueryParameters)
        {
            var queryParameters = new DynamicParameters();

            // Handle nullable parameters correctly
            queryParameters.Add("@Name", employeeQueryParameters.Name ?? (object)DBNull.Value);
            queryParameters.Add("@Department", employeeQueryParameters.Department ?? (object)DBNull.Value);

            // Handle MinSalary and MaxSalary correctly: if null, pass DBNull.Value
            queryParameters.Add("@MinSalary", employeeQueryParameters.MinSalary ?? 0);  // Default to 0 if null
            queryParameters.Add("@MaxSalary", employeeQueryParameters.MaxSalary ?? 0);  // Default to 0 if null
            queryParameters.Add("@SortBy", employeeQueryParameters.SortBy);
            queryParameters.Add("@SortOrder", employeeQueryParameters.SortOrder);
            queryParameters.Add("@PageNumber", employeeQueryParameters.PageNumber);
            queryParameters.Add("@PageSize", employeeQueryParameters.PageSize);

            // Open a SQL connection using your connection string
            using (var connection = new SqlConnection(_configuration.GetConnectionString("DefaultConnection")))
            {
                // Open the connection asynchronously
                await connection.OpenAsync();

                // Step 1: Execute the stored procedure to get the query string
                var queryString = await connection.QueryFirstOrDefaultAsync<string>(
                    "GetFilteredEmployees",  // Stored Procedure name
                    queryParameters,         // Parameters
                    commandType: System.Data.CommandType.StoredProcedure
                );

                // If queryString is null or empty, handle it appropriately
                if (string.IsNullOrEmpty(queryString))
                {
                    throw new InvalidOperationException("The stored procedure returned an empty or null query.");
                }

                // Step 2: Execute the returned query string to get the employees
                var employees = await connection.QueryAsync<EmployeeModel>(queryString);

                // Return the list of employees
                return employees.ToList();
            }
        }

        public async Task AddEmployeesAsync(List<EmployeeModelBulkUpload> employees)
        {
            await _appDbContext.Employees.AddRangeAsync((IEnumerable<Employee>)employees);
            await _appDbContext.SaveChangesAsync();
        }

    }
}
