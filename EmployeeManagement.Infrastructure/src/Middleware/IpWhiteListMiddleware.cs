using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Azure.Core;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;

namespace EmployeeManagement.Infrastructure.Middleware
{
    public class IpWhiteListMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IConfiguration _configuration;
        public IpWhiteListMiddleware(RequestDelegate next, IConfiguration configuration) 
        {
            _next = next;
            _configuration = configuration;
        }

        public async Task InvokeAsync(HttpContext httpContext)
        { 
            var clientIp = httpContext.Connection.RemoteIpAddress?.ToString();
            var allowedIps = _configuration.GetSection("IpWhitelist").Get<string[]>();
            if (allowedIps !=null && allowedIps.Any()&& !allowedIps.Contains(clientIp))
            {
                httpContext.Response.StatusCode = StatusCodes.Status403Forbidden;
                await httpContext.Response.WriteAsync("Forbidden: IP address not allowed.");
                return;
            }
            await _next(httpContext);
        }
    }
}
