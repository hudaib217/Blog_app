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
            public DbSet<Category> Categories { get; set; } = null!;
            public DbSet<Tag> Tags { get; set; } = null!;
            public DbSet<Comment> Comments { get; set; } = null!;

            protected override void OnModelCreating(ModelBuilder modelBuilder)
            {
                modelBuilder.Entity<User>()
                    .HasIndex(u => u.Username)
                    .IsUnique();

                modelBuilder.Entity<User>()
                    .HasIndex(u => u.Email)
                    .IsUnique();

                // Configure Comment relationships to avoid cascade delete cycles
                modelBuilder.Entity<Comment>()
                    .HasOne(c => c.BlogPost)
                    .WithMany(p => p.Comments)
                    .HasForeignKey(c => c.BlogPostId)
                    .OnDelete(DeleteBehavior.Cascade);

                modelBuilder.Entity<Comment>()
                    .HasOne(c => c.Author)
                    .WithMany()
                    .HasForeignKey(c => c.AuthorId)
                    .OnDelete(DeleteBehavior.Restrict);

                // Configure BlogPost-User relationship
                modelBuilder.Entity<BlogPost>()
                    .HasOne(p => p.Author)
                    .WithMany()
                    .HasForeignKey(p => p.AuthorId)
                    .OnDelete(DeleteBehavior.Restrict);

                base.OnModelCreating(modelBuilder);
            }
        }
    }

