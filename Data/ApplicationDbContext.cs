using BlogApplication.Models;
using Microsoft.EntityFrameworkCore;

namespace BlogApplication.Data
{
   
        public class ApplicationDbContext : DbContext
        {
            public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
            {
            }

            public DbSet<User> Users { get; set; } = null!;
            public DbSet<BlogPost> BlogPosts { get; set; } = null!;
            public DbSet<Session> Sessions { get; set; } = null!;

            protected override void OnModelCreating(ModelBuilder modelBuilder)
            {
                modelBuilder.Entity<User>()
                    .HasIndex(u => u.Username)
                    .IsUnique();

                modelBuilder.Entity<User>()
                    .HasIndex(u => u.Email)
                    .IsUnique();

                base.OnModelCreating(modelBuilder);
            }
        }
    }

