using System.ComponentModel.DataAnnotations;

namespace TaskManager.Api.DTOs;

public class CreateOrganizationDto
{
    [Required]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;

    [StringLength(500)]
    public string? Description { get; set; }

    [Required]
    [StringLength(50)]
    public string OwnerId { get; set; } = string.Empty;
}

public class UpdateOrganizationDto
{
    [StringLength(100)]
    public string? Name { get; set; }

    [StringLength(500)]
    public string? Description { get; set; }

    [StringLength(50)]
    public string? OwnerId { get; set; }
}
