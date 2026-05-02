using System.Security.Claims;
using Bellhop.Infrastructure.Data;
using Bellhop.Infrastructure.Security;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Bellhop.Features.Auth.Token;

public static class TokenEndpoints
{
    public record LoginRequest(string Username, string Password);
    public record TokenResponse(string AccessToken, string RefreshToken);
    public record RefreshRequest(string RefreshToken);
    public record UserResponse(Guid Id, string Username);

    public static void MapTokenEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v1/auth/token");

        group.MapPost("/login", async (
            [FromBody] LoginRequest request,
            AppDbContext dbContext,
            IPasswordHasher passwordHasher,
            ITokenService tokenService) =>
        {
            var user = await dbContext.Users.SingleOrDefaultAsync(u => u.Username == request.Username);
            if (user == null || !passwordHasher.VerifyPassword(request.Password, user.PasswordHash))
            {
                return Results.Unauthorized();
            }

            var accessToken = tokenService.GenerateAccessToken(user);
            var refreshTokenString = tokenService.GenerateRefreshToken();

            var refreshToken = new RefreshToken
            {
                Id = Guid.NewGuid(),
                UserId = user.Id,
                Token = refreshTokenString,
                ExpiresAt = DateTime.UtcNow.AddDays(7),
                IsRevoked = false,
                CreatedAt = DateTime.UtcNow
            };

            dbContext.RefreshTokens.Add(refreshToken);
            await dbContext.SaveChangesAsync();

            return Results.Ok(new TokenResponse(accessToken, refreshTokenString));
        });

        group.MapPost("/refresh", async (
            [FromBody] RefreshRequest request,
            AppDbContext dbContext,
            ITokenService tokenService) =>
        {
            var refreshToken = await dbContext.RefreshTokens
                .Include(t => t.User)
                .SingleOrDefaultAsync(t => t.Token == request.RefreshToken);

            if (refreshToken == null || refreshToken.IsRevoked || refreshToken.ExpiresAt < DateTime.UtcNow)
            {
                return Results.Unauthorized();
            }

            // Rotate refresh token
            refreshToken.IsRevoked = true;
            
            var newAccessToken = tokenService.GenerateAccessToken(refreshToken.User);
            var newRefreshTokenString = tokenService.GenerateRefreshToken();

            var newRefreshToken = new RefreshToken
            {
                Id = Guid.NewGuid(),
                UserId = refreshToken.UserId,
                Token = newRefreshTokenString,
                ExpiresAt = DateTime.UtcNow.AddDays(7),
                IsRevoked = false,
                CreatedAt = DateTime.UtcNow
            };

            dbContext.RefreshTokens.Add(newRefreshToken);
            await dbContext.SaveChangesAsync();

            return Results.Ok(new TokenResponse(newAccessToken, newRefreshTokenString));
        });

        group.MapGet("/me", (HttpContext httpContext) =>
        {
            var userId = httpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
            var username = httpContext.User.FindFirstValue(ClaimTypes.Name);

            if (userId == null || username == null)
            {
                return Results.Unauthorized();
            }

            return Results.Ok(new UserResponse(Guid.Parse(userId), username));
        }).RequireAuthorization("TokenAuthPolicy");
    }
}
