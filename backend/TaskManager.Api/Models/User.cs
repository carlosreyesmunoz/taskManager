using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TaskManager.Api.Models;

public class User
{
    [Key]
    [StringLength(50)]
    public string Id { get; set; } = Guid.NewGuid().ToString();

    [Required]
    [StringLength(255)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [StringLength(255)]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    [StringLength(20)]
    public string Role { get; set; } = "user"; // 'admin' or 'user'

    [StringLength(50)]
    public string? OrganizationId { get; set; }

    public int Points { get; set; } = 0;

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public Organization? Organization { get; set; }
    public ICollection<TaskEntity> CreatedTasks { get; set; } = new List<TaskEntity>();
    public ICollection<TaskEntity> AssignedTasks { get; set; } = new List<TaskEntity>();
    public ICollection<UserInvitation> SentInvitations { get; set; } = new List<UserInvitation>();
    public ICollection<TaskHistory> TaskHistories { get; set; } = new List<TaskHistory>();
    public ICollection<Organization> OwnedOrganizations { get; set; } = new List<Organization>();
}