using System.Security.Claims;
using EmployeeManagement.Infrastructure.Interfaces;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;

namespace EmployeeManagement.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ExternalAuthController : ControllerBase
    {
        private readonly ITokenService _tokenService;

        public ExternalAuthController(ITokenService tokenService)
        {
            _tokenService = tokenService;
        }

        [HttpGet("signin-google")]
        public async Task<IActionResult> GoogleLogin()
        {
            var result = await HttpContext.AuthenticateAsync("Google");

            if (!result.Succeeded || result.Principal == null)
                return Unauthorized();

            var email = Convert.ToString(result.Principal.FindFirst(ClaimTypes.Email)?.Value);

            var response = await _tokenService.GoogleLogin(email);

            return Ok(response);
        }
    }
}
