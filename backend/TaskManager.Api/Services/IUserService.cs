using TaskManager.Api.Models;
using TaskManager.Api.DTOs;

namespace TaskManager.Api.Services;

public interface IUserService
{
    Task<IEnumerable<User>> GetAllAsync();
    Task<User?> GetByIdAsync(string id);
    Task<User?> GetByEmailAsync(string email);
    Task<User> CreateAsync(CreateUserDto dto);
    Task<User?> UpdateAsync(string id, UpdateUserDto dto);
    Task<bool> DeleteAsync(string id);
    Task<IEnumerable<User>> GetByOrganizationAsync(string organizationId);
    Task<bool> UpdatePointsAsync(string id, int points);
}