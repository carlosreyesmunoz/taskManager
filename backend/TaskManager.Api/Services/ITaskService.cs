using TaskManager.Api.Models;
using TaskManager.Api.DTOs;

namespace TaskManager.Api.Services;

public interface ITaskService
{
    Task<IEnumerable<TaskEntity>> GetAllAsync();
    Task<TaskEntity?> GetByIdAsync(string id);
    Task<TaskEntity> CreateAsync(CreateTaskDto dto);
    Task<TaskEntity?> UpdateAsync(string id, UpdateTaskDto dto);
    Task<bool> DeleteAsync(string id);
    Task<IEnumerable<TaskEntity>> GetByOrganizationAsync(string organizationId);
    Task<IEnumerable<TaskEntity>> GetTaskPoolAsync(string organizationId);
    Task<IEnumerable<TaskEntity>> GetUserTasksAsync(string userId);
    Task<TaskEntity?> AssignTaskAsync(string taskId, string userId);
    Task<TaskEntity?> PickTaskAsync(string taskId, string userId);
    Task<TaskEntity?> CompleteTaskAsync(string taskId, string userId);
    Task<TaskEntity?> FinalizeTaskAsync(string taskId, string userId);
    Task<IEnumerable<TaskHistory>> GetTaskHistoryAsync(string taskId);
}