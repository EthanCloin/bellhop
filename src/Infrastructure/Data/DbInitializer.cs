using Microsoft.EntityFrameworkCore;

namespace Bellhop.Infrastructure.Data;

public interface IDbInitializer
{
    Task InitializeAsync();
}

public class PostgresDbInitializer(AppDbContext dbContext) : IDbInitializer
{
    public async Task InitializeAsync()
    {
        await dbContext.Database.MigrateAsync();
    }
}
