using System.ComponentModel.DataAnnotations;

namespace TaskManager.Api.DTOs;

public class CreateUserDto
{
    [Required]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    [StringLength(100)]
    public string Email { get; set; } = string.Empty;

    [Required]
    public string Role { get; set; } = "user";

    [StringLength(50)]
    public string? OrganizationId { get; set; }

    public bool IsActive { get; set; } = true;
}

public class UpdateUserDto
{
    [StringLength(100)]
    public string? Name { get; set; }

    [EmailAddress]
    [StringLength(100)]
    public string? Email { get; set; }

    public string? Role { get; set; }

    [StringLength(50)]
    public string? OrganizationId { get; set; }

    public int? Points { get; set; }

    public bool? IsActive { get; set; }
}
