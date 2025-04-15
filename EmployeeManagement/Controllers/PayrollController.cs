using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace EmployeeManagement.API.Controllers
{
    [Authorize(Policy = "HRPolicy")]
    [Route("api/[controller]")]
    [ApiController]
    public class PayrollController : ControllerBase
    {
        [HttpGet("details")]
        public IActionResult Get()
        {
            return Ok("Sensitive payroll data for HR only.");
            
        }
    }
}
