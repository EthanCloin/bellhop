using Microsoft.EntityFrameworkCore;
using Bellhop.Features.Identity;
using Bellhop.Features.Auth.Session;
using Bellhop.Features.Auth.Token;

namespace Bellhop.Infrastructure.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();
    public DbSet<Session> Sessions => Set<Session>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasIndex(u => u.Username).IsUnique();
        });

        modelBuilder.Entity<Session>(entity =>
        {
            entity.HasIndex(s => s.SessionToken).IsUnique();
            entity.HasOne(s => s.User)
                  .WithMany()
                  .HasForeignKey(s => s.UserId);
        });

        modelBuilder.Entity<RefreshToken>(entity =>
        {
            entity.HasIndex(t => t.Token).IsUnique();
            entity.HasOne(t => t.User)
                  .WithMany()
                  .HasForeignKey(t => t.UserId);
        });
    }
}
