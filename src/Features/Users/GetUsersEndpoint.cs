using Bellhop.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Bellhop.Features.Users;

public static class GetUsersEndpoint
{
    public record UserDto(Guid Id, string Username, DateTime CreatedAt);
    public record PagedResponse<T>(IEnumerable<T> Items, int TotalCount, int Page, int PageSize);

    public static void MapGetUsersEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapGet("/api/v1/users", async (
            AppDbContext dbContext,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10) =>
        {
            if (page < 1) page = 1;
            if (pageSize < 1) pageSize = 10;
            if (pageSize > 100) pageSize = 100;

            var totalCount = await dbContext.Users.CountAsync();

            var users = await dbContext.Users
                .OrderByDescending(u => u.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(u => new UserDto(u.Id, u.Username, u.CreatedAt))
                .ToListAsync();

            return Results.Ok(new PagedResponse<UserDto>(users, totalCount, page, pageSize));
        })
        .WithName("GetUsers")
        ;
    }
}
