using System.Text;
using EmployeeManagement.Application.Interfaces;
using EmployeeManagement.Domain.Moderls;
using EmployeeManagement.Infrastructure.Data;
using EmployeeManagement.Infrastructure.Interfaces;
using EmployeeManagement.Infrastructure.Middleware;
using EmployeeManagement.Infrastructure.Services;
using EmployeeManagement.Infrastructure.Services.Auth;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"),
        b => b.MigrationsAssembly("EmployeeManagement.API")
        ));
var jwtSettings = builder.Configuration.GetSection("JwtSettings");
builder.Services.Configure<JwtSettings>(jwtSettings);
builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddScoped<IEmployeeService, EmployeeService>();
builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = GoogleDefaults.AuthenticationScheme;
})
.AddJwtBearer("Bearer", options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateIssuerSigningKey = true,
        ValidateLifetime = true,
        ValidIssuer = jwtSettings["Issuer"],
        ValidAudience = jwtSettings["Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(jwtSettings["Secret"])
        )
    };
})
.AddGoogle(GoogleDefaults.AuthenticationScheme, options =>
{
    options.ClientId = builder.Configuration["GoogleOAuth:ClientId"];
    options.ClientSecret = builder.Configuration["GoogleOAuth:ClientSecret"];
    options.CallbackPath = "/signin-ggogle";

    options.Events.OnCreatingTicket = ctx =>
    {
        return Task.CompletedTask;
    };
});

builder.Services.AddSingleton<IBackgroundTaskQueue, BackgroundTaskQueue>();
builder.Services.AddHostedService<BackgroundWorkerService>();
builder.Services.AddScoped<IEmployeeService, EmployeeService>();

builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "EmployeeManagement API", Version = "v1" });

    // ✅ Add this block for JWT auth
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Enter 'Bearer {your token}'"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] {}
        }
    });
});
builder.Services.AddDistributedMemoryCache();
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("HRPolicy", policy =>
        policy.RequireRole("HR"));
});
var app = builder.Build();


// Configure the HTTP request pipeline.
app.UseMiddleware<IpWhiteListMiddleware>();
app.UseMiddleware<RateLimitingMiddleware>();
app.UseHttpsRedirection();
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "EmployeeManagement API v1");
    });
}
app.UseAuthorization();

app.MapControllers();

app.Run();
