using Microsoft.EntityFrameworkCore;
using TaskManager.Api.Data;
using TaskManager.Api.Models;
using TaskManager.Api.DTOs;

namespace TaskManager.Api.Services;

public class OrganizationService : IOrganizationService
{
    private readonly TaskManagerDbContext _context;

    public OrganizationService(TaskManagerDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Organization>> GetAllAsync()
    {
        return await _context.Organizations
            .Include(o => o.Owner)
            .ToListAsync();
    }

    public async Task<Organization?> GetByIdAsync(string id)
    {
        return await _context.Organizations
            .Include(o => o.Owner)
            .FirstOrDefaultAsync(o => o.Id == id);
    }

    public async Task<Organization> CreateAsync(CreateOrganizationDto dto)
    {
        // Verify the owner exists
        var owner = await _context.Users.FindAsync(dto.OwnerId);
        if (owner == null)
            throw new ArgumentException($"User with ID {dto.OwnerId} not found");

        var organization = new Organization
        {
            Id = Guid.NewGuid().ToString(),
            Name = dto.Name,
            Description = dto.Description,
            OwnerId = dto.OwnerId
        };

        _context.Organizations.Add(organization);
        
        // If the owner doesn't have an organization yet, assign them to this one
        if (owner.OrganizationId == null)
        {
            owner.OrganizationId = organization.Id;
        }
        
        await _context.SaveChangesAsync();

        // Reload with navigation properties
        return await GetByIdAsync(organization.Id) ?? organization;
    }

    public async Task<Organization?> UpdateAsync(string id, UpdateOrganizationDto dto)
    {
        var existingOrg = await _context.Organizations.FindAsync(id);
        if (existingOrg == null) return null;

        if (dto.Name != null)
            existingOrg.Name = dto.Name;

        if (dto.Description != null)
            existingOrg.Description = dto.Description;

        if (dto.OwnerId != null)
        {
            // Verify the new owner exists
            var owner = await _context.Users.FindAsync(dto.OwnerId);
            if (owner == null)
                throw new ArgumentException($"User with ID {dto.OwnerId} not found");
            
            existingOrg.OwnerId = dto.OwnerId;
        }

        existingOrg.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        
        // Reload with navigation properties
        return await GetByIdAsync(id);
    }

    public async Task<bool> DeleteAsync(string id)
    {
        var organization = await _context.Organizations.FindAsync(id);
        if (organization == null) return false;

        _context.Organizations.Remove(organization);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<IEnumerable<User>> GetUsersAsync(string organizationId)
    {
        return await _context.Users
            .Where(u => u.OrganizationId == organizationId)
            .ToListAsync();
    }

    public async Task<IEnumerable<TaskEntity>> GetTasksAsync(string organizationId)
    {
        return await _context.Tasks
            .Where(t => t.OrganizationId == organizationId)
            .Include(t => t.Creator)
            .Include(t => t.Assignee)
            .OrderByDescending(t => t.CreatedAt)
            .ToListAsync();
    }
}