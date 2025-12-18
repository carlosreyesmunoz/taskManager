using TaskManager.Api.Models;
using TaskManager.Api.DTOs;

namespace TaskManager.Api.Services;

public interface IOrganizationService
{
    Task<IEnumerable<Organization>> GetAllAsync();
    Task<Organization?> GetByIdAsync(string id);
    Task<Organization> CreateAsync(CreateOrganizationDto dto);
    Task<Organization?> UpdateAsync(string id, UpdateOrganizationDto dto);
    Task<bool> DeleteAsync(string id);
    Task<IEnumerable<User>> GetUsersAsync(string organizationId);
    Task<IEnumerable<TaskEntity>> GetTasksAsync(string organizationId);
}