using Microsoft.EntityFrameworkCore;
using SocialSim.Core.Models;

namespace SocialSim.Api.Data;

public class SocialSimDbContext : DbContext
{
    public SocialSimDbContext(DbContextOptions<SocialSimDbContext> options)
        : base(options)
    {
    }

    public DbSet<SocialAgent> Agents { get; set; } = null!;
    public DbSet<Post> Posts { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<SocialAgent>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.Username).IsUnique();
            entity.HasIndex(e => e.DecentralizedId);
            entity.HasIndex(e => e.Handle);
            entity.OwnsOne(e => e.Behavior);
        });

        modelBuilder.Entity<Post>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.AuthorId);
            entity.HasIndex(e => e.CreatedAt);
            entity.HasIndex(e => e.RecordKey);
        });
    }
}
