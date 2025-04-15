using EmployeeManagement.Domain.Moderls;
using EmployeeManagement.Infrastructure.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace EmployeeManagement.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly ITokenService _tokenService;
        public AuthController(ITokenService tokenService) 
        {
            _tokenService = tokenService;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest loginRequest)
        { 
            var result = await _tokenService.Login(loginRequest);  
            return Ok(result);
        }
        
        [HttpPost("refersh-token")]
        public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequest refreshTokenRequest)
        { 
            var result = await _tokenService.RefreshToken(refreshTokenRequest);  
            return Ok(result);
        }
    }
}
