using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using EmployeeManagement.Domain.Entities;
using EmployeeManagement.Domain.Moderls;
using EmployeeManagement.Infrastructure.Data;
using EmployeeManagement.Infrastructure.Interfaces;
using EmployeeManagement.Infrastructure.Services.Security;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace EmployeeManagement.Infrastructure.Services.Auth
{
    public class TokenService : ITokenService
    {
        private readonly JwtSettings _jwtSettings;
        private readonly AppDbContext _appDbContext;
        private readonly IConfiguration _configuration;
        public TokenService(IOptions<JwtSettings> JwtOptions, AppDbContext appDbContext, IConfiguration configuration)
        { 
            _jwtSettings = JwtOptions.Value;
            _appDbContext = appDbContext;
            _configuration = configuration;
        }

        public async Task<AuthResponse> Login(LoginRequest loginRequest)
        {
            if (string.IsNullOrWhiteSpace(loginRequest.Username))
            {
                throw new Exception("Username is required");
            }
            if (string.IsNullOrWhiteSpace(loginRequest.Password))
            {
                throw new Exception("password is required");
            }
            var user = await _appDbContext.Users.FirstOrDefaultAsync(u => u.Username == loginRequest.Username && u.PasswordHash == AESHelper.Encrypt(loginRequest.Password));
            if (user == null)
            {
                throw new UnauthorizedAccessException();
            }

            var token = GenerateToken(user);
            var refreshToken = GenerateRefreshToken();
            user.RefreshToken = refreshToken;
            user.RefreshTokenExpiry = DateTime.UtcNow.AddDays(7);
            _appDbContext.Users.Update(user);
            await _appDbContext.SaveChangesAsync();
            return new AuthResponse
            {
                Token = token,
                RefreshToken = refreshToken,
                ExpiresIn = 60 * 60
            };
        }

        private string GenerateToken(User user)
        {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.Secret));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.Role, user.Role)
            };

            var token = new JwtSecurityToken(
                issuer: _jwtSettings.Issuer,
                audience: _jwtSettings.Audience,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(_jwtSettings.ExpiryMinutes),
                signingCredentials: creds
                );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        private string GenerateRefreshToken()
        { 
            var randomBytes = new byte[32];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomBytes);
            return Convert.ToBase64String(randomBytes);
        }

        public async Task<AuthResponse> GoogleLogin(string email)
        {
            var user = await _appDbContext.Users.FirstOrDefaultAsync(u => u.Username == email);
            var token = string.Empty;
            var refreshToken = string.Empty;
            if (user == null)
            {
                user = new User
                {
                    Id = Guid.NewGuid(),
                    Username = email,
                    PasswordHash = "",
                    Role = "ExternalUser",
                    CreatedAt = DateTime.UtcNow,

                };
                token = GenerateToken(user);
                refreshToken = GenerateRefreshToken();
                user.RefreshToken = refreshToken;
                user.RefreshTokenExpiry = DateTime.UtcNow.AddDays(7);
                _appDbContext.Users.Add(user);
                await _appDbContext.SaveChangesAsync();
            }
            else
            {
                token = GenerateToken(user);
                refreshToken = GenerateRefreshToken();
                user.RefreshToken = refreshToken;
                user.RefreshTokenExpiry = DateTime.UtcNow.AddDays(7);
                await _appDbContext.SaveChangesAsync();
            }

            return new AuthResponse
            {
                Token = token,
                RefreshToken = refreshToken,
                ExpiresIn = 60 * 60
            };
        }

        public async Task<AuthResponse> RefreshToken(RefreshTokenRequest refreshTokenRequest)
        {
            var principal = GetPrincipalFromExpiredToken(refreshTokenRequest.Token);
            if (principal == null)
                throw new Exception("Inalid access Token.");

            var username = principal.Identity.Name;
            var user = await _appDbContext.Users.FirstOrDefaultAsync(x => x.Username == username);

            if (user == null|| user.RefreshToken != refreshTokenRequest.RefreshToken || user.RefreshTokenExpiry <= DateTime.UtcNow)
                throw new Exception("Inalid refresh Token.");

            var newAccessToken = GenerateToken(user);
            var newRefreshToken = GenerateRefreshToken();
            user.RefreshToken = newRefreshToken;
            user.RefreshTokenExpiry = DateTime.UtcNow.AddDays(7);
            await _appDbContext.SaveChangesAsync();

            return new AuthResponse
            {
                Token = newAccessToken,
                RefreshToken = newRefreshToken,
                ExpiresIn = 60 * 60
            };
        }
        private ClaimsPrincipal GetPrincipalFromExpiredToken(string token)
        {
            var jwtSettings = _configuration.GetSection("JwtSettings");
            var TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateIssuerSigningKey = true,
                ValidateLifetime = false,
                ValidIssuer = jwtSettings["Issuer"],
                ValidAudience = jwtSettings["Audience"],
                IssuerSigningKey = new SymmetricSecurityKey(
                    Encoding.UTF8.GetBytes(jwtSettings["Secret"])
                )
            };
            var tokenHandler = new JwtSecurityTokenHandler();
            var principal = tokenHandler.ValidateToken(token, TokenValidationParameters, out var securityToken);
            if (securityToken is not JwtSecurityToken jwtToken || !jwtToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256))
                throw new SecurityTokenException("Invalid Token");

            return principal;
        }
    }
}
