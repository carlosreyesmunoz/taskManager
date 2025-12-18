using Microsoft.EntityFrameworkCore;
using TaskManager.Api.Models;

namespace TaskManager.Api.Data;

public class TaskManagerDbContext : DbContext
{
    public TaskManagerDbContext(DbContextOptions<TaskManagerDbContext> options) : base(options)
    {
    }

    public DbSet<Organization> Organizations { get; set; }
    public DbSet<User> Users { get; set; }
    public DbSet<TaskEntity> Tasks { get; set; }
    public DbSet<UserInvitation> UserInvitations { get; set; }
    public DbSet<TaskHistory> TaskHistories { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Organization configuration
        modelBuilder.Entity<Organization>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.Name);
            
            // Organization -> Owner relationship
            entity.HasOne(o => o.Owner)
                .WithMany(u => u.OwnedOrganizations)
                .HasForeignKey(o => o.OwnerId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // User configuration
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.Email).IsUnique();
            entity.HasIndex(e => new { e.OrganizationId, e.Role });

            // User -> Organization relationship
            entity.HasOne(u => u.Organization)
                .WithMany(o => o.Users)
                .HasForeignKey(u => u.OrganizationId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Task configuration
        modelBuilder.Entity<TaskEntity>(entity =>
        {
            entity.ToTable("Tasks");
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => new { e.OrganizationId, e.Status });
            entity.HasIndex(e => new { e.AssigneeId, e.Status });
            entity.HasIndex(e => e.CreatorId);
            entity.HasIndex(e => e.DueDate);

            // Task -> Creator relationship
            entity.HasOne(t => t.Creator)
                .WithMany(u => u.CreatedTasks)
                .HasForeignKey(t => t.CreatorId)
                .OnDelete(DeleteBehavior.Restrict);

            // Task -> Assignee relationship (nullable)
            entity.HasOne(t => t.Assignee)
                .WithMany(u => u.AssignedTasks)
                .HasForeignKey(t => t.AssigneeId)
                .OnDelete(DeleteBehavior.SetNull);

            // Task -> Organization relationship
            entity.HasOne(t => t.Organization)
                .WithMany(o => o.Tasks)
                .HasForeignKey(t => t.OrganizationId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // UserInvitation configuration
        modelBuilder.Entity<UserInvitation>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.Token).IsUnique();
            entity.HasIndex(e => new { e.OrganizationId, e.Email });

            // UserInvitation -> Organization relationship
            entity.HasOne(ui => ui.Organization)
                .WithMany(o => o.UserInvitations)
                .HasForeignKey(ui => ui.OrganizationId)
                .OnDelete(DeleteBehavior.Cascade);

            // UserInvitation -> Inviter relationship
            entity.HasOne(ui => ui.Inviter)
                .WithMany(u => u.SentInvitations)
                .HasForeignKey(ui => ui.InviterId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // TaskHistory configuration
        modelBuilder.Entity<TaskHistory>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.TaskId);
            entity.HasIndex(e => new { e.UserId, e.CreatedAt });

            // TaskHistory -> Task relationship
            entity.HasOne(th => th.Task)
                .WithMany(t => t.TaskHistories)
                .HasForeignKey(th => th.TaskId)
                .OnDelete(DeleteBehavior.Cascade);

            // TaskHistory -> User relationship
            entity.HasOne(th => th.User)
                .WithMany(u => u.TaskHistories)
                .HasForeignKey(th => th.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            // TaskHistory -> PreviousAssignee relationship (nullable)
            entity.HasOne(th => th.PreviousAssignee)
                .WithMany()
                .HasForeignKey(th => th.PreviousAssigneeId)
                .OnDelete(DeleteBehavior.SetNull);

            // TaskHistory -> NewAssignee relationship (nullable)
            entity.HasOne(th => th.NewAssignee)
                .WithMany()
                .HasForeignKey(th => th.NewAssigneeId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        base.OnModelCreating(modelBuilder);
    }

    public override int SaveChanges()
    {
        UpdateTimestamps();
        return base.SaveChanges();
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        UpdateTimestamps();
        return base.SaveChangesAsync(cancellationToken);
    }

    private void UpdateTimestamps()
    {
        var entries = ChangeTracker.Entries()
            .Where(e => e.Entity is Organization || e.Entity is User || e.Entity is TaskEntity)
            .Where(e => e.State == EntityState.Added || e.State == EntityState.Modified);

        foreach (var entry in entries)
        {
            if (entry.State == EntityState.Added)
            {
                if (entry.Entity is Organization org)
                {
                    org.CreatedAt = DateTime.UtcNow;
                    org.UpdatedAt = DateTime.UtcNow;
                }
                else if (entry.Entity is User user)
                {
                    user.CreatedAt = DateTime.UtcNow;
                    user.UpdatedAt = DateTime.UtcNow;
                }
                else if (entry.Entity is TaskEntity task)
                {
                    task.CreatedAt = DateTime.UtcNow;
                    task.UpdatedAt = DateTime.UtcNow;
                }
            }
            else if (entry.State == EntityState.Modified)
            {
                if (entry.Entity is Organization org)
                    org.UpdatedAt = DateTime.UtcNow;
                else if (entry.Entity is User user)
                    user.UpdatedAt = DateTime.UtcNow;
                else if (entry.Entity is TaskEntity task)
                    task.UpdatedAt = DateTime.UtcNow;
            }
        }
    }
}