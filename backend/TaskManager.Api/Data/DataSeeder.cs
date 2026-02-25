using Microsoft.EntityFrameworkCore;
using TaskManager.Api.Models;

namespace TaskManager.Api.Data;

public static class DataSeeder
{
    public static async Task SeedAsync(TaskManagerDbContext context)
    {
        // Only seed if no data exists
        if (await context.Organizations.AnyAsync())
            return;

        var now = DateTime.UtcNow;

        // 1. Insert organization first with null OwnerId (circular FK)
        var org = new Organization
        {
            Id = "org-default-001",
            Name = "Default Organization",
            Description = "Initial organization for the TaskManager application",
            OwnerId = null,
            CreatedAt = now,
            UpdatedAt = now
        };
        context.Organizations.Add(org);
        await context.SaveChangesAsync();

        // 2. Insert users
        var adminUser = new User
        {
            Id = "user-admin-001",
            Name = "Admin User",
            Email = "admin@taskmanager.com",
            Role = "admin",
            OrganizationId = org.Id,
            Points = 0,
            IsActive = true,
            CreatedAt = now,
            UpdatedAt = now
        };

        var devUser = new User
        {
            Id = "user-dev-001",
            Name = "Jane Developer",
            Email = "jane@taskmanager.com",
            Role = "user",
            OrganizationId = org.Id,
            Points = 50,
            IsActive = true,
            CreatedAt = now,
            UpdatedAt = now
        };

        var designerUser = new User
        {
            Id = "user-designer-001",
            Name = "John Designer",
            Email = "john@taskmanager.com",
            Role = "user",
            OrganizationId = org.Id,
            Points = 30,
            IsActive = true,
            CreatedAt = now,
            UpdatedAt = now
        };

        context.Users.AddRange(adminUser, devUser, designerUser);
        await context.SaveChangesAsync();

        // 3. Update organization owner now that the admin user exists
        org.OwnerId = adminUser.Id;
        org.UpdatedAt = now;
        await context.SaveChangesAsync();

        // 4. Insert sample tasks
        var tasks = new[]
        {
            new TaskEntity
            {
                Id = "task-001",
                Title = "Set up project repository",
                Description = "Initialize the GitHub repository with README, .gitignore and branch protection rules",
                Status = "completed",
                Assigned = true,
                OrganizationId = org.Id,
                CreatorId = adminUser.Id,
                AssigneeId = adminUser.Id,
                Points = 10,
                CompletedAt = now,
                CreatedAt = now,
                UpdatedAt = now
            },
            new TaskEntity
            {
                Id = "task-002",
                Title = "Design database schema",
                Description = "Define all entities, relationships, and indexes for the task management system",
                Status = "finalized",
                Assigned = true,
                OrganizationId = org.Id,
                CreatorId = adminUser.Id,
                AssigneeId = devUser.Id,
                Points = 20,
                CompletedAt = now,
                FinalizedAt = now,
                CreatedAt = now,
                UpdatedAt = now
            },
            new TaskEntity
            {
                Id = "task-003",
                Title = "Build REST API endpoints",
                Description = "Implement CRUD endpoints for Organizations, Users, Tasks, and Invitations",
                Status = "uncompleted",
                Assigned = true,
                OrganizationId = org.Id,
                CreatorId = adminUser.Id,
                AssigneeId = devUser.Id,
                Points = 30,
                CreatedAt = now,
                UpdatedAt = now
            },
            new TaskEntity
            {
                Id = "task-004",
                Title = "Create UI wireframes",
                Description = "Design wireframes for the main dashboard, task list, and task detail views",
                Status = "uncompleted",
                Assigned = true,
                OrganizationId = org.Id,
                CreatorId = adminUser.Id,
                AssigneeId = designerUser.Id,
                Points = 15,
                CreatedAt = now,
                UpdatedAt = now
            },
            new TaskEntity
            {
                Id = "task-005",
                Title = "Write API documentation",
                Description = "Document all API endpoints with request/response examples in the README",
                Status = "uncompleted",
                Assigned = false,
                OrganizationId = org.Id,
                CreatorId = adminUser.Id,
                AssigneeId = null,
                Points = 10,
                CreatedAt = now,
                UpdatedAt = now
            },
            new TaskEntity
            {
                Id = "task-006",
                Title = "Set up CI/CD pipeline",
                Description = "Configure GitHub Actions for automated build, test and deployment to Azure",
                Status = "uncompleted",
                Assigned = false,
                OrganizationId = org.Id,
                CreatorId = adminUser.Id,
                AssigneeId = null,
                Points = 25,
                CreatedAt = now,
                UpdatedAt = now
            }
        };

        context.Tasks.AddRange(tasks);
        await context.SaveChangesAsync();
    }
}
