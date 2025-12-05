using InfoInfo2025.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace InfoInfo2025.Data
{
    public class ApplicationDbContext : IdentityDbContext<AppUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Category> Categories { get; set; }
        public DbSet<Text> Texts { get; set; }
        public DbSet<Opinion> Opinions { get; set; }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Text>()
                .HasOne(t => t.Category)
                .WithMany(c => c.Texts)
                .HasForeignKey(t => t.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Text>()
                .HasOne(t => t.Author)
                .WithMany(u => u.Texts)
                .HasForeignKey(t => t.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Opinion>()
                .HasOne(o => o.Text)
                .WithMany(t => t.Opinions)
                .HasForeignKey(o => o.TextId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Opinion>()
                .HasOne(o => o.Author)
                .WithMany(u => u.Opinions)
                .HasForeignKey(o => o.UserId)
                .OnDelete(DeleteBehavior.Restrict);

        }

    }
}
