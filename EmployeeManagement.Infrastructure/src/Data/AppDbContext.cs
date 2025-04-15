using EmployeeManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace EmployeeManagement.Infrastructure.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        { }
        public DbSet<User> Users { get; set; }

        public DbSet<Employee> Employees { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Employee>()
                .HasQueryFilter(e => !e.IsDeleted);

            modelBuilder.Entity<Employee>()
                .HasIndex(e => e.FullName);

            modelBuilder.Entity<Employee>()
                .HasIndex(e => e.Department);

            modelBuilder.Entity<Employee>()
                .HasIndex(e => new { e.Salary, e.Department });

            modelBuilder.Entity<Employee>()
                .HasIndex(e => e.CreatedBy);

            modelBuilder.Entity<User>()
                .HasIndex(u => u.Username)
                .IsUnique();
        }
    }
}
