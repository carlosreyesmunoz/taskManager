using Microsoft.EntityFrameworkCore;
using TaskManager.Api.Data;
using TaskManager.Api.Models;
using TaskManager.Api.DTOs;

namespace TaskManager.Api.Services;

public class TaskService : ITaskService
{
    private readonly TaskManagerDbContext _context;
    private readonly IUserService _userService;

    public TaskService(TaskManagerDbContext context, IUserService userService)
    {
        _context = context;
        _userService = userService;
    }

    public async Task<IEnumerable<TaskEntity>> GetAllAsync()
    {
        return await _context.Tasks
            .Include(t => t.Creator)
            .Include(t => t.Assignee)
            .Include(t => t.Organization)
            .OrderByDescending(t => t.CreatedAt)
            .ToListAsync();
    }

    public async Task<TaskEntity?> GetByIdAsync(string id)
    {
        return await _context.Tasks
            .Include(t => t.Creator)
            .Include(t => t.Assignee)
            .Include(t => t.Organization)
            .FirstOrDefaultAsync(t => t.Id == id);
    }

    public async Task<TaskEntity> CreateAsync(CreateTaskDto dto)
    {
        using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            // Verify organization exists
            var organization = await _context.Organizations.FindAsync(dto.OrganizationId);
            if (organization == null)
                throw new ArgumentException($"Organization with ID {dto.OrganizationId} not found");

            // Verify creator exists
            var creator = await _context.Users.FindAsync(dto.CreatedById);
            if (creator == null)
                throw new ArgumentException($"User with ID {dto.CreatedById} not found");

            // Verify assignee exists if provided
            if (dto.AssignedToId != null)
            {
                var assignee = await _context.Users.FindAsync(dto.AssignedToId);
                if (assignee == null)
                    throw new ArgumentException($"User with ID {dto.AssignedToId} not found");
            }

            var task = new TaskEntity
            {
                Id = Guid.NewGuid().ToString(),
                Title = dto.Title,
                Description = dto.Description,
                DueDate = dto.DueDate,
                OrganizationId = dto.OrganizationId,
                CreatorId = dto.CreatedById,
                AssigneeId = dto.AssignedToId,
                Assigned = dto.AssignedToId != null,
                Status = "uncompleted",
                Points = 0
            };

            _context.Tasks.Add(task);
            await _context.SaveChangesAsync();

            // Add task history entry
            var history = new TaskHistory
            {
                TaskId = task.Id,
                UserId = task.CreatorId,
                Action = "created",
                NewStatus = task.Status,
                Notes = "Task created"
            };
            _context.TaskHistories.Add(history);
            await _context.SaveChangesAsync();

            await transaction.CommitAsync();
            
            // Reload with navigation properties
            return await GetByIdAsync(task.Id) ?? task;
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    public async Task<TaskEntity?> UpdateAsync(string id, UpdateTaskDto dto)
    {
        var existingTask = await _context.Tasks.FindAsync(id);
        if (existingTask == null) return null;

        if (dto.Title != null)
            existingTask.Title = dto.Title;

        if (dto.Description != null)
            existingTask.Description = dto.Description;

        if (dto.DueDate.HasValue)
            existingTask.DueDate = dto.DueDate;

        if (dto.AssignedToId != null)
        {
            // Verify assignee exists
            var assignee = await _context.Users.FindAsync(dto.AssignedToId);
            if (assignee == null)
                throw new ArgumentException($"User with ID {dto.AssignedToId} not found");
            
            existingTask.AssigneeId = dto.AssignedToId;
            existingTask.Assigned = true;
        }

        existingTask.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        
        // Reload with navigation properties
        return await GetByIdAsync(id);
    }

    public async Task<bool> DeleteAsync(string id)
    {
        var task = await _context.Tasks.FindAsync(id);
        if (task == null) return false;

        _context.Tasks.Remove(task);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<IEnumerable<TaskEntity>> GetByOrganizationAsync(string organizationId)
    {
        return await _context.Tasks
            .Where(t => t.OrganizationId == organizationId)
            .Include(t => t.Creator)
            .Include(t => t.Assignee)
            .OrderByDescending(t => t.CreatedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<TaskEntity>> GetTaskPoolAsync(string organizationId)
    {
        return await _context.Tasks
            .Where(t => t.OrganizationId == organizationId && !t.Assigned && t.Status == "uncompleted")
            .Include(t => t.Creator)
            .OrderByDescending(t => t.CreatedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<TaskEntity>> GetUserTasksAsync(string userId)
    {
        return await _context.Tasks
            .Where(t => t.AssigneeId == userId)
            .Include(t => t.Creator)
            .Include(t => t.Organization)
            .OrderByDescending(t => t.CreatedAt)
            .ToListAsync();
    }

    public async Task<TaskEntity?> AssignTaskAsync(string taskId, string userId)
    {
        using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            var task = await _context.Tasks.FindAsync(taskId);
            if (task == null || task.Status != "uncompleted") return null;

            var previousAssigneeId = task.AssigneeId;
            task.AssigneeId = userId;
            task.Assigned = true;
            task.UpdatedAt = DateTime.UtcNow;

            var history = new TaskHistory
            {
                TaskId = taskId,
                UserId = userId,
                Action = "assigned",
                PreviousAssigneeId = previousAssigneeId,
                NewAssigneeId = userId,
                Notes = "Task assigned"
            };
            _context.TaskHistories.Add(history);

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();
            return task;
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    public async Task<TaskEntity?> PickTaskAsync(string taskId, string userId)
    {
        using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            var task = await _context.Tasks.FindAsync(taskId);
            if (task == null || task.Assigned || task.Status != "uncompleted") return null;

            task.AssigneeId = userId;
            task.Assigned = true;
            task.UpdatedAt = DateTime.UtcNow;

            var history = new TaskHistory
            {
                TaskId = taskId,
                UserId = userId,
                Action = "picked",
                NewAssigneeId = userId,
                Notes = "Task picked from pool"
            };
            _context.TaskHistories.Add(history);

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();
            return task;
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    public async Task<TaskEntity?> CompleteTaskAsync(string taskId, string userId)
    {
        using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            var task = await _context.Tasks.FindAsync(taskId);
            if (task == null || task.AssigneeId != userId || task.Status != "uncompleted") return null;

            var previousStatus = task.Status;
            task.Status = "completed";
            task.CompletedAt = DateTime.UtcNow;
            task.UpdatedAt = DateTime.UtcNow;

            var history = new TaskHistory
            {
                TaskId = taskId,
                UserId = userId,
                Action = "completed",
                PreviousStatus = previousStatus,
                NewStatus = "completed",
                Notes = "Task marked as completed"
            };
            _context.TaskHistories.Add(history);

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();
            return task;
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    public async Task<TaskEntity?> FinalizeTaskAsync(string taskId, string userId)
    {
        using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            var task = await _context.Tasks.FindAsync(taskId);
            if (task == null || task.Status != "completed") return null;

            var previousStatus = task.Status;
            task.Status = "finalized";
            task.FinalizedAt = DateTime.UtcNow;
            task.UpdatedAt = DateTime.UtcNow;

            var history = new TaskHistory
            {
                TaskId = taskId,
                UserId = userId,
                Action = "finalized",
                PreviousStatus = previousStatus,
                NewStatus = "finalized",
                Notes = "Task finalized and points awarded"
            };
            _context.TaskHistories.Add(history);

            // Award points to the assignee
            if (task.AssigneeId != null && task.Points > 0)
            {
                await _userService.UpdatePointsAsync(task.AssigneeId, task.Points);
            }

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();
            return task;
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    public async Task<IEnumerable<TaskHistory>> GetTaskHistoryAsync(string taskId)
    {
        return await _context.TaskHistories
            .Where(h => h.TaskId == taskId)
            .Include(h => h.User)
            .Include(h => h.PreviousAssignee)
            .Include(h => h.NewAssignee)
            .OrderByDescending(h => h.CreatedAt)
            .ToListAsync();
    }
}