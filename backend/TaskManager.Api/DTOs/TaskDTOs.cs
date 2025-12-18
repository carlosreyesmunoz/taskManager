using System.ComponentModel.DataAnnotations;

namespace TaskManager.Api.DTOs;

public class CreateTaskDto
{
    [Required]
    [StringLength(200)]
    public string Title { get; set; } = string.Empty;

    [StringLength(1000)]
    public string? Description { get; set; }

    public DateTime? DueDate { get; set; }

    [Required]
    [StringLength(50)]
    public string OrganizationId { get; set; } = string.Empty;

    [Required]
    [StringLength(50)]
    public string CreatedById { get; set; } = string.Empty;

    [StringLength(50)]
    public string? AssignedToId { get; set; }
}

public class UpdateTaskDto
{
    [StringLength(200)]
    public string? Title { get; set; }

    [StringLength(1000)]
    public string? Description { get; set; }

    public DateTime? DueDate { get; set; }

    [StringLength(50)]
    public string? AssignedToId { get; set; }
}

public class AssignTaskDto
{
    [Required]
    [StringLength(50)]
    public string UserId { get; set; } = string.Empty;
}
