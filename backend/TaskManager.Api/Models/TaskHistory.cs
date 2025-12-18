using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TaskManager.Api.Models;

public class TaskHistory
{
    [Key]
    [StringLength(50)]
    public string Id { get; set; } = Guid.NewGuid().ToString();

    [Required]
    [StringLength(50)]
    public string TaskId { get; set; } = string.Empty;

    [Required]
    [StringLength(50)]
    public string UserId { get; set; } = string.Empty;

    [Required]
    [StringLength(50)]
    public string Action { get; set; } = string.Empty; // 'created', 'assigned', 'picked', 'completed', 'finalized', 'reassigned'

    [StringLength(20)]
    public string? PreviousStatus { get; set; }

    [StringLength(20)]
    public string? NewStatus { get; set; }

    [StringLength(50)]
    public string? PreviousAssigneeId { get; set; }

    [StringLength(50)]
    public string? NewAssigneeId { get; set; }

    public string? Notes { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public TaskEntity? Task { get; set; }
    public User? User { get; set; }
    public User? PreviousAssignee { get; set; }
    public User? NewAssignee { get; set; }
}