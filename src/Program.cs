using System.Text;
using Bellhop.Features.Auth.Session;
using Bellhop.Features.Auth.Token;
using Bellhop.Features.Users;
using Bellhop.Infrastructure.Data;
using Bellhop.Infrastructure.Security;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
if (builder.Environment.EnvironmentName != "Testing")
{
    builder.Services.AddDbContext<AppDbContext>(options =>
        options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));
}

builder.Services.AddScoped<IDbInitializer, PostgresDbInitializer>();
builder.Services.AddSingleton<IPasswordHasher, PasswordHasher>();
builder.Services.AddScoped<ITokenService, TokenService>();

builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = "SessionAuth";
    options.DefaultChallengeScheme = "SessionAuth";
})
    .AddCookie("SessionAuth", options =>
    {
        options.Cookie.Name = "Bellhop.Session";
        options.Cookie.HttpOnly = true;
        options.Cookie.SameSite = SameSiteMode.Strict;
        options.Events.OnRedirectToLogin = context =>
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            return Task.CompletedTask;
        };
    })
    .AddJwtBearer("TokenAuth", options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!))
        };
    });

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("TokenAuthPolicy", policy =>
    {
        policy.AuthenticationSchemes.Add("TokenAuth");
        policy.RequireAuthenticatedUser();
    });

    options.AddPolicy("SessionAuthPolicy", policy =>
    {
        policy.AuthenticationSchemes.Add("SessionAuth");
        policy.RequireAuthenticatedUser();
    });
});

builder.Services.AddOpenApi();

var app = builder.Build();

// Initialize database
using (var scope = app.Services.CreateScope())
{
    var initializer = scope.ServiceProvider.GetRequiredService<IDbInitializer>();
    await initializer.InitializeAsync();
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}
else
{
    app.UseHttpsRedirection();
}

app.UseAuthentication();
app.UseAuthorization();

app.MapRegisterEndpoint();
app.MapGetUsersEndpoint();
app.MapSessionEndpoints();
app.MapTokenEndpoints();
app.Run();

public partial class Program { }
