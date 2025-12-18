using Microsoft.EntityFrameworkCore;
using TaskManager.Api.Data;
using TaskManager.Api.Models;
using TaskManager.Api.DTOs;

namespace TaskManager.Api.Services;

public class UserService : IUserService
{
    private readonly TaskManagerDbContext _context;

    public UserService(TaskManagerDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<User>> GetAllAsync()
    {
        return await _context.Users
            .Include(u => u.Organization)
            .Where(u => u.IsActive)
            .ToListAsync();
    }

    public async Task<User?> GetByIdAsync(string id)
    {
        return await _context.Users
            .Include(u => u.Organization)
            .FirstOrDefaultAsync(u => u.Id == id && u.IsActive);
    }

    public async Task<User?> GetByEmailAsync(string email)
    {
        return await _context.Users
            .Include(u => u.Organization)
            .FirstOrDefaultAsync(u => u.Email == email && u.IsActive);
    }

    public async Task<User> CreateAsync(CreateUserDto dto)
    {
        // Verify the organization exists if provided
        if (dto.OrganizationId != null)
        {
            var organization = await _context.Organizations.FindAsync(dto.OrganizationId);
            if (organization == null)
                throw new ArgumentException($"Organization with ID {dto.OrganizationId} not found");
        }

        // Check if email already exists
        var existingUser = await _context.Users.FirstOrDefaultAsync(u => u.Email == dto.Email);
        if (existingUser != null)
            throw new ArgumentException($"User with email {dto.Email} already exists");

        var user = new User
        {
            Id = Guid.NewGuid().ToString(),
            Name = dto.Name,
            Email = dto.Email,
            Role = dto.Role,
            OrganizationId = dto.OrganizationId,
            IsActive = dto.IsActive,
            Points = 0
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        // Reload with navigation properties
        return await GetByIdAsync(user.Id) ?? user;
    }

    public async Task<User?> UpdateAsync(string id, UpdateUserDto dto)
    {
        var existingUser = await _context.Users.FindAsync(id);
        if (existingUser == null || !existingUser.IsActive) return null;

        if (dto.Name != null)
            existingUser.Name = dto.Name;

        if (dto.Email != null)
        {
            // Check if email already exists for another user
            var emailExists = await _context.Users
                .AnyAsync(u => u.Email == dto.Email && u.Id != id);
            if (emailExists)
                throw new ArgumentException($"User with email {dto.Email} already exists");
            
            existingUser.Email = dto.Email;
        }

        if (dto.Role != null)
            existingUser.Role = dto.Role;

        if (dto.OrganizationId != null)
        {
            // Verify the new organization exists
            var organization = await _context.Organizations.FindAsync(dto.OrganizationId);
            if (organization == null)
                throw new ArgumentException($"Organization with ID {dto.OrganizationId} not found");
            
            existingUser.OrganizationId = dto.OrganizationId;
        }

        if (dto.Points.HasValue)
            existingUser.Points = dto.Points.Value;

        if (dto.IsActive.HasValue)
            existingUser.IsActive = dto.IsActive.Value;

        existingUser.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        
        // Reload with navigation properties
        return await GetByIdAsync(id);
    }

    public async Task<bool> DeleteAsync(string id)
    {
        var user = await _context.Users.FindAsync(id);
        if (user == null) return false;

        user.IsActive = false;
        user.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<IEnumerable<User>> GetByOrganizationAsync(string organizationId)
    {
        return await _context.Users
            .Where(u => u.OrganizationId == organizationId && u.IsActive)
            .OrderBy(u => u.Name)
            .ToListAsync();
    }

    public async Task<bool> UpdatePointsAsync(string id, int points)
    {
        var user = await _context.Users.FindAsync(id);
        if (user == null || !user.IsActive) return false;

        user.Points += points;
        user.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        return true;
    }
}