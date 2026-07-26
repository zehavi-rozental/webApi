using Microsoft.EntityFrameworkCore;
using MyMiddleware.Models;

namespace MyMiddleware.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<IceCream> IceCreams { get; set; }
        public DbSet<User> Users { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure IceCream
            modelBuilder.Entity<IceCream>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id)
                    .ValueGeneratedOnAdd();
                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(256);
                entity.Property(e => e.UserId)
                    .IsRequired()
                    .HasMaxLength(256);
            });

            // Configure User
            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id)
                    .ValueGeneratedOnAdd();
                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(256);
                entity.Property(e => e.Password)
                    .IsRequired()
                    .HasMaxLength(256);
                entity.Property(e => e.Role)
                    .IsRequired()
                    .HasMaxLength(50)
                    .HasDefaultValue("User");
            });
        }
    }
}
