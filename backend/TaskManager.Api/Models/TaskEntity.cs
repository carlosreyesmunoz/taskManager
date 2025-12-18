using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TaskManager.Api.Models;

public class TaskEntity
{
    [Key]
    [StringLength(50)]
    public string Id { get; set; } = Guid.NewGuid().ToString();

    [Required]
    [StringLength(255)]
    public string Title { get; set; } = string.Empty;

    public string? Description { get; set; }

    [Required]
    [StringLength(50)]
    public string CreatorId { get; set; } = string.Empty;

    [StringLength(50)]
    public string? AssigneeId { get; set; } // null when in task pool

    [Required]
    [StringLength(50)]
    public string OrganizationId { get; set; } = string.Empty;

    public int Points { get; set; } = 0;

    public bool Assigned { get; set; } = false;

    [Required]
    [StringLength(20)]
    public string Status { get; set; } = "uncompleted"; // 'uncompleted', 'completed', 'finalized'

    public DateTime? DueDate { get; set; }
    public DateTime? CompletedAt { get; set; }
    public DateTime? FinalizedAt { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public User? Creator { get; set; }
    public User? Assignee { get; set; }
    public Organization? Organization { get; set; }
    public ICollection<TaskHistory> TaskHistories { get; set; } = new List<TaskHistory>();
}