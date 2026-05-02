using Bellhop.Features.Identity;
using Bellhop.Infrastructure.Data;
using Bellhop.Infrastructure.Security;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Bellhop.Features.Users;

public static class RegisterEndpoint
{
    public record RegisterRequest(string Username, string Password);
    public record RegisterResponse(Guid Id, string Username);

    public static void MapRegisterEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapPost("/api/v1/users/register", async (
            [FromBody] RegisterRequest request,
            AppDbContext dbContext,
            IPasswordHasher passwordHasher) =>
        {
            if (await dbContext.Users.AnyAsync(u => u.Username == request.Username))
            {
                return Results.BadRequest("Username already exists.");
            }

            var user = new User
            {
                Id = Guid.NewGuid(),
                Username = request.Username,
                PasswordHash = passwordHasher.HashPassword(request.Password),
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            dbContext.Users.Add(user);
            await dbContext.SaveChangesAsync();

            return Results.Ok(new RegisterResponse(user.Id, user.Username));
        })
        .WithName("RegisterUser");
    }
}
