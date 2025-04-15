using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Distributed;

namespace EmployeeManagement.Infrastructure.Middleware
{
    public class RateLimitingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IDistributedCache _cache;
        private readonly int _limit;
        private readonly TimeSpan _timeWindow;

        public RateLimitingMiddleware(RequestDelegate next, IDistributedCache cache, int limit = 100, int windowInSeconds = 60)
        {
            _next = next;
            _cache = cache;
            _limit = limit;
            _timeWindow = TimeSpan.FromSeconds(windowInSeconds);
        }

        public async Task InvokeAsync(HttpContext context)
        { 
            var clientIp = context.Connection.RemoteIpAddress.ToString();

            if (string.IsNullOrEmpty(clientIp))
            { 
                await _next(context);
                return;
            }
            var rateLimitKey = $"RateLimit_{clientIp}";
            var requestCount = await _cache.GetStringAsync(rateLimitKey);
            if (!string.IsNullOrEmpty(requestCount) && int.Parse(requestCount) >= _limit)
            {
                context.Response.StatusCode = StatusCodes.Status429TooManyRequests;
                await context.Response.WriteAsync("Too many requests. Please try again later.");
                return;
            }

            var newRequestCount = requestCount == null ? 1:int.Parse(requestCount) + 1;

            var options = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = _timeWindow
            };
            await _cache.SetStringAsync(rateLimitKey, newRequestCount.ToString(), options);

            await _next(context);
        }

    }
}
