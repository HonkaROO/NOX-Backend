using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using NOX_Backend.Models;
using NOX_Backend.Models.Onboarding;

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
    /// DbSet for OnboardingFolder entities.
    /// </summary>
    public DbSet<OnboardingFolder> OnboardingFolders { get; set; } = null!;

    /// <summary>
    /// DbSet for OnboardingTask entities.
    /// </summary>
    public DbSet<OnboardingTask> OnboardingTasks { get; set; } = null!;

    /// <summary>
    /// DbSet for OnboardingMaterial entities.
    /// </summary>
    public DbSet<OnboardingMaterial> OnboardingMaterials { get; set; } = null!;

    /// <summary>
    /// DbSet for OnboardingSteps entities.
    /// </summary>
    public DbSet<OnboardingSteps> OnboardingSteps { get; set; } = null!;

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

        // Configure OnboardingFolder entity
        modelBuilder.Entity<OnboardingFolder>(entity =>
        {
            entity.HasKey(f => f.Id);

            entity.Property(f => f.Title)
                .IsRequired()
                .HasMaxLength(255);

            entity.Property(f => f.Description)
                .IsRequired()
                .HasMaxLength(1000);

            entity.Property(f => f.CreatedAt)
                .HasDefaultValueSql("GETUTCDATE()");

            // Unique constraint on title
            entity.HasIndex(f => f.Title)
                .IsUnique()
                .HasDatabaseName("IX_OnboardingFolders_Title_Unique");

            // Configure Folder -> Tasks relationship (one-to-many)
            entity.HasMany(f => f.Tasks)
                .WithOne(t => t.Folder)
                .HasForeignKey(t => t.FolderId)
                .OnDelete(DeleteBehavior.Cascade) // Delete tasks when folder is deleted
                .HasConstraintName("FK_OnboardingTasks_OnboardingFolder");
        });

        // Configure OnboardingTask entity
        modelBuilder.Entity<OnboardingTask>(entity =>
        {
            entity.HasKey(t => t.Id);

            entity.Property(t => t.Title)
                .IsRequired()
                .HasMaxLength(255);

            entity.Property(t => t.Description)
                .IsRequired()
                .HasMaxLength(1000);

            entity.Property(t => t.CreatedAt)
                .HasDefaultValueSql("GETUTCDATE()");

            // Foreign key constraint for Folder
            entity.HasOne(t => t.Folder)
                .WithMany(f => f.Tasks)
                .HasForeignKey(t => t.FolderId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK_OnboardingTasks_OnboardingFolder");

            // Configure Task -> Materials relationship (one-to-many)
            entity.HasMany(t => t.Materials)
                .WithOne(m => m.Task)
                .HasForeignKey(m => m.TaskId)
                .OnDelete(DeleteBehavior.Cascade) // Delete materials when task is deleted
                .HasConstraintName("FK_OnboardingMaterials_OnboardingTask");

            // Configure Task -> Steps relationship (one-to-many)
            entity.HasMany(t => t.Steps)
                .WithOne(s => s.Task)
                .HasForeignKey(s => s.TaskId)
                .OnDelete(DeleteBehavior.Cascade) // Delete steps when task is deleted
                .HasConstraintName("FK_OnboardingSteps_OnboardingTask");
        });

        // Configure OnboardingMaterial entity
        modelBuilder.Entity<OnboardingMaterial>(entity =>
        {
            entity.HasKey(m => m.Id);

            entity.Property(m => m.FileName)
                .IsRequired()
                .HasMaxLength(255);

            entity.Property(m => m.FileType)
                .IsRequired()
                .HasMaxLength(50);

            entity.Property(m => m.Url)
                .IsRequired()
                .HasMaxLength(500);

            entity.Property(m => m.CreatedAt)
                .HasDefaultValueSql("GETUTCDATE()");

            // Foreign key constraint for Task
            entity.HasOne(m => m.Task)
                .WithMany(t => t.Materials)
                .HasForeignKey(m => m.TaskId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK_OnboardingMaterials_OnboardingTask");
        });

        // Configure OnboardingSteps entity
        modelBuilder.Entity<OnboardingSteps>(entity =>
        {
            entity.HasKey(s => s.Id);

            entity.Property(s => s.StepDescription)
                .IsRequired()
                .HasMaxLength(1000);

            entity.Property(s => s.CreatedAt)
                .HasDefaultValueSql("GETUTCDATE()");

            // Foreign key constraint for Task
            entity.HasOne(s => s.Task)
                .WithMany(t => t.Steps)
                .HasForeignKey(s => s.TaskId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK_OnboardingSteps_OnboardingTask");
        });
    }
}