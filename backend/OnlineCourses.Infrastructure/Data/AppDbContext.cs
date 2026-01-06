using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using OnlineCourses.Domain.Entities;
using OnlineCourses.Infrastructure.Identity;

namespace OnlineCourses.Infrastructure.Data
{
    public class AppDbContext : IdentityDbContext<AppUser>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<Course> Courses { get; set; }
        public DbSet<Lesson> Lessons { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Global Query Filter for Soft Delete
            modelBuilder.Entity<Course>().HasQueryFilter(c => !c.IsDeleted);
            modelBuilder.Entity<Lesson>().HasQueryFilter(l => !l.IsDeleted);

            // Configure Relationships
            modelBuilder.Entity<Course>()
                .HasMany(c => c.Lessons)
                .WithOne(l => l.Course)
                .HasForeignKey(l => l.CourseId)
                .OnDelete(DeleteBehavior.Restrict);

            // Unique Index for Lesson Order per Course (Partial Index for Soft Delete)
            modelBuilder.Entity<Lesson>()
                .HasIndex(l => new { l.CourseId, l.Order })
                .IsUnique()
                .HasFilter("\"IsDeleted\" = false");
        }
    }
}
