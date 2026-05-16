using Bellhop.Infrastructure.Data;

namespace Bellhop.IntegrationTests;

public class TestDbInitializer(AppDbContext dbContext) : IDbInitializer
{
    public async Task InitializeAsync()
    {
        await dbContext.Database.EnsureCreatedAsync();
    }
}
