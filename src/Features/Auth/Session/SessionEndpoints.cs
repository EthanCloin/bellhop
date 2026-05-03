using System.Security.Claims;
using Bellhop.Infrastructure.Data;
using Bellhop.Infrastructure.Security;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Bellhop.Features.Auth.Session;

public static class SessionEndpoints
{
    public record LoginRequest(string Username, string Password);
    public record UserResponse(Guid Id, string Username);

    public static void MapSessionEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v1/auth/session");

        group.MapPost("/login", async (
            [FromBody] LoginRequest request,
            AppDbContext dbContext,
            IPasswordHasher passwordHasher,
            HttpContext httpContext) =>
        {
            var user = await dbContext.Users.SingleOrDefaultAsync(u => u.Username == request.Username);
            if (user == null || !passwordHasher.VerifyPassword(request.Password, user.PasswordHash))
            {
                return Results.Unauthorized();
            }

            var sessionToken = Guid.NewGuid().ToString("N");
            var session = new Session
            {
                Id = Guid.NewGuid(),
                UserId = user.Id,
                SessionToken = sessionToken,
                ExpiresAt = DateTime.UtcNow.AddDays(7),
                IsRevoked = false,
                CreatedAt = DateTime.UtcNow
            };

            dbContext.Sessions.Add(session);
            await dbContext.SaveChangesAsync();

            var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new(ClaimTypes.Name, user.Username),
                new("SessionToken", sessionToken)
            };

            var claimsIdentity = new ClaimsIdentity(claims, "SessionAuth");
            var authProperties = new AuthenticationProperties
            {
                IsPersistent = true,
                ExpiresUtc = session.ExpiresAt
            };

            await httpContext.SignInAsync("SessionAuth", new ClaimsPrincipal(claimsIdentity), authProperties);

            return Results.Ok(new UserResponse(user.Id, user.Username));
        });

        group.MapPost("/logout", async (
            AppDbContext dbContext,
            HttpContext httpContext) =>
        {
            var sessionToken = httpContext.User.FindFirstValue("SessionToken");
            if (sessionToken != null)
            {
                var session = await dbContext.Sessions.SingleOrDefaultAsync(s => s.SessionToken == sessionToken);
                if (session != null)
                {
                    session.IsRevoked = true;
                    await dbContext.SaveChangesAsync();
                }
            }

            await httpContext.SignOutAsync("SessionAuth");
            return Results.Ok();
        }).RequireAuthorization("SessionAuthPolicy");

        group.MapGet("/me", (HttpContext httpContext) =>
        {
            var userId = httpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
            var username = httpContext.User.FindFirstValue(ClaimTypes.Name);

            if (userId == null || username == null)
            {
                return Results.Unauthorized();
            }

            return Results.Ok(new UserResponse(Guid.Parse(userId), username));
        }).RequireAuthorization("SessionAuthPolicy");
    }
}
