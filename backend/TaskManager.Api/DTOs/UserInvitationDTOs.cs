using System.ComponentModel.DataAnnotations;

namespace TaskManager.Api.DTOs;

public class CreateUserInvitationDto
{
    [Required]
    [EmailAddress]
    [StringLength(100)]
    public string Email { get; set; } = string.Empty;

    [Required]
    public string Role { get; set; } = "user";

    [Required]
    [StringLength(50)]
    public string OrganizationId { get; set; } = string.Empty;

    [Required]
    [StringLength(50)]
    public string InvitedById { get; set; } = string.Empty;

    public DateTime? ExpiresAt { get; set; }
}

public class UpdateUserInvitationDto
{
    public string? Status { get; set; }

    public string? Role { get; set; }

    public DateTime? ExpiresAt { get; set; }
}
