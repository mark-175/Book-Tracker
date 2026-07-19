using BookTracker.Api.Entities;
using Microsoft.EntityFrameworkCore;

namespace BookTracker.Api.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();
    public DbSet<Book> Books => Set<Book>();
    public DbSet<UserBook> UserBooks => Set<UserBook>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasIndex(u => u.Username).IsUnique();
        });

        modelBuilder.Entity<Book>(entity =>
        {
            entity.HasIndex(b => b.GoogleBooksId).IsUnique();
        });
        modelBuilder.Entity<UserBook>(entity =>
        {
            entity.HasOne(ub => ub.User)
                  .WithMany(u => u.UserBooks)
                  .HasForeignKey(ub => ub.UserId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(ub => ub.Book)
                  .WithMany(b => b.UserBooks)
                  .HasForeignKey(ub => ub.BookId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(ub => new { ub.UserId, ub.BookId })
                  .IsUnique()
                  .HasFilter("\"IsDeleted\" = 0");

            entity.HasQueryFilter(ub => !ub.IsDeleted);

        });
    }
}