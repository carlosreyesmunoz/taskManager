using Microsoft.EntityFrameworkCore;
using TaskManager.Api.Data;
using TaskManager.Api.Models;
using TaskManager.Api.DTOs;

namespace TaskManager.Api.Services;

public class UserInvitationService : IUserInvitationService
{
    private readonly TaskManagerDbContext _context;
    private readonly IUserService _userService;

    public UserInvitationService(TaskManagerDbContext context, IUserService userService)
    {
        _context = context;
        _userService = userService;
    }

    public async Task<IEnumerable<UserInvitation>> GetAllAsync()
    {
        return await _context.UserInvitations
            .Include(i => i.Organization)
            .Include(i => i.Inviter)
            .Where(i => i.AcceptedAt == null && i.ExpiresAt > DateTime.UtcNow)
            .ToListAsync();
    }

    public async Task<UserInvitation?> GetByIdAsync(string id)
    {
        return await _context.UserInvitations
            .Include(i => i.Organization)
            .Include(i => i.Inviter)
            .FirstOrDefaultAsync(i => i.Id == id);
    }

    public async Task<UserInvitation?> GetByTokenAsync(string token)
    {
        return await _context.UserInvitations
            .Include(i => i.Organization)
            .Include(i => i.Inviter)
            .FirstOrDefaultAsync(i => i.Token == token && i.AcceptedAt == null && i.ExpiresAt > DateTime.UtcNow);
    }

    public async Task<UserInvitation> CreateAsync(CreateUserInvitationDto dto)
    {
        // Verify organization exists
        var organization = await _context.Organizations.FindAsync(dto.OrganizationId);
        if (organization == null)
            throw new ArgumentException($"Organization with ID {dto.OrganizationId} not found");

        // Verify inviter exists
        var inviter = await _context.Users.FindAsync(dto.InvitedById);
        if (inviter == null)
            throw new ArgumentException($"User with ID {dto.InvitedById} not found");

        // Check if there's already an active invitation for this email in the organization
        var existingInvitation = await _context.UserInvitations
            .FirstOrDefaultAsync(i => i.OrganizationId == dto.OrganizationId 
                                    && i.Email == dto.Email 
                                    && i.AcceptedAt == null 
                                    && i.ExpiresAt > DateTime.UtcNow);

        if (existingInvitation != null)
        {
            // Update the existing invitation
            existingInvitation.Role = dto.Role;
            existingInvitation.ExpiresAt = dto.ExpiresAt ?? DateTime.UtcNow.AddDays(7);
            existingInvitation.Token = Guid.NewGuid().ToString();
            await _context.SaveChangesAsync();
            return existingInvitation;
        }

        var invitation = new UserInvitation
        {
            Id = Guid.NewGuid().ToString(),
            Email = dto.Email,
            Role = dto.Role,
            OrganizationId = dto.OrganizationId,
            InviterId = dto.InvitedById,
            Token = Guid.NewGuid().ToString(),
            ExpiresAt = dto.ExpiresAt ?? DateTime.UtcNow.AddDays(7)
        };

        _context.UserInvitations.Add(invitation);
        await _context.SaveChangesAsync();
        return invitation;
    }

    public async Task<UserInvitation?> AcceptInvitationAsync(string token, CreateUserDto userDto)
    {
        using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            var invitation = await GetByTokenAsync(token);
            if (invitation == null) return null;

            // Check if user already exists with this email
            var existingUser = await _userService.GetByEmailAsync(invitation.Email);
            if (existingUser != null) return null;

            // Create the user with data from invitation
            var newUserDto = new CreateUserDto
            {
                Email = invitation.Email,
                Name = userDto.Name,
                Role = invitation.Role,
                OrganizationId = invitation.OrganizationId,
                IsActive = true
            };
            
            await _userService.CreateAsync(newUserDto);

            // Mark invitation as accepted
            invitation.AcceptedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            await transaction.CommitAsync();
            return invitation;
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    public async Task<bool> DeleteAsync(string id)
    {
        var invitation = await _context.UserInvitations.FindAsync(id);
        if (invitation == null) return false;

        _context.UserInvitations.Remove(invitation);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<IEnumerable<UserInvitation>> GetByOrganizationAsync(string organizationId)
    {
        return await _context.UserInvitations
            .Where(i => i.OrganizationId == organizationId)
            .Include(i => i.Inviter)
            .OrderByDescending(i => i.CreatedAt)
            .ToListAsync();
    }
}