using BlogApi.Models;
using Microsoft.EntityFrameworkCore;

namespace BlogApi.Data;

public class BlogDbContext : DbContext
{
    public BlogDbContext(DbContextOptions<BlogDbContext> options) : base(options)
    {
    }

    public DbSet<User> Users { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasIndex(e => e.Username)
                  .IsUnique();

            entity.Property(e => e.Username)
                  .IsRequired()
                  .HasMaxLength(50);

            entity.Property(e => e.TotpSecret)
                  .IsRequired();

            entity.Property(e => e.CreatedAt)
                  .HasColumnType("timestamp with time zone")
                  .HasDefaultValueSql("CURRENT_TIMESTAMP");

            entity.Property(e => e.UpdatedAt)
                  .HasColumnType("timestamp with time zone")
                  .HasDefaultValueSql("CURRENT_TIMESTAMP");
        });
    }
}
