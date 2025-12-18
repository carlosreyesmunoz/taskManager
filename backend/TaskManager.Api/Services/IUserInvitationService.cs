using TaskManager.Api.Models;
using TaskManager.Api.DTOs;

namespace TaskManager.Api.Services;

public interface IUserInvitationService
{
    Task<IEnumerable<UserInvitation>> GetAllAsync();
    Task<UserInvitation?> GetByIdAsync(string id);
    Task<UserInvitation?> GetByTokenAsync(string token);
    Task<UserInvitation> CreateAsync(CreateUserInvitationDto dto);
    Task<UserInvitation?> AcceptInvitationAsync(string token, CreateUserDto userDto);
    Task<bool> DeleteAsync(string id);
    Task<IEnumerable<UserInvitation>> GetByOrganizationAsync(string organizationId);
}