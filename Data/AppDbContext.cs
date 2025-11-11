using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using NOX_Backend.Models;

public class AppDbContext : IdentityDbContext<ApplicationUser, IdentityRole, string>
{
    public AppDbContext(DbContextOptions<AppDbContext> options) :
        base(options)
    { }

    /// <summary>
    /// DbSet for Department entities.
    /// </summary>
    public DbSet<Department> Departments { get; set; } = null!;

    /// <summary>
    /// Configures entity relationships, constraints, and indexes.
    /// </summary>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure Department entity
        modelBuilder.Entity<Department>(entity =>
        {
            entity.HasKey(d => d.Id);

            entity.Property(d => d.Name)
                .IsRequired()
                .HasMaxLength(100);

            entity.Property(d => d.Description)
                .HasMaxLength(500);

            entity.Property(d => d.CreatedAt)
                .HasDefaultValueSql("GETUTCDATE()");

            // Unique constraint on department name
            entity.HasIndex(d => d.Name)
                .IsUnique()
                .HasDatabaseName("IX_Departments_Name_Unique");

            // Configure Manager relationship (Department -> Manager)
            // A department can have one manager (optional), and a manager manages one department
            entity.HasOne(d => d.Manager)
                .WithOne(u => u.ManagedDepartment)
                .HasForeignKey<Department>(d => d.ManagerId)
                .OnDelete(DeleteBehavior.SetNull) // If manager is deleted, set ManagerId to null
                .HasConstraintName("FK_Departments_Manager");
        });

        // Configure ApplicationUser-Department relationship
        modelBuilder.Entity<ApplicationUser>(entity =>
        {
            entity.Property(u => u.CreatedAt)
                .HasDefaultValueSql("GETUTCDATE()");

            // Configure the relationship: User -> Department
            // A user belongs to exactly one department, and a department has zero to many users
            entity.HasOne(u => u.Department)
                .WithMany(d => d.Users)
                .HasForeignKey(u => u.DepartmentId)
                .OnDelete(DeleteBehavior.Restrict) // Prevent deletion of departments with users
                .HasConstraintName("FK_AspNetUsers_Department");
        });
    }
}