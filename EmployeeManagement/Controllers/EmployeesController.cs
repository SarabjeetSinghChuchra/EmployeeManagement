using System.Globalization;
using System.Reflection.PortableExecutable;
using System.Security.Claims;
using CsvHelper;
using CsvHelper.Configuration;
using EmployeeManagement.Application.Interfaces;
using EmployeeManagement.Domain.Entities;
using EmployeeManagement.Domain.Models;
using EmployeeManagement.Infrastructure.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EmployeeManagement.API.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class EmployeesController : ControllerBase
    {
        private readonly IEmployeeService _employeeService;
        private readonly IBackgroundTaskQueue _taskQueue;
        public EmployeesController(IEmployeeService employeeService, IBackgroundTaskQueue taskQueue)
        {
            _employeeService = employeeService;
            _taskQueue = taskQueue;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="employeeQueryParameters"></param>
        /// <returns></returns>
        [HttpGet("employees")]
        public async Task<IActionResult> Employees([FromQuery] EmployeeQueryParameters employeeQueryParameters)
        {
            var result = await _employeeService.GetFilteredEmployeesAsync(employeeQueryParameters);
            return Ok(result);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="employee"></param>
        /// <returns></returns>
        [HttpPost("employees")]
        [Authorize(Roles ="Admin")]
        public async Task<IActionResult> AddEmployees([FromBody] EmployeeModel employee)
        {
            string userIdClaim = Convert.ToString(User.FindFirstValue(ClaimTypes.NameIdentifier));
            if (String.IsNullOrEmpty(userIdClaim))
                throw new UnauthorizedAccessException("User Id claim not found.");
            employee.CreatedBy=Guid.Parse(userIdClaim);
            employee.UpdatedBy=Guid.Parse(userIdClaim);
            var result = await _employeeService.AddEmployeeAsync(employee);
            return Ok(result);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="employee"></param>
        /// <returns></returns>
        [HttpPut("employees")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateEmployees([FromBody] EmployeeModel employee)
        {
            string userIdClaim = Convert.ToString(User.FindFirstValue(ClaimTypes.NameIdentifier));
            if (String.IsNullOrEmpty(userIdClaim))
                throw new UnauthorizedAccessException("User Id claim not found.");
            employee.UpdatedBy = Guid.Parse(userIdClaim);
            var result = await _employeeService.UpdateEmployeeAsync(employee);
            return Ok(result);
        }
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="employee"></param>
        /// <returns></returns>
        [HttpDelete("employees")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> SoftDeleteEmployees([FromQuery] Guid employeeId)
        {
            string userIdClaim = Convert.ToString(User.FindFirstValue(ClaimTypes.NameIdentifier));
            if (String.IsNullOrEmpty(userIdClaim))
                throw new UnauthorizedAccessException("User Id claim not found.");
            Guid userId = Guid.Parse(userIdClaim);
            var result = await _employeeService.SoftDeleteEmployeeAsync(employeeId, userId);
            return Ok(result);
        }

        [HttpPost("bulk-upload")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> BulkUpload(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest("No file uploaded.");
            }

            await _taskQueue.EnqueueAsync(async (cancellationToken) =>
            {
                await ProcessCsvFileAsync(file);
            });

            return Accepted("File is being processed.");
        }

        private async Task ProcessCsvFileAsync(IFormFile file)
        {
            var employees = new List<EmployeeModelBulkUpload>();
            try
            {
                var config = new CsvConfiguration(CultureInfo.InvariantCulture)
                {
                    MissingFieldFound = null,
                    HeaderValidated = null,
                    BadDataFound = null
                };

                using var reader = new StreamReader(file.OpenReadStream());
                {
                    using var csv = new CsvReader(reader, config);
                    {
                        employees = csv.GetRecords<EmployeeModelBulkUpload>().ToList();
                    }
                }
            }
            catch (Exception ex)
            {
                // Log or throw a detailed error here
                throw new InvalidOperationException("Failed to parse CSV file. Ensure headers match model properties.", ex);
            }

            await _employeeService.AddEmployeesAsync(employees);
        }

    }
}
