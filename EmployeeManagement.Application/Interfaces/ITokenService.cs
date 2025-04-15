using EmployeeManagement.Domain.Entities;
using EmployeeManagement.Domain.Moderls;

namespace EmployeeManagement.Infrastructure.Interfaces
{
    public interface ITokenService
    {
        Task<AuthResponse> Login(LoginRequest loginRequest);
        Task<AuthResponse> GoogleLogin(string email);
        Task<AuthResponse> RefreshToken(RefreshTokenRequest refreshTokenRequest);
    }
}
